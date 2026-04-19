using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NashPay.API.Models;

namespace NashPay.API.Helpers
{
    public class JwtTokenGenerator
    {
        private readonly string _key;
        private readonly string _issuer;

        public JwtTokenGenerator(IConfiguration configuration)
        {
            _key = configuration["Jwt:Key"] ?? throw new Exception("JWT Key is missing!");
            _issuer = configuration["Jwt:Issuer"] ?? "NashPayAPI";
        }

        public string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_key);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role), // Using Standard ClaimType
                new Claim("KYCStatus", user.KYCStatus ?? "Pending")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1), 
                Issuer = _issuer,
                Audience = _issuer,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public ClaimsPrincipal? GetPrincipalFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_key);

                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _issuer,
                    ValidateLifetime = true // Production pe ye True hona chahiye
                }, out SecurityToken securityToken);

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}