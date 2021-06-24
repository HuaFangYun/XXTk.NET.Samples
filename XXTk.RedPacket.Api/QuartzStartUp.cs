using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XXTk.RedPacket.Api
{
    public class QuartzStartUp
    {
        private readonly IServiceProvider _serviceProvider;

        //调度工厂
        private ISchedulerFactory _factory;
        //调度器
        private IScheduler _scheduler;

        public QuartzStartUp(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task Start()
        {
            //1、声明一个调度工厂
            _factory = new StdSchedulerFactory();
            //2、通过调度工厂获得调度器
            _scheduler = await _factory.GetScheduler();
            _scheduler.JobFactory = new RedPacketExpiryJobFactory(_serviceProvider);
            //3、开启调度器
            await _scheduler.Start();
            //4、创建任务
            var redPacketExpiryJob = JobBuilder.Create<RedPacketExpiryJob>()
                .WithIdentity(nameof(RedPacketExpiryJob), "RedPacketJob")
                .Build();

            //5、创建一个触发器
            var redPacketExpiryTrigger = TriggerBuilder.Create()
                            .WithIdentity("redPacketExpiryTrigger", "redPacketTrigger")
                            .WithSimpleSchedule(x => x.WithIntervalInSeconds(1).RepeatForever())
                            .Build();

            //6、将触发器和任务器绑定到调度器中
            await _scheduler.ScheduleJob(redPacketExpiryJob, redPacketExpiryTrigger);
        }

        public void Stop()
        {
            if (_scheduler != null)
            {
                _scheduler.Clear();
                _scheduler.Shutdown();
            }
            _scheduler = null;
            _factory = null;
        }
    }
}
