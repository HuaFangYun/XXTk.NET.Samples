using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XXTk.RedPacket.Api.Redis
{
    public static class RedisConsts
    {
        public static class Instances
        {
            private const string Default = nameof(Default);
        }

        public static class Keys
        {
            public const string ShortUrlIdRedisKey = "ShortUrlId";
            public const string ShortLongUrlRedisKeyPrefix = "ShortLongUrl-";
            public const string LongShortUrlRedisKeyPrefix = "LongShortUrl-";
        }
    }
}
