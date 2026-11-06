using System;
using System.Security.Cryptography;

namespace Hydra.Services
{
    /// <summary>
    /// Minimal, dependency-free password hashing utility (PBKDF2-HMACSHA256).
    /// Hydra core does not reference Microsoft.AspNetCore.Identity (it is also
    /// consumed by non-web projects such as the RazorClassLibrary), so this
    /// small helper exists instead of pulling in that package.
    ///
    /// Stored format: "{iterations}.{saltBase64}.{hashBase64}"
    ///
    /// Used today by DbInitializer's Default Admin seed (see SeedDefaultAdmin).
    /// SystemUserController.LoginAsync should call Verify() here once the
    /// missing password-check gap is fixed — see Hydra Academy
    /// "05-Access-Management-Story/04-login-to-dashboard-and-gaps.md" and
    /// Tentacle/docs/login-to-dashboard-flow.md, both of which document that
    /// gap independently of this seed.
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltSizeBytes = 16;   // 128 bit
        private const int HashSizeBytes = 32;   // 256 bit
        private const int Iterations = 100_000; // OWASP-ish baseline for PBKDF2-SHA256

        public static string Hash(string password)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));

            var salt = RandomNumberGenerator.GetBytes(SaltSizeBytes);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSizeBytes);

            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        /// <summary>
        /// Is this value already one of our stored hashes, rather than a plaintext password?
        ///
        /// The user-editing screen posts the password through the same field the hash is stored
        /// in, so a value that is already hashed must be left alone and a plaintext one must be
        /// hashed. Without this check, saving a user twice would hash the hash and lock them out.
        /// </summary>
        public static bool IsHashed(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var parts = value.Split('.', 3);

            return parts.Length == 3
                   && int.TryParse(parts[0], out var iterations)
                   && iterations > 0
                   && IsBase64(parts[1])
                   && IsBase64(parts[2]);
        }

        private static bool IsBase64(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            Span<byte> buffer = stackalloc byte[value.Length];

            return Convert.TryFromBase64String(value, buffer, out _);
        }

        public static bool Verify(string password, string? hashedValue)
        {
            if (string.IsNullOrEmpty(hashedValue) || string.IsNullOrEmpty(password))
                return false;

            var parts = hashedValue.Split('.', 3);
            if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
                return false;

            try
            {
                var salt = Convert.FromBase64String(parts[1]);
                var expectedHash = Convert.FromBase64String(parts[2]);

                var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);

                return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
            }
            catch
            {
                // Malformed stored hash (e.g. legacy/plaintext row) -> treat as no match.
                return false;
            }
        }
    }
}
