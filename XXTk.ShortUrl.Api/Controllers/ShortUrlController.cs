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
        public async Task<string> GetShortUrl([FromBody] string longUrl)
        {
            return await _shortUrlHelper.GetShortUrl(longUrl);
        }

        /// <summary>
        /// 获取长连接
        /// </summary>
        /// <param name="shortUrl"></param>
        /// <returns></returns>
        [HttpGet("GetLongUrl")]
        public async Task<string> GetLongUrl(string shortUrl)
        {
            return await _shortUrlHelper.GetLongUrl(shortUrl);
        }

        [HttpGet("{shortUrl}")]
        public async Task<IActionResult> Get(string shortUrl)
        {
            var longUrl = await _shortUrlHelper.GetLongUrl(shortUrl);
            if(longUrl is null)
            {
                return NotFound();
            }

            return Redirect(longUrl);
        }
    }
}
