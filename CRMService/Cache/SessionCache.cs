using CRMService.DTOs;
using Microsoft.Extensions.Options;
using Shared.Settings;
using StackExchange.Redis;
using System.Text.Json;

namespace CRMService.Cache
{
    public class SessionCache : ISessionCache
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly TimeSpan _ttl;

        public SessionCache(IConnectionMultiplexer redis, IOptions<RedisOptions> options)
        {
            _redis = redis;
            _ttl = TimeSpan.FromMinutes(options.Value.SessionsTtlMinutes);
        }

        public async Task<IReadOnlyList<SessionResponse>?> GetSessionsAsync(Guid clientId)
        {
            var db = _redis.GetDatabase();
            var key = GetKey(clientId);
            var json = await db.StringGetAsync(key);

            if (json.IsNullOrEmpty)
                return null;

            return JsonSerializer.Deserialize<List<SessionResponse>>(json!);
        }

        public async Task SetSessionsAsync(Guid clientId, IReadOnlyList<SessionResponse> sessions)
        {
            var db = _redis.GetDatabase();
            var key = GetKey(clientId);
            var json = JsonSerializer.Serialize(sessions);

            await db.StringSetAsync(key, json, _ttl);
        }

        public Task InvalidateAsync(Guid clientId)
        {
            var db = _redis.GetDatabase();
            return db.KeyDeleteAsync(GetKey(clientId));
        }

        private static string GetKey(Guid clientId) => $"sessions:client:{clientId}";
    }
}
