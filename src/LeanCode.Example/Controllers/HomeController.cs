using System;
using System.Threading.Tasks;
using LeanCode.CQRS;
using Microsoft.AspNetCore.Mvc;

namespace LeanCode.Example.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly ICqrs cqrs;

        public HomeController(ICqrs cqrs)
        {
            this.cqrs = cqrs;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var result = await cqrs.QueryAsync(new CQRS.SampleQuery());
            return Content(result.Name);
        }

        [HttpGet("do")]
        public async Task<IActionResult> DoAction()
        {
            try
            {
                await cqrs.ExecuteAsync(new CQRS.SampleCommand());
                return Content("Everything's OK");
            }
            catch (Exception e)
            {
                return Content("Exception: " + e.Message);
            }
        }
    }
}
