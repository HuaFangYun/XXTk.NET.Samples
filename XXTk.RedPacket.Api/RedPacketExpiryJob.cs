using Quartz;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Abstractions;
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
        private readonly IRedisCacheClient _redisCacheClient;

        public RedPacketExpiryJob(IRedisCacheClient redisCacheClient)
        {
            _redisCacheClient = redisCacheClient;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var redisDatabase = _redisCacheClient.GetDbFromConfiguration();

            // 查询已过期的红包
            var now = DateTimeOffset.Now.ToUnixTimeSeconds();
            var expiredRedPacketIds = (await redisDatabase.Database.SortedSetRangeByScoreAsync("RedPacketExpiries", 0, now))
                .Select(id => (string)id);

            foreach (var id in expiredRedPacketIds)
            {
                var moneies = (await redisDatabase.Database.ListRangeAsync(id))
                    .Select(value => (decimal)value);

                if (moneies.Any())
                {
                    using (var sw = File.AppendText("RedPacketExpiryLog.txt"))
                    {
                        sw.WriteLine($"Id: {id}{Environment.NewLine}" +
                                     $"Money: {moneies.Sum()}{Environment.NewLine}" +
                                     $"Time: {DateTimeOffset.Now}{Environment.NewLine}"
                        );
                    }
                }

                await redisDatabase.SortedSetRemoveAsync("RedPacketExpiries", id);
                await redisDatabase.Database.KeyDeleteAsync(id);
            }
        }
    }
}
