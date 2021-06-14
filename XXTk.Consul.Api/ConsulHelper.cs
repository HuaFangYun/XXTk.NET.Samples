using Consul;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XXTk.Consul.Api
{
    public class ConsulHelper
    {
        private readonly IConfiguration _configuration;
        private readonly IHostApplicationLifetime _lifetime;

        public ConsulHelper(IConfiguration configuration, IHostApplicationLifetime lifetime)
        {
            _configuration = configuration;
            _lifetime = lifetime;
        }

        public void Register()
        {
            var consulConfig = _configuration.GetSection("Consul");
            var serviceConfig = consulConfig.GetSection("Service");

            var consulClient = new ConsulClient(c =>
            {
                c.Address = new Uri(consulConfig["Address"]);
            });
            
            var registration = new AgentServiceRegistration()
            {
                ID = serviceConfig["ID"],
                Name = serviceConfig["Name"],
                Address = serviceConfig["Address"],
                Port = int.Parse(serviceConfig["Port"]),
                Check = new AgentServiceCheck()
                {
                    // 服务启动5s后注册
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),
                    // 健康检查每10s一次
                    Interval = TimeSpan.FromSeconds(10),
                    // 健康检查地址
                    HTTP = $"http://{serviceConfig["Address"]}:{serviceConfig["Port"]}{serviceConfig["HealthCheck"]}",
                    // 超时时间
                    Timeout = TimeSpan.FromSeconds(5)
                }
            };

            // 服务注册
            consulClient.Agent.ServiceRegister(registration);

            // 应用程序正常终止时，取消服务注册
            _lifetime.ApplicationStopping.Register(() =>
            {
                consulClient.Agent.ServiceDeregister(registration.ID);
            });
        }
    }
}
