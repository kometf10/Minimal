using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Minimal.Domain.Core.Administration;
using Minimal.Services.Core.Auth;

namespace Minmal.API.Controllers.Core
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService identityService;

        public AuthController(IAuthService identityService)
        {
            this.identityService = identityService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync(RegisterRequest registerRequest)
        {
            var authResult = await identityService.Register(registerRequest);

            if (!authResult.Successed)
            {
                return Ok(authResult);
            }

            return Ok(authResult);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest registerRequest)
        {
            var authResult = await identityService.Login(registerRequest);

            if (!authResult.Successed)
            {
                return Ok(authResult);
            }
            return Ok(authResult);
        }

        [HttpPost("Refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest refreshTokenRequest)
        {

            var authResult = await identityService.RefreshToken(refreshTokenRequest);

            if (!authResult.Successed)
            {
                //if (authResult.Errors.Contains("Refresh Token Expired"))
                //    return Ok(authResult);

                return Ok(authResult);
            }

            return Ok(authResult);

        }
    }
}
