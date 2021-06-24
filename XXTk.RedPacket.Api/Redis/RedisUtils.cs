using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XXTk.RedPacket.Api.Redis
{
    public static class RedisUtils
    {
        public static string SerializeObject<T>(T obj) where T: class
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        public static T DeserializeObject<T>(string value) where T: class
        {
            return JsonConvert.DeserializeObject<T>(value);
        }
    }
}
