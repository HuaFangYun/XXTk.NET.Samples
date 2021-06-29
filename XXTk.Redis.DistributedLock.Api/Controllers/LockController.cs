using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RedLockNet;
using RedLockNet.SERedis;
using StackExchange.Redis.Extensions.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XXTk.Redis.DistributedLock.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LockController : ControllerBase
    {
        private readonly ILogger<LockController> _logger;
        private readonly IRedisCacheClient _redisCacheClient;
        private readonly IRedisDatabase _redisDatabase;
        private readonly IDistributedLockFactory _distributedLockFactory;

        public LockController(
            ILogger<LockController> logger,
            IRedisCacheClient redisCacheClient,
            IRedisDatabase redisDatabase,
            IDistributedLockFactory distributedLockFactory)
        {
            _logger = logger;
            _redisCacheClient = redisCacheClient;
            _redisDatabase = redisDatabase;
            _distributedLockFactory = distributedLockFactory;
        }

        [HttpPost("InitProductStock")]
        public async Task InitProductStock()
        {
            for (int i = 0; i < 10; i++)
            {
                await _redisDatabase.AddAsync(i.ToString(), 100);
            }
        }

        [HttpPost("immediate/{productId}")]
        public async Task<long> PostImmediate([FromRoute] int productId = 0)
        {
            var productIdKey = productId.ToString();

            // 加分布式锁，锁有效时长30s，内部有Timer进行自动延时
            // 若获取不到锁，则立即返回
            // key格式：[redlock:produtIdKey]
            using(var redLock = await _distributedLockFactory.CreateLockAsync(productIdKey, TimeSpan.FromSeconds(30)))
            {
                // 确保已获取到锁
                if (redLock.IsAcquired)
                {
                    var currentQuantity = await _redisDatabase.GetAsync<long?>(productIdKey);
                    if (currentQuantity >= 1)
                    {
                        var lefttQuantity = await _redisDatabase.Database.StringDecrementAsync(productIdKey);
                        return lefttQuantity;
                    }

                    throw new ApplicationException("库存不足");
                }
            }

            throw new ApplicationException("获取锁失败");
        }

        [HttpPost("wait/{productId}")]
        public async Task<long> PostWait([FromRoute] int productId = 0)
        {
            var productIdKey = productId.ToString();

            // 加分布式锁，锁有效时长30s，内部有Timer进行自动延时
            // 若获取不到锁，则每1秒进行一次重试，直到总等待时间超过60秒，锁获取失败
            using (var redLock = await _distributedLockFactory.CreateLockAsync($"lock-{productId}", TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(1)))
            {
                // 确保已获取到锁
                if (redLock.IsAcquired)
                {
                    var currentQuantity = await _redisDatabase.GetAsync<long?>(productIdKey);
                    if (currentQuantity >= 1)
                    {
                        var lefttQuantity = await _redisDatabase.Database.StringDecrementAsync(productIdKey);
                        return lefttQuantity;
                    }

                    throw new ApplicationException("库存不足");
                }
            }

            throw new ApplicationException("获取锁失败");
        }
    }
}
