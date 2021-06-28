using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Quartz;
using Quartz.Impl;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Newtonsoft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XXTk.RedPacket.Api.Redis;

namespace XXTk.RedPacket.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var redisConfig = Configuration.GetSection("Redis").Get<RedisConfiguration>();
            services.AddStackExchangeRedisExtensions<NewtonsoftSerializer>(redisConfig);
            services.AddTransient<RedPacketHelper>();

            services.AddSingleton<RedPacketExpiryJob>();
            services.AddTransient<QuartzStartUp>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "XXTk.RedPacket.Api", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime appLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "XXTk.RedPacket.Api v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            var quartz = app.ApplicationServices.GetRequiredService<QuartzStartUp>();
            appLifetime.ApplicationStarted.Register(() =>
            {
                quartz.Start().Wait();
            });

            appLifetime.ApplicationStopped.Register(() =>
            {
                quartz.Stop();
            });

            // 将redis信息通过api暴露到外部
            // /redis/connectionInfo
            // /redis/info
            app.UseRedisInformation(o => 
            {
                // 配置允许调用api的ip地址
                //o.AllowedIPs = 
                //o.AllowFunction = (httpContext) =>
                //{
                //    // 自定义认证授权逻辑

                //    // true：允许访问
                //    // false：拒绝访问
                //    return true;
                //};
            });
        }
    }
}
