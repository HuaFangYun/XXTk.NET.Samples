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
    /// 将很长的Url转换为很短的Url
    /// 常用于“分享”功能，例如，通过手机B站的分享功能，将视频源地址 https://m.bilibili.com/video/BV1G54y1z7u4?p=1&share_medium=android&share_plat=android&share_source=COPY&share_tag=s_i&timestamp=1624343913&unique_k=gauzfq 转换为 https://b23.tv/gauzfq
    /// 
    /// 
    /// 参考：https://zhuanlan.zhihu.com/p/267979620
    /// 常见方案：
    ///     一、发号器，参见 https://www.zhihu.com/question/29270034
    ///     1.使用自增的方式，生成一个10进制的编号
    ///     2.将10进制转换为62进制
    ///     3.存储在 hashtable 中（key：编号 value：长链接），并设置过期时间（如1小时）
    ///     4.使用滑动过期时间，当短连接被访问时，将其过期时间重新设置为1小时
    ///     二、初始时生成
    ///     例如，当视频刚生成时，便根据视频的相对url生成short url
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
