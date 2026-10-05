using CRMService.Data;
using CRMService.DTOs;
using CRMService.Messaging;
using CRMService.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Events;

namespace CRMService.Services
{
    public class ClientService : IClientService
    {
        private readonly AppDbContext _db;
        private readonly ICrmEventPublisher _events;

        public ClientService(AppDbContext db, ICrmEventPublisher events)
        {
            _db = db;
            _events = events;
        }

        public async Task<IReadOnlyList<ClientResponse>> GetAllAsync(Guid userId)
        {
            var clients = await _db.Clients
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Name)
                .Select(c => new ClientResponse(c.Id, c.Name, c.Email, c.Phone, c.CreatedAt))
                .ToListAsync();

            return clients;
        }

        public async Task<ClientResponse> GetByIdAsync(Guid userId, Guid clientId)
        {
            var client = await _db.Clients
                .FirstOrDefaultAsync(c => c.Id == clientId && c.UserId == userId)
                ?? throw new KeyNotFoundException("Клиент не найден.");

            return new ClientResponse(client.Id, client.Name, client.Email, client.Phone, client.CreatedAt);
        }

        public async Task<ClientResponse> CreateAsync(Guid userId, ClientCreateRequest request)
        {
            var client = new Client
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                Phone = request.Phone.Trim(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _db.Clients.Add(client);
            await _db.SaveChangesAsync();

            await _events.PublishClientCreatedAsync(new ClientCreatedEvent(
                client.Id,
                client.Name,
                client.Email));

            return new ClientResponse(client.Id, client.Name, client.Email, client.Phone, client.CreatedAt);
        }

        public async Task<ClientResponse> UpdateAsync(Guid userId, Guid clientId, ClientUpdateRequest request)
        {
            var client = await _db.Clients
                .FirstOrDefaultAsync(c => c.Id == clientId && c.UserId == userId)
                ?? throw new KeyNotFoundException("Клиент не найден.");

            client.Name = request.Name.Trim();
            client.Email = request.Email.Trim().ToLowerInvariant();
            client.Phone = request.Phone.Trim();

            await _db.SaveChangesAsync();

            return new ClientResponse(client.Id, client.Name, client.Email, client.Phone, client.CreatedAt);
        }

        public async Task DeleteAsync(Guid userId, Guid clientId)
        {
            var client = await _db.Clients
                .FirstOrDefaultAsync(c => c.Id == clientId && c.UserId == userId)
                ?? throw new KeyNotFoundException("Клиент не найден.");

            _db.Clients.Remove(client);
            await _db.SaveChangesAsync();
        }
    }
}
