using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XXTk.ShortUrl.Api
{
    /// <summary>
    /// ���ܳ���Urlת��Ϊ�̵ܶ�Url
    /// �����ڡ��������ܣ����磬ͨ���ֻ�Bվ�ķ����ܣ�����ƵԴ��ַ https://m.bilibili.com/video/BV1G54y1z7u4?p=1&share_medium=android&share_plat=android&share_source=COPY&share_tag=s_i&timestamp=1624343913&unique_k=gauzfq ת��Ϊ https://b23.tv/gauzfq
    /// 
    /// 
    /// �ο���https://zhuanlan.zhihu.com/p/267979620
    /// ����������
    ///     һ�����������μ� https://www.zhihu.com/question/29270034
    ///     1.ʹ�������ķ�ʽ������һ��10���Ƶı��
    ///     2.��10����ת��Ϊ62����
    ///     3.�洢�� hashtable �У�key����� value�������ӣ��������ù���ʱ�䣨��1Сʱ��
    ///     4.ʹ�û�������ʱ�䣬�������ӱ�����ʱ���������ʱ����������Ϊ1Сʱ
    ///     ������ʼʱ����
    ///     ���磬����Ƶ������ʱ���������Ƶ�����url����short url
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
