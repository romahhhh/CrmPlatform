using UserService.Dtos;

namespace UserService.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> UpdateAsync(Guid UserId, UpdateUserRequest request);
        Task DeleteAsync(Guid UserId);
    }
}
