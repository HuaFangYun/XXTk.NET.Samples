using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XXTk.Consul.Api.Controllers
{
    public class HealthCheckController : ApiController
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}
