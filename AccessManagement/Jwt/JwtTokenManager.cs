using Hydra.Services;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.AccessManagement.Jwt
{
    public interface IJwtTokenManager
    {
        string GenerateToken(IEnumerable<Claim> claims, TimeSpan? lifetime = null);
        ClaimsPrincipal? ValidateToken(string token);
    }

    public class JwtTokenManager : IJwtTokenManager
    {
        /// <summary>
        /// Issuer/audience are fixed across every Hydra-based application, so the token-issuing
        /// side and the bearer-validation side cannot drift apart.
        /// </summary>
        public const string Issuer = "hydra-api";
        public const string Audience = "hydra-clients";

        /// <summary>
        /// Config key holding the signing secret. Resolved through ICustomConfigurationService,
        /// so "Secrets:JwtSecretKey" (secret store / environment) wins over a plain appsettings
        /// value — the key should not live in appsettings.json in production.
        /// </summary>
        public const string SecretKeyConfigName = "JwtSecretKey";

        /// <summary>
        /// Development-only fallback used when no secret is configured.
        ///
        /// It is deliberately long: HMAC-SHA256 requires a key of at least 256 bits, and the
        /// previous short fallback ("fallback-secret") made token signing throw at runtime rather
        /// than fall back to anything usable. This value is not a secret and must be overridden
        /// in any real deployment.
        /// </summary>
        public const string DevelopmentFallbackSecret = "hydra-development-only-signing-key-please-override-in-configuration";

        private readonly ICustomConfigurationService _config;
        private readonly string _secretKey;

        public JwtTokenManager(ICustomConfigurationService config)
        {
            _config = config;
            _secretKey = _config.Get(SecretKeyConfigName, DevelopmentFallbackSecret);

            if (string.IsNullOrWhiteSpace(_secretKey))
                _secretKey = DevelopmentFallbackSecret;
        }

        public string GenerateToken(IEnumerable<Claim> claims, TimeSpan? lifetime = null)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromHours(1)),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            try
            {
                var parameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = Issuer,
                    ValidAudience = Audience,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                return tokenHandler.ValidateToken(token, parameters, out _);
            }
            catch
            {
                return null;
            }
        }
    }

}
