using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilPDF.ComprobantesPdf.Cache
{
    public class LogoCacheService : ILogoCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly IHttpClientFactory _httpClientFactory;

        public LogoCacheService(
            IMemoryCache cache,
            IHttpClientFactory httpClientFactory)
        {
            _cache = cache;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<byte[]> GetLogoBytesAsync(string? logoUrl)
        {
            if (string.IsNullOrWhiteSpace(logoUrl))
                return Array.Empty<byte>();

            var cacheKey = $"logo_pdf_{logoUrl}";

            if (_cache.TryGetValue(cacheKey, out byte[]? logoBytes) && logoBytes is { Length: > 0 })
                return logoBytes;

            try
            {
                var client = _httpClientFactory.CreateClient();

                logoBytes = await client.GetByteArrayAsync(logoUrl);

                _cache.Set(cacheKey, logoBytes, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12),
                    SlidingExpiration = TimeSpan.FromHours(2)
                });

                return logoBytes;
            }
            catch
            {
                return Array.Empty<byte>();
            }
        }
    }
}
