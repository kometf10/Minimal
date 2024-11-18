using Minimal.Domain.Core.Administration;
using Minimal.Domain.Core.Responces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Minimal.Domain.Core.Settings;
using Minimal.DataAccess;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace Minimal.Services.Core.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> userManager;
        private readonly JwtSettings jwtSettings;
        private readonly AppDbContext dbContext;

        public AuthService(UserManager<User> userManager,
                       JwtSettings jwtSettings,
                       AppDbContext dbContext)
        {
            this.userManager = userManager;
            this.jwtSettings = jwtSettings;
            this.dbContext = dbContext;
        }


        public async Task<AuthResult> Login(LoginRequest loginRequest)
        {
            var email = loginRequest.Email;
            var userName = loginRequest.UserName;
            var password = loginRequest.Password;

            User? user = null;
            if (!string.IsNullOrEmpty(userName))
                user = await userManager.FindByNameAsync(userName);

            if (user == null && !string.IsNullOrEmpty(email))
                user = await userManager.FindByEmailAsync(email);

            if (user != null)
            {
                if (!user.ConfirmedAccount)
                    return new AuthResult { Successed = false, Errors = new List<string>() { "Account Not Active" } };

                var valid = await userManager.CheckPasswordAsync(user, password!);
                if (valid)
                {
                    AuthResult authResult = new AuthResult();

                    //Check If Register Confirmation Setting is on
                    //authResult = ConfirmationCheck(user)!;
                    //if (authResult != null)
                    //    return authResult;
                    //else
                    //    authResult = new AuthResult();

                    var token = await GenerateUserJwtToken(user);
                    var refreshToken = GenerateRefreshToken();
                    var wsToken = await GenerateWSToken(user);

                    //Set Refresh Token To User
                    user.RefreshTokn = refreshToken;
                    user.RefreshTokenExpiryTime = DateTime.Now.AddDays(Convert.ToDouble(jwtSettings.RefreshExpiresInDays));
                    await userManager.UpdateAsync(user);

                    if (token != "")
                    {
                        authResult.Successed = true;
                        authResult.Token = token;
                        authResult.RefreshToken = refreshToken;

                        return authResult;
                    }
                    return new AuthResult() { Successed = false, Errors = new List<string>() { "Faild To Login" } };
                }
            }

            return new AuthResult { Errors = new string[] { "Invalid Login Data" } };
        }

        public async Task<AuthResult> Register(RegisterRequest registerRequest)
        {
            var userName = registerRequest.UserName;
            var pUserName = userName!.Replace(" ", "").ToLower();
            var email = $"{pUserName}@app.com";
            var password = registerRequest.Password;

            AuthResult authResult = new AuthResult();

            //If Registration App Setting is off
            var regSetting = true; //dbContext.AppSettings.Find("Registration");
            if (!regSetting)
            {
                authResult.Successed = false;
                authResult.Errors = new List<string>() { "Registration Is Disabled By Admin" };
                return authResult;
            }

            var existedUser = dbContext.Users.AsNoTracking().FirstOrDefault(u => u.UserName == userName);
            if (existedUser != null)
            {
                return new AuthResult { Successed = false, Errors = new List<string> { "This User Name Address Already Existed" } };
            }

            var newUser = new User { Email = email, UserName = email, ConfirmedAccount = true };

            var createdUser = await userManager.CreateAsync(newUser, password!);

            if (!createdUser.Succeeded)
            {
                return new AuthResult() { Successed = false, Errors = createdUser.Errors.Select(x => x.Description) };
            }

            authResult.Successed = true;
            return authResult;
        }

        public async Task<AuthResult> RefreshToken(RefreshTokenRequest refreshTokenRequest)
        {
            if (refreshTokenRequest == null)
                return new AuthResult { Successed = false, Errors = new List<string>() { "Invalid Client Request" } };

            string userId = GetUserIdFromExpiredToken(refreshTokenRequest.Token!);
            var user = dbContext.Users.Find(userId);

            if (user == null ||
               user.RefreshTokn != refreshTokenRequest.RefreshToken)
            {
                return new AuthResult { Successed = false, Errors = new List<string>() { "Invalid Client Request" } };
            }
            if (user.RefreshTokenExpiryTime < DateTime.Now)
            {
                return new AuthResult { Successed = false, Errors = new List<string>() { "Refresh Token Expired" } };
            }

            var token = await GenerateUserJwtToken(user);
            var refreshtoken = GenerateRefreshToken();
            var wsToken = await GenerateWSToken(user);

            user.RefreshTokn = refreshtoken;
            dbContext.Entry(user).State = EntityState.Modified;
            dbContext.SaveChanges();

            return new AuthResult()
            {
                Successed = true,
                Token = token,
                RefreshToken = user.RefreshTokn
            };
        }


        #region "Private"

        private async Task<string> GenerateUserJwtToken(User user)
        {
            try
            {
                var signingCredentials = GetSigningCredentials();
                var claims = await GetClaims(user);
                var tokenOptions = GenerateTokenOptions(signingCredentials, claims);
                var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

                return token;
            }
            catch (Exception)
            {
                //Log The Exception
                return "";
            }
        }

        private async Task<string> GenerateWSToken(User user)
        {
            try
            {
                var signingCredentials = GetSigningCredentials();
                var claims = await GetBasicClaims(user);
                var tokenOptions = GenerateTokenOptions(signingCredentials, claims);
                var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

                return token;
            }
            catch (Exception)
            {
                //Log The Exception
                return "";
            }
        }

        private SigningCredentials GetSigningCredentials()
        {
            var key = Encoding.UTF8.GetBytes(jwtSettings.Secret!);
            var secret = new SymmetricSecurityKey(key);

            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        private async Task<List<Claim>> GetClaims(User user)
        {
            //Basic Claims
            List<Claim> claims = await GetBasicClaims(user);

            //Role Claims
            var userRoles = dbContext.UserRoles.Where(ur => ur.UserId == user.Id).ToList();

            foreach (var userRole in userRoles)
            {
                var role = dbContext.Roles.Find(userRole.RoleId);
                claims.Add(new Claim(ClaimTypes.Role, role.Name));

                //Role Premission Claims
                var roleClaims = dbContext.RoleClaims.Where(rc => rc.RoleId == role.Id).ToList();
                foreach (var roleClaim in roleClaims)
                    claims.Add(new Claim(roleClaim.ClaimType, roleClaim.ClaimValue));
            }
            return claims;
        }

        private async Task<List<Claim>> GetBasicClaims(User user)
            => new List<Claim>
            {
                new Claim("Id", user.Id),
                //new Claim("EmployeeId", await GetEmployeeId(user.Id)),
                new Claim(ClaimTypes.Email, user?.Email ?? ""),
                new Claim("UserName", user?.UserName ?? ""),
                new Claim("FirstUse", user?.FirstUse.ToString()?? "false"),
                new Claim("LastPasswordChange", (user?.LastPasswordChange == null)? string.Empty : user.LastPasswordChange.ToString()!),
                new Claim("ShouldChangePassword", (await ShouldChangePassword(user?.LastPasswordChange)).ToString()),
                new Claim("DataAccessKey", (string.IsNullOrEmpty(user?.DataAccessKey))? string.Empty : user.DataAccessKey),
            };

        private async Task<bool> ShouldChangePassword(DateTime? lastChange)
        {
            var daysToChange = 30; //await dbContext.AppSettings.FindAsync("ForceChangePasswordEvery");

            //if (daysToChange == null || daysToChange.Value == "0")
            //    return false;

            if (lastChange == null)
                return true;

            var days = Convert.ToInt32(daysToChange);
            var changeDate = lastChange.Value.AddDays(days);

            return DateTime.Now > changeDate;
        }

        //private async Task<string> GetEmployeeId(string userId)
        //{
        //    var employeeResult = await employeesService.GetByUserId(userId);

        //    return !employeeResult.HasErrors ? employeeResult.Result?.Id.ToString() ?? "0" : "0";
        //}

        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
        {
            var tokenOptions = new JwtSecurityToken(
                //issuer: jwtSettings.issuar,
                //audience: jwtSettings.validAudience,
                claims: claims,
                expires: DateTime.Now.AddHours(Convert.ToDouble(jwtSettings.ExpiresInHours)),
                signingCredentials: signingCredentials
            );

            return tokenOptions;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            var rng = RandomNumberGenerator.Create();

            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);

        }

        private string GetUserIdFromExpiredToken(string token)
        {
            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret!)),
                    ValidateLifetime = false,
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                SecurityToken securityToken;

                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);

                var jwtSecurityToken = securityToken as JwtSecurityToken;
                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token");
                }

                string userId = principal.Claims.First(x => x.Type == "Id").Value;

                return userId;
            }
            catch (Exception e)
            {
                var msg = e.Message;
                return "";
            }

        }

        private bool IsAdmin(User user)
        {
            var userRoles = dbContext.UserRoles.Where(ur => ur.UserId == user.Id).ToList();

            foreach (var userRole in userRoles)
            {
                var role = dbContext.Roles.Find(userRole.RoleId);
                if (role.Name == "Admin")
                    return true;
            }

            return false;

        }

        #endregion
    }
}
