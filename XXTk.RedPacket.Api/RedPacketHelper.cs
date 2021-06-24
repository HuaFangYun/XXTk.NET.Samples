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

            _redis.ListRightPush(redPacketId, moneies.ToArray());

            return redPacketId;
        }

        public decimal? GetRedPacket(string id, string userId)
        {
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

        private static string GetRedPacketRecordsRedisKey(string redPackedId)
        {
            if (string.IsNullOrWhiteSpace(redPackedId))
                throw new ArgumentNullException(redPackedId);

            return $"{redPackedId}-Records";
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
