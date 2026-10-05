using CRMService.DTOs;

namespace CRMService.Services
{
    public interface IClientService
    {
        Task<IReadOnlyList<ClientResponse>> GetAllAsync(Guid userId);
        Task<ClientResponse> GetByIdAsync(Guid userId, Guid clientId);
        Task<ClientResponse> CreateAsync(Guid userId, ClientCreateRequest request);
        Task<ClientResponse> UpdateAsync(Guid userId, Guid clientId, ClientUpdateRequest request);
        Task DeleteAsync(Guid userId, Guid clientId);
    }
}
