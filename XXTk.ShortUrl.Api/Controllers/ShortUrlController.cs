using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XXTk.ShortUrl.Api.Redis;

namespace XXTk.ShortUrl.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ShortUrlController : ControllerBase
    {
        private const string ShortUrlIdRedisKey = "ShortUrlId";
        private const string ShortLongUrlRedisKeyPrefix = "ShortLongUrl-";
        private const string LongShortUrlRedisKeyPrefix = "LongShortUrl-";
        private readonly static TimeSpan _shortUrlExpiry = TimeSpan.FromMinutes(1);

        private readonly ILogger<ShortUrlController> _logger;
        private readonly IDatabase _redis;

        public ShortUrlController(
            ILogger<ShortUrlController> logger,
            DefaultRedisHelper redisHelper)
        {
            _logger = logger;
            _redis = redisHelper.GetDatabase();
        }

        [HttpPost("GetShortUrl")]
        public string GetShortUrl([FromBody] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            var longUrlRedisKey = GetLongUrlRedisKey(url);
            var shortUrl = _redis.StringGet(longUrlRedisKey);
            if (shortUrl.HasValue)
            {
                _redis.KeyExpire(GetShortUrlRedisKey(shortUrl), _shortUrlExpiry);
                _redis.KeyExpire(longUrlRedisKey, _shortUrlExpiry);

                return shortUrl;
            }

            if (!_redis.KeyExists(ShortUrlIdRedisKey))
            {
                _redis.StringSet(ShortUrlIdRedisKey, 10000);
            }

            var id = _redis.StringIncrement(ShortUrlIdRedisKey);
            shortUrl = Base62Converter.LongToBase(id);
            _redis.StringSet(GetShortUrlRedisKey(shortUrl), url, _shortUrlExpiry);
            _redis.StringSet(longUrlRedisKey, shortUrl, _shortUrlExpiry);

            return shortUrl;
        }

        [HttpGet("GetOriginalUrl")]
        public string GetOriginalUrl(string shortUrl)
        {
            var shortUrlRedisKey = GetShortUrlRedisKey(shortUrl);
            var url = _redis.StringGet(shortUrlRedisKey);
            if (url.HasValue)
            {
                _redis.KeyExpire(shortUrlRedisKey, _shortUrlExpiry);
                _redis.KeyExpire(GetLongUrlRedisKey(url), _shortUrlExpiry);
                return url;
            }

            return null;
        }

        [NonAction]
        private static string GetShortUrlRedisKey(string shortUrl)
        {
            return $"{ShortLongUrlRedisKeyPrefix}-{shortUrl}";
        }

        [NonAction]
        private static string GetLongUrlRedisKey(string url)
        {
            return $"{LongShortUrlRedisKeyPrefix}-{url}";
        }
    }

    public static class Base62Converter
    {
        private static readonly char[] _baseChars
            = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private static readonly Dictionary<char, int> _charValues
            = _baseChars
            .Select((c, i) => new { Char = c, Index = i })
            .ToDictionary(c => c.Char, c => c.Index);

        public static string LongToBase(long value)
        {
            var targetBase = _baseChars.Length;
            var buffer = new char[Math.Max((int)Math.Ceiling(Math.Log(value + 1, targetBase)), 1)];

            var i = buffer.Length;
            do
            {
                buffer[--i] = _baseChars[value % targetBase];
                value /= targetBase;
            }
            while (value > 0);

            return new string(buffer, i, buffer.Length - i);
        }

        public static long BaseToLong(string number)
        {
            var chrs = number.ToCharArray();
            var m = chrs.Length - 1;
            var n = _baseChars.Length;
            int x;
            var result = 0L;
            for (var i = 0; i < chrs.Length; i++)
            {
                x = _charValues[chrs[i]];
                result += x * (long)Math.Pow(n, m--);
            }

            return result;
        }
    }
}
