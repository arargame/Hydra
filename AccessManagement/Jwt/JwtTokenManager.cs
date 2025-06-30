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
        private readonly ICustomConfigurationService _config;
        private readonly string _secretKey;

        public JwtTokenManager(ICustomConfigurationService config)
        {
            _config = config;
            _secretKey = _config.Get("JwtSecretKey", "fallback-secret"); // dev için fallback
        }

        public string GenerateToken(IEnumerable<Claim> claims, TimeSpan? lifetime = null)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "hydra-api",
                audience: "hydra-clients",
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
                    ValidIssuer = "hydra-api",
                    ValidAudience = "hydra-clients",
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
