using Application.Jwt;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace Application.Jwt.Filters
{
    public class JwtAuthenticationAttribute : Attribute, IAuthenticationFilter
    {
        public string Realm { get; set; }

        public bool AllowMultiple => false;

        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            var request = context.Request;
            var authorization = request.Headers.Authorization;

            if (authorization == null || authorization.Scheme != "Bearer")
                return;

            if (string.IsNullOrEmpty(authorization.Parameter))
            {
                context.ErrorResult = new AuthenticationFailureResult("Missing Jwt Token", request);
                return;
            }

            var token = authorization.Parameter;
            var principal = await AuthenticateJwtToken(token);

            if (principal == null)
                context.ErrorResult = new AuthenticationFailureResult("Invalid token", request);

            else
                context.Principal = principal;
        }

        /// <summary>
        /// Validates JWT token, checking for authentication and valid userId claim.
        /// </summary>
        /// <param name="token">JWT token being validated.</param>
        /// <param name="accountId">Account id to be extracted from the token.</param>
        /// <param name="role">User role to be extracted from the token.</param>
        /// <param name="expiration">Date string associated with token expiration.</param>
        /// <returns>Returns true if token is successsfully validated, false otherwise.</returns>
        private static bool ValidateToken(string token, out int accountId, out string role, out string expiration)
        {
            accountId = -1;
            role = null;
            expiration = null;

            // Checks for valid principal
            var simplePrinciple = JwtManager.GetPrincipal(token);
            if (simplePrinciple == null)
                return false;

            // Checks for valid identity
            if (!(simplePrinciple.Identity is ClaimsIdentity identity))
                return false;

            if (!identity.IsAuthenticated)
                return false;

            // Checks for valid account id
            var accountIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(accountIdClaim?.Value) || !int.TryParse(accountIdClaim.Value, out accountId))
            {
                return false;
            }

            // Gets role from token's identity
            var roleClaim = identity.FindFirst(ClaimTypes.Role);
            role = roleClaim.Value;

            // Gets expiration
            var expirationClaim = identity.FindFirst(ClaimTypes.Expiration);
            expiration = expirationClaim.Value;

            return true;
        }

        protected Task<IPrincipal> AuthenticateJwtToken(string token)
        {

            if (ValidateToken(token, out int accountId, out string role, out string expiration))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, accountId.ToString()),
                    new Claim(ClaimTypes.Role, role),
                    new Claim(ClaimTypes.Expiration, expiration),
                };

                var identity = new ClaimsIdentity(claims, "Jwt");
                IPrincipal user = new ClaimsPrincipal(identity);

                return Task.FromResult(user);
            }

            return Task.FromResult<IPrincipal>(null);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            Challenge(context);
            return Task.FromResult(0);
        }

        private void Challenge(HttpAuthenticationChallengeContext context)
        {
            string parameter = null;

            if (!string.IsNullOrEmpty(Realm))
                parameter = "realm=\"" + Realm + "\"";

            context.ChallengeWith("Bearer", parameter);
        }
    }
}