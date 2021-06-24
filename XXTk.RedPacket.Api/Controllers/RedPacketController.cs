using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XXTk.RedPacket.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RedPacketController : ControllerBase
    {
        private readonly ILogger<RedPacketController> _logger;
        private readonly RedPacketHelper _redPacketHelper;

        public RedPacketController(
            ILogger<RedPacketController> logger,
            RedPacketHelper redPacketHelper)
        {
            _logger = logger;
            _redPacketHelper = redPacketHelper;
        }

        /// <summary>
        /// 发送红包
        /// </summary>
        /// <param name="totalMoney"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        [HttpPost]
        public string Post([FromQuery] int totalMoney, [FromQuery] int count)
        {
            return _redPacketHelper.CreateRedPacket(totalMoney, count);
        }

        /// <summary>
        /// 抢红包
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public decimal Get([FromRoute] string id)
        {
            var userId = $"User-{DateTime.Now:O}";
            var money = _redPacketHelper.GetRedPacket(id, userId);                

            return money ?? throw new Exception("来晚了，红包已被抢光");
        }

        [HttpGet("GetRedPacketRecords/{id}")]
        public ActionResult<List<RedPacketRecord>> GetRedPacketRecords([FromRoute] string id)
        {
            return _redPacketHelper.GetRedPacketRecords(id);
        }
    }
}
