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
        public IActionResult Index()
        {
            return Content("LeanCode rox!");
        }
    }
}
