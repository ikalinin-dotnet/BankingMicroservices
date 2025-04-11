using System.Threading.Tasks;
using AccountService.DTOs;

namespace AccountService.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> Login(LoginRequest request);
        Task<AuthResponse> Register(RegisterRequest request);
    }
}