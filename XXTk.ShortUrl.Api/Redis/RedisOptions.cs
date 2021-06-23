using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XXTk.ShortUrl.Api.Redis
{
    public class RedisOptions
    {
        public string ConnectionString { get; set; }

        public string InstanceName { get; set; }

        public int DefaultDB { get; set; }
    }
}
