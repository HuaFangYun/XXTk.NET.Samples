using Quartz;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XXTk.RedPacket.Api.Redis;

namespace XXTk.RedPacket.Api
{
    [DisallowConcurrentExecution]
    public class RedPacketExpiryJob : IJob
    {
        private readonly IDatabase _redis;

        public RedPacketExpiryJob(DefaultRedisHelper redisHelper)
        {
            _redis = redisHelper.GetDatabase();
        }

        public async Task Execute(IJobExecutionContext context)
        {
            // 查询已过期的红包
            var now = DateTimeOffset.Now.ToUnixTimeSeconds();
            var expiredRedPacketIds = (await _redis.SortedSetRangeByScoreAsync("RedPacketExpiries", 0, now))
                .Select(value => (string)value);

            foreach (var id in expiredRedPacketIds)
            {
                var moneies = (await _redis.ListRangeAsync(id))
                    .Select(value => (decimal)value);

                if (moneies.Any())
                {
                    using (var sw = File.AppendText("RedPacketExpiryLog.txt"))
                    {
                        sw.WriteLine($"Id: {id}{Environment.NewLine}" +
                                     $"Money: {moneies.Sum()}{Environment.NewLine}" +
                                     $"Time: {DateTime.Now}{Environment.NewLine}"
                        );
                    }
                }

                await _redis.SortedSetRemoveAsync("RedPacketExpiries", id);
                await _redis.KeyDeleteAsync(id);
            }
        }
    }
}
