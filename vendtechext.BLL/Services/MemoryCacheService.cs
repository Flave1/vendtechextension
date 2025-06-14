using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace vendtechext.BLL.Services
{
    public interface ICacheService
    {
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task<T?> GetAsync<T>(string key);
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
    }
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<MemoryCacheService> _logger;

        public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(30)
            };

            _cache.Set(key, value, options);
            return Task.CompletedTask;
        }

        public Task<T?> GetAsync<T>(string key)
        {
            _cache.TryGetValue(key, out T? value);
            return Task.FromResult(value);
        }

        public Task RemoveAsync(string key)
        {
            _cache.Remove(key);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key)
        {
            var exists = _cache.TryGetValue(key, out _);
            return Task.FromResult(exists);
        }
    }
}