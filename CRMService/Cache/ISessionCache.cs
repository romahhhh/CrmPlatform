using CRMService.DTOs;

namespace CRMService.Cache
{
    public interface ISessionCache
    {
        Task<IReadOnlyList<SessionResponse>?> GetSessionsAsync(Guid clientId);
        Task SetSessionsAsync(Guid clientId, IReadOnlyList<SessionResponse> sessions);
        Task InvalidateAsync(Guid clientId);
    }
}
