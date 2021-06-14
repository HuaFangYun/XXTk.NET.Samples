using Consul;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XXTk.Consul.Mvc.Services
{
    public class ServiceHelper
    {
        private static readonly string[] _serviceNames = new[] { "MyService" };
        public static readonly ConcurrentDictionary<string, ConcurrentBag<string>> ServiceUrlDic = new();

        private readonly IConfiguration _configuration;
        private readonly ConsulClient _consulClient;

        public ServiceHelper(IConfiguration configuration)
        {
            _configuration = configuration;
            _consulClient = new ConsulClient(c =>
            {
                c.Address = new Uri(_configuration["Consul:Address"]);
            });
        }

        public void GetServices()
        {
            Array.ForEach(_serviceNames, serviceName =>
            {
                Task.Run(() =>
                {
                    var queryOptions = new QueryOptions
                    {
                        WaitTime = TimeSpan.FromMinutes(5)
                    };
                    while (true)
                    {
                        GetServicesInternal(serviceName, queryOptions);
                    }
                });
            });
        }

        private void GetServicesInternal(string serviceName, QueryOptions queryOptions)
        {
            var result = _consulClient.Health.Service(serviceName, null, true, queryOptions).Result;

            if (queryOptions.WaitIndex != result.LastIndex)
            {
                queryOptions.WaitIndex = result.LastIndex;

                var serviceUrls = result.Response.Select(e => $"http://{e.Service.Address}:{e.Service.Port}");
                ServiceUrlDic[serviceName] = new ConcurrentBag<string>(serviceUrls);
            }

        }
    }
}
