using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;
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

        private readonly IRedisCacheClient _redisCacheClient;
        private readonly IRedisDatabase _redisDatabase;

        public ShortUrlHelper(IRedisCacheClient redisCacheClient)
        {
            _redisCacheClient = redisCacheClient;
            _redisDatabase = _redisCacheClient.GetDbFromConfiguration();
        }

        public async Task<string> GetShortUrl(string longUrl)
        {
            if (string.IsNullOrWhiteSpace(longUrl))
            {
                return null;
            }

            var longUrlRedisKey = GetLongUrlRedisKey(longUrl);
            var shortUrl = await _redisDatabase.GetAsync<string>(longUrlRedisKey);
            if (shortUrl is not null)
            {
                await _redisDatabase.Database.KeyExpireAsync(GetShortUrlRedisKey(shortUrl), _shortUrlExpiry);
                await _redisDatabase.Database.KeyExpireAsync(longUrlRedisKey, _shortUrlExpiry);

                return shortUrl;
            }

            if (!_redisDatabase.Database.KeyExists(RedisConsts.Keys.ShortUrlIdRedisKey))
            {
                await _redisDatabase.AddAsync(RedisConsts.Keys.ShortUrlIdRedisKey, 10000);
            }

            var id = await _redisDatabase.Database.StringIncrementAsync(RedisConsts.Keys.ShortUrlIdRedisKey);
            shortUrl = Base62Converter.LongToBase(id);
            await _redisDatabase.AddAsync(GetShortUrlRedisKey(shortUrl), longUrl, _shortUrlExpiry);
            await _redisDatabase.AddAsync(longUrlRedisKey, shortUrl, _shortUrlExpiry);

            return shortUrl;

        }

        public async Task<string> GetLongUrl(string shortUrl)
        {
            var shortUrlRedisKey = GetShortUrlRedisKey(shortUrl);
            var longUrl = await _redisDatabase.GetAsync<string>(shortUrlRedisKey);
            if (longUrl is not null)
            {
                _redisDatabase.Database.KeyExpire(shortUrlRedisKey, _shortUrlExpiry);
                _redisDatabase.Database.KeyExpire(GetLongUrlRedisKey(longUrl), _shortUrlExpiry);
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
