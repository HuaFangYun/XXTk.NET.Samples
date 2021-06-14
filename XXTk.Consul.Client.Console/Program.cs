using Consul;
using System;
using System.Collections.Generic;

namespace XXTk.Consul.Client.Consoled
{
    class Program
    {
        static void Main(string[] args)
        {
            ShowServices(GetServices());

            Console.WriteLine("Finish...");
            Console.ReadKey();
        }

        private static Dictionary<string, AgentService> GetServices()
        {
            var consulClient = new ConsulClient(c =>
            {
                c.Address = new Uri("http://192.168.181.130:8500");
                c.Datacenter = "dc1";
            });

            var services = consulClient.Agent.Services().Result.Response;
            return services;
        }

        private static void ShowServices(Dictionary<string, AgentService> services)
        {
            foreach (var serviceKvp in services)
            {
                Console.WriteLine($"ID：{serviceKvp.Key}");
                Console.WriteLine($"Address：{serviceKvp.Value.Address}");
                Console.WriteLine($"Port：{serviceKvp.Value.Port}");
                Console.WriteLine("--------------------------------------------------------");
            }
        }
    }
}
