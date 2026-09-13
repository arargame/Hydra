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
