using CRMService.DTOs;

namespace CRMService.Services
{
    public interface ISessionService
    {
        Task<IReadOnlyList<SessionResponse>> GetByClientAsync(Guid userId, Guid clientId);
        Task<SessionResponse> GetByIdAsync(Guid userId, Guid sessionId);
        Task<SessionResponse> CreateAsync(Guid userId, SessionCreateRequest request);
        Task<SessionResponse> UpdateAsync(Guid userId, Guid sessionId, SessionUpdateRequest request);
        Task DeleteAsync(Guid userId, Guid sessionId);
    }
}
