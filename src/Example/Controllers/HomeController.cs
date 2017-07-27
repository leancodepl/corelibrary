using System;
using System.Threading.Tasks;
using LeanCode.CQRS.Execution;
using LeanCode.CQRS.RemoteHttp.Client;
using Microsoft.AspNetCore.Mvc;

namespace LeanCode.Example.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly ICommandExecutor<AppContext> commands;
        private readonly IQueryExecutor<AppContext> queries;

        public HomeController(ICommandExecutor<AppContext> commands, IQueryExecutor<AppContext> queries)
        {
            this.commands = commands;
            this.queries = queries;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var ctx = new AppContext { User = User };
            var result = await queries.GetAsync(ctx, new CQRS.SampleQuery());
            return Content(result.Name);
        }

        [HttpGet("push")]
        public IActionResult PushNotifications()
        {
            return Redirect("push/index.html");
        }

        [HttpGet("do")]
        public async Task<IActionResult> DoAction()
        {
            try
            {
                var ctx = new AppContext { User = User };
                await commands.RunAsync(ctx, new CQRS.SampleCommand("Name"));
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
            return Json(await client.GetAsync(new CQRS.SampleQuery()));
        }

        [HttpGet("remote/do")]
        public async Task<IActionResult> RemoteCommand(string name = "")
        {
            var client = new HttpCommandsExecutor(new Uri("http://localhost:5000/api/"));
            await client.RunAsync(new CQRS.SampleCommand(name));
            return Json(await client.RunAsync(new CQRS.SampleCommand(name)));
        }
    }
}
