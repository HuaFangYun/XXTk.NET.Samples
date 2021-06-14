using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace XXTk.Consul.EmailSender.Controllers
{
    public class MonitorController : ApiController
    {
        private readonly ILogger<MonitorController> _logger;
        private readonly INoticeEmailSender _noticeEmailSender;

        public MonitorController(
            ILogger<MonitorController> logger,
            INoticeEmailSender noticeEmailSender)
        {
            _logger = logger;
            _noticeEmailSender = noticeEmailSender;
        }

        [HttpPost("Notice")]
        public async Task<IActionResult> Notice()
        {
            var message = await JsonSerializer.DeserializeAsync<IEnumerable<ConsulMessage>>(Request.Body);
            await _noticeEmailSender.SendAsync(message);

            return Ok();
        }

        [HttpGet]
        public string Get()
        {
            return "Monitor";
        }
    }
}
