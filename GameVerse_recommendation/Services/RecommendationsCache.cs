using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GameVerse_recommendation.Services
{
    public class RecommendationsCache : MemoryCache
    {
        public RecommendationsCache(IOptions<MemoryCacheOptions> optionsAccessor) : base(optionsAccessor)
        {
        }

        public RecommendationsCache(IOptions<MemoryCacheOptions> optionsAccessor, ILoggerFactory loggerFactory) : base(optionsAccessor, loggerFactory)
        {
        }
    }
}
