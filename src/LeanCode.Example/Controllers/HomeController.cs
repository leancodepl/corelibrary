using System;
using System.Threading.Tasks;
using LeanCode.CQRS;
using LeanCode.CQRS.RemoteHttp.Client;
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
                await cqrs.ExecuteAsync(new CQRS.SampleCommand("Name"));
                return Content("Everything's OK");
            }
            catch (Exception e)
            {
                return Content("Exception: " + e.Message);
            }
        }

        [HttpGet("remote")]
        public async Task<IActionResult> RemoteQuery()
        {
            var client = new HttpQueriesExecutor(new Uri("http://localhost:5000/api/"));
            return Json(await client.ExecuteQuery(new CQRS.SampleQuery()));
        }

        [HttpGet("remote/do")]
        public async Task<IActionResult> RemoteCommand(string name = "")
        {
            var client = new HttpCommandsExecutor(new Uri("http://localhost:5000/api/"));
            return Json(await client.ExecuteAsync(new CQRS.SampleCommand(name)));
        }
    }
}
