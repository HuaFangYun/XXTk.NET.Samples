using Consul;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XXTk.Consul.Mvc.Services
{
    public class MyAppService : IMyAppService
    {
        public MyAppService()
        {
        }

        public async Task<string> GetHelloAysnc()
        {
            var serviceUrls = ServiceHelper.ServiceUrlDic["MyService"];
            if (!serviceUrls.Any())
                throw new Exception("MyService服务列表为空");

            var client = new RestClient(serviceUrls.ElementAt(new Random().Next(0, serviceUrls.Count)));
            var request = new RestRequest("/api/hello", Method.GET);

            var response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                return response.Content;
            }

            throw new Exception(response.ErrorMessage);
        }

        public async Task<List<string>> GetNamesAsync()
        {
            var serviceUrls = ServiceHelper.ServiceUrlDic["MyService"];
            if (!serviceUrls.Any())
                throw new Exception("MyService服务列表为空");

            var client = new RestClient(serviceUrls.ElementAt(new Random().Next(0, serviceUrls.Count)));
            var request = new RestRequest("/api/hello/GetNames", Method.GET);

            var response = await client.ExecuteAsync<List<string>>(request);
            if (response.IsSuccessful)
            {
                return response.Data;
            }

            throw new Exception(response.ErrorMessage);
        }
    }
}
