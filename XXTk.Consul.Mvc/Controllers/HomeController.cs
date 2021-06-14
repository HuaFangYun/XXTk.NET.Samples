using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using XXTk.Consul.Mvc.Models;
using XXTk.Consul.Mvc.Services;

namespace XXTk.Consul.Mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMyAppService _myAppService;

        public HomeController(
            ILogger<HomeController> logger,
            IMyAppService myAppService)
        {
            _logger = logger;
            _myAppService = myAppService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new IndexViewModel
            {
                Hello = await _myAppService.GetHelloAysnc(),
                Names = await _myAppService.GetNamesAsync()
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
