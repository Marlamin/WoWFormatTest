using Microsoft.AspNetCore.Mvc;

namespace DBCDumpHost.Controllers
{
    public class HomeController : ControllerBase
    {
        [Route("")]
        public ActionResult<string> Index()
        {
            return "Hello!";
        }
    }
}
