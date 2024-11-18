using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Minimal.DataAccess;
using Minimal.Domain.Core.Administration;
using Minimal.Domain.Core.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Minmal.API.Middlewares
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JwtSettings jwtSettings;

        public JwtMiddleware(RequestDelegate next, JwtSettings jwtSettings)
        {
            _next = next;
            this.jwtSettings = jwtSettings;
        }

        public async Task Invoke(HttpContext context, UserManager<User> userManager, AppDbContext dbContext)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
                await AttachUserToContext(context, userManager, dbContext, token);

            await _next(context);
        }

        private async Task AttachUserToContext(HttpContext context, UserManager<User> userManager, AppDbContext dbContext, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(jwtSettings.Secret!);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                if (jwtToken == null || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token");
                }

                string userId = jwtToken.Claims.First(x => x.Type == "Id").Value;

                // attach user to context on successful jwt validation
                var user = dbContext?.Users.FirstOrDefault(u => u.Id == userId);
                user ??= await userManager?.FindByIdAsync(userId);
                context.Items["User"] = user;
            }
            catch
            {
                // do nothing if jwt validation fails
                // user is not attached to context so request won't have access to secure routes
            }
        }
    }
}
