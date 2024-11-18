using Minimal.Domain.Core.Administration;
using Minimal.Domain.Core.Responces;

namespace Minimal.Services.Core.Auth
{
    public interface IAuthService
    {
        Task<AuthResult> Register(RegisterRequest registerRequest);

        Task<AuthResult> Login(LoginRequest loginRequest);

        Task<AuthResult> RefreshToken(RefreshTokenRequest refreshToken);
    }
}
