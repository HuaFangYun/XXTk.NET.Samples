using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XXTk.ShortUrl.Api.Redis;

namespace XXTk.ShortUrl.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ShortUrlController : ControllerBase
    {
        private readonly ILogger<ShortUrlController> _logger;
        private readonly ShortUrlHelper _shortUrlHelper;

        public ShortUrlController(
            ILogger<ShortUrlController> logger,
            ShortUrlHelper shortUrlHelper)
        {
            _logger = logger;
            _shortUrlHelper = shortUrlHelper;
        }

        /// <summary>
        /// 获取短连接
        /// </summary>
        /// <param name="longUrl"></param>
        /// <returns></returns>
        /// <example>
        /// <code>"https://www.baidu.com"</code>
        /// </example>
        [HttpPost("GetShortUrl")]
        public string GetShortUrl([FromBody] string longUrl)
        {
            return _shortUrlHelper.GetShortUrl(longUrl);
        }

        /// <summary>
        /// 获取长连接
        /// </summary>
        /// <param name="shortUrl"></param>
        /// <returns></returns>
        [HttpGet("GetLongUrl")]
        public string GetLongUrl(string shortUrl)
        {
            return _shortUrlHelper.GetLongUrl(shortUrl);
        }

        [HttpGet("{shortUrl}")]
        public IActionResult Get(string shortUrl)
        {
            var longUrl = _shortUrlHelper.GetLongUrl(shortUrl);
            if(longUrl is null)
            {
                return NotFound();
            }

            return Redirect(longUrl);
        }
    }
}
