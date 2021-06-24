using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XXTk.ShortUrl.Api.Converters;
using XXTk.ShortUrl.Api.Redis;

namespace XXTk.ShortUrl.Api
{
    public class ShortUrlHelper
    {
        private readonly static TimeSpan _shortUrlExpiry = TimeSpan.FromMinutes(1);

        private readonly IDatabase _redis;

        public ShortUrlHelper(DefaultRedisHelper redisHelper)
        {
            _redis = redisHelper.GetDatabase();
        }

        public string GetShortUrl(string longUrl)
        {
            if (string.IsNullOrWhiteSpace(longUrl))
            {
                return null;
            }

            var longUrlRedisKey = GetLongUrlRedisKey(longUrl);
            var shortUrl = _redis.StringGet(longUrlRedisKey);
            if (shortUrl.HasValue)
            {
                _redis.KeyExpire(GetShortUrlRedisKey(shortUrl), _shortUrlExpiry);
                _redis.KeyExpire(longUrlRedisKey, _shortUrlExpiry);

                return shortUrl;
            }

            if (!_redis.KeyExists(RedisConsts.Keys.ShortUrlIdRedisKey))
            {
                _redis.StringSet(RedisConsts.Keys.ShortUrlIdRedisKey, 10000);
            }

            var id = _redis.StringIncrement(RedisConsts.Keys.ShortUrlIdRedisKey);
            shortUrl = Base62Converter.LongToBase(id);
            _redis.StringSet(GetShortUrlRedisKey(shortUrl), longUrl, _shortUrlExpiry);
            _redis.StringSet(longUrlRedisKey, shortUrl, _shortUrlExpiry);

            return shortUrl;

        }

        public string GetLongUrl(string shortUrl)
        {
            var shortUrlRedisKey = GetShortUrlRedisKey(shortUrl);
            var longUrl = _redis.StringGet(shortUrlRedisKey);
            if (longUrl.HasValue)
            {
                _redis.KeyExpire(shortUrlRedisKey, _shortUrlExpiry);
                _redis.KeyExpire(GetLongUrlRedisKey(longUrl), _shortUrlExpiry);
                return longUrl;
            }

            return null;
        }

        private static string GetShortUrlRedisKey(string shortUrl)
        {
            return $"{RedisConsts.Keys.ShortLongUrlRedisKeyPrefix}-{shortUrl}";
        }

        private static string GetLongUrlRedisKey(string url)
        {
            return $"{RedisConsts.Keys.LongShortUrlRedisKeyPrefix}-{url}";
        }

    }
}
