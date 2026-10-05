using CRMService.Cache;
using CRMService.Data;
using CRMService.DTOs;
using CRMService.Messaging;
using CRMService.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Events;

namespace CRMService.Services
{
    public class SessionService : ISessionService
    {
        private readonly AppDbContext _db;
        private readonly ISessionCache _cache;
        private readonly ICrmEventPublisher _events;

        public SessionService(AppDbContext db, ISessionCache cache, ICrmEventPublisher events)
        {
            _db = db;
            _cache = cache;
            _events = events;
        }

        public async Task<IReadOnlyList<SessionResponse>> GetByClientAsync(Guid userId, Guid clientId)
        {
            // 1. Проверяем, что клиент принадлежит пользователю
            var clientExists = await _db.Clients
                .AnyAsync(c => c.Id == clientId && c.UserId == userId);
            if (!clientExists)
                throw new KeyNotFoundException("Клиент не найден.");

            // 2. Пробуем взять из кэша
            var cached = await _cache.GetSessionsAsync(clientId);
            if (cached is not null)
                return cached;

            // 3. Промах — идём в БД
            var sessions = await _db.Sessions
                .Where(s => s.ClientId == clientId)
                .OrderBy(s => s.ScheduledAt)
                .Select(s => new SessionResponse(
                    s.Id, s.ClientId, s.ScheduledAt, s.DurationInMinutes, s.Status, s.Notes))
                .ToListAsync();

            // 4. Кладём в кэш
            await _cache.SetSessionsAsync(clientId, sessions);

            return sessions;
        }

        public async Task<SessionResponse> GetByIdAsync(Guid userId, Guid sessionId)
        {
            var session = await _db.Sessions
                .Include(s => s.Client)
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.Client.UserId == userId)
                ?? throw new KeyNotFoundException("Сессия не найдена.");

            return new SessionResponse(
                session.Id, session.ClientId, session.ScheduledAt,
                session.DurationInMinutes, session.Status, session.Notes);
        }

        public async Task<SessionResponse> CreateAsync(Guid userId, SessionCreateRequest request)
        {
            // 1. Проверка владельца клиента
            var clientExists = await _db.Clients
                .AnyAsync(c => c.Id == request.ClientId && c.UserId == userId);
            if (!clientExists)
                throw new KeyNotFoundException("Клиент не найден.");

            // 2. Создание сессии
            var session = new Session
            {
                Id = Guid.NewGuid(),
                ClientId = request.ClientId,
                ScheduledAt = request.ScheduledAt.ToUniversalTime(),
                DurationInMinutes = request.DurationInMinutes,
                Status = request.Status,
                Notes = request.Notes ?? string.Empty
            };

            _db.Sessions.Add(session);
            await _db.SaveChangesAsync();

            // 3. Инвалидация кэша
            await _cache.InvalidateAsync(session.ClientId);

            // 4. Публикация SessionPlannedEvent
            if (session.Status == SessionStatus.Planned)
            {
                await _events.PublishSessionPlannedAsync(new SessionPlannedEvent(
                    session.Id,
                    session.ClientId,
                    session.ScheduledAt,
                    session.DurationInMinutes));
            }

            return new SessionResponse(
                session.Id, session.ClientId, session.ScheduledAt,
                session.DurationInMinutes, session.Status, session.Notes);
        }

        public async Task<SessionResponse> UpdateAsync(Guid userId, Guid sessionId, SessionUpdateRequest request)
        {
            var session = await _db.Sessions
                .Include(s => s.Client)
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.Client.UserId == userId)
                ?? throw new KeyNotFoundException("Сессия не найдена.");

            var wasPlanned = session.Status == SessionStatus.Planned;

            session.ScheduledAt = request.ScheduledAt.ToUniversalTime();
            session.DurationInMinutes = request.DurationInMinutes;
            session.Status = request.Status;
            session.Notes = request.Notes ?? string.Empty;

            await _db.SaveChangesAsync();

            await _cache.InvalidateAsync(session.ClientId);

            // Если сессия стала Planned — публикуем событие
            if (!wasPlanned && session.Status == SessionStatus.Planned)
            {
                await _events.PublishSessionPlannedAsync(new SessionPlannedEvent(
                    session.Id, session.ClientId, session.ScheduledAt, session.DurationInMinutes));
            }

            return new SessionResponse(
                session.Id, session.ClientId, session.ScheduledAt,
                session.DurationInMinutes, session.Status, session.Notes);
        }

        public async Task DeleteAsync(Guid userId, Guid sessionId)
        {
            var session = await _db.Sessions
                .Include(s => s.Client)
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.Client.UserId == userId)
                ?? throw new KeyNotFoundException("Сессия не найдена.");

            var clientId = session.ClientId;

            _db.Sessions.Remove(session);
            await _db.SaveChangesAsync();

            await _cache.InvalidateAsync(clientId);
        }
    }
}
