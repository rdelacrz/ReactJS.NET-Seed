using Microsoft.IdentityModel.Tokens;
using System;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Newtonsoft.Json;

namespace Application.Jwt
{
    /// <summary>
    /// Manages JWT token generation and data extraction.
    /// </summary>
    public static class JwtManager
    {
        private static readonly string Secret = ConfigurationManager.AppSettings.Get("AuthKey");

        /// <summary>
        /// Amounts of time (in minutes) for JWT token to expire (1440 minutes, or 24 hours).
        /// </summary>
        public static int JWT_EXPIRATION_MINUTES
        {
            get
            {
                string configExpirationStr = ConfigurationManager.AppSettings["JWTExpirationMinutes"]?.ToString();
                if (!int.TryParse(configExpirationStr, out int jwtExpirationMinutes))
                {
                    jwtExpirationMinutes = 1440;        // Defaults to 1440 minutes, or 24 hours
                }
                return jwtExpirationMinutes;
            }
        }

        /// <summary>
        /// Generates a JWT token based on the given account data, user permissions, password reset status, and expiration 
        /// time (in minutes).
        /// </summary>
        /// <param name="userId">Id associated with authenticating user.</param>
        /// <param name="expireMinutes">Expiration time of JWT token to be created (in minutes).</param>
        /// <returns>JWT token generated based on the given data parameters.</returns>
        public static string GenerateToken(string userId, int? expireMinutes = null)
        {
            // Defaults expiration minutes to value determined by Web.config file
            if (!expireMinutes.HasValue)
            {
                expireMinutes = JWT_EXPIRATION_MINUTES;
            }

            byte[] symmetricKey = Convert.FromBase64String(Secret);

            // If application has roles associated with users, define them here
            object roleData = null;
            DateTime expiration = DateTime.UtcNow.AddMinutes(Convert.ToInt32(expireMinutes.Value));

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId),
                        new Claim(ClaimTypes.Role, JsonConvert.SerializeObject(roleData)),
                        new Claim(ClaimTypes.Expiration, expiration.ToString()),
                    }
                ),

                Expires = expiration,

                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(symmetricKey),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            // Creates token based on the attributes of the descriptor
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);
            string token = tokenHandler.WriteToken(securityToken);

            return token;
        }

        /// <summary>
        /// Extracts the claims principal from the given JWT token, which contains information such as identity.
        /// </summary>
        /// <param name="token">JWT token.</param>
        /// <returns>Claims principal associated with the JWT token.</returns>
        public static ClaimsPrincipal GetPrincipal(string token)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                if (!(tokenHandler.ReadToken(token) is JwtSecurityToken jwtToken))
                    return null;

                byte[] symmetricKey = Convert.FromBase64String(Secret);

                TokenValidationParameters validationParameters = new TokenValidationParameters()
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(symmetricKey)
                };

                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken securityToken);

                return principal;
            }

            catch (Exception)
            {
                return null;
            }
        }

    }
}