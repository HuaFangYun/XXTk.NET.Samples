using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XXTk.Consul.Api.Controllers
{
    public class HelloController : ApiController
    {
        private readonly ILogger<HelloController> _logger;

        public HelloController(ILogger<HelloController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get()
        {
            return $"Hello !!! {DateTime.Now:o}";
        }

        [HttpGet("GetNames")]
        public IEnumerable<string> GetNames()
        {
            return new[]
            {
                "jjj",
                "kkk",
                "lll"
            };
        }
    }
}
