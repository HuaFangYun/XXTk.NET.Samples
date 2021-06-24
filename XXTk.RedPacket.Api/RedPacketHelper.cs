using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XXTk.RedPacket.Api.Redis;

namespace XXTk.RedPacket.Api
{
    public class RedPacketHelper
    {
        private static readonly TimeSpan _expiry = TimeSpan.FromMinutes(1);

        private readonly IDatabase _redis;

        public RedPacketHelper(DefaultRedisHelper redisHelper)
        {
            _redis = redisHelper.GetDatabase();
        }

        /// <summary>
        /// 创建红包
        /// 
        /// 使用二倍均值法拆分红包
        /// </summary>
        /// <param name="totalMoneyOfYuan">红包总钱数（单位：元）</param>
        /// <param name="count">红包数量</param>
        /// <returns>红包id</returns>
        public string CreateRedPacket(decimal totalMoneyOfYuan, int count)
        {
            if (!IsNumber(totalMoneyOfYuan.ToString(), 3, 2))
                throw new ArgumentException("红包面值格式不正确", nameof(totalMoneyOfYuan));

            var redPacketId = Guid.NewGuid().ToString("N");
            var moneies = new List<RedisValue>(count);
            var leftMoneyOfFen = (int)(totalMoneyOfYuan * 100);
            var leftCount = count;
            var random = new Random();

            while(leftCount > 1)
            {
                var money = random.Next(leftMoneyOfFen / leftCount * 2);
                moneies.Add(Math.Round(money / 100m, 2).ToString());

                leftMoneyOfFen -= money;
                leftCount--;
            }
            moneies.Add(Math.Round(leftMoneyOfFen / 100d, 2).ToString());

            // 将红包金额存放到List中
            _redis.ListRightPush(redPacketId, moneies.ToArray());
            // 将红包的到期时间存放到zset中
            _redis.SortedSetAdd("RedPacketExpiries", redPacketId, DateTimeOffset.Now.Add(_expiry).ToUnixTimeSeconds());

            return redPacketId;
        }

        public decimal? GetRedPacket(string id, string userId)
        {
            if (_redis.HashExists(GetRedPacketRecordsRedisKey(id), userId))
                throw new Exception("您已经领取过了");

            var money = _redis.ListLeftPop(id);
            if (money.HasValue)
            {
                var moneyOfYuan = (decimal)money;
                _redis.HashSet(
                    GetRedPacketRecordsRedisKey(id), 
                    userId,
                    RedisUtils.SerializeObject(new RedPacketRecord { MoneyOfYuan = moneyOfYuan, DateTime = DateTime.Now })
                );
                return moneyOfYuan;
            }

            return null;
        }

        public List<RedPacketRecord> GetRedPacketRecords(string id)
        {
            return _redis.HashGetAll(GetRedPacketRecordsRedisKey(id))
                .Select(entry => 
                {
                    var record = RedisUtils.DeserializeObject<RedPacketRecord>(entry.Value);
                    record.UserId = entry.Name;

                    return record;
                })
                .ToList();
        }

        private static RedisKey GetRedPacketRecordsRedisKey(string redPacketId)
        {
            if (string.IsNullOrWhiteSpace(redPacketId))
                throw new ArgumentNullException(redPacketId);

            return $"{redPacketId}-Records";
        }

        /// <summary>
        /// 判断是否为指定格式数字
        /// </summary>
        /// <param name="numberStr"></param>
        /// <param name="precision">最大整数位数</param>
        /// <param name="scale">最大小数位数</param>
        /// <returns></returns>
        public static bool IsNumber(string numberStr, uint precision, uint scale)
        {
            if (precision <= 0)
            {
                return false;
            }

            string pattern = @$"(^\d{{1,{precision}}}";
            if (scale > 0)
            {
                pattern += @$"\.\d{{0,{scale}}}$)|{pattern}";
            }
            pattern += "$)";
            return Regex.IsMatch(numberStr, pattern);
        }
    }
}
