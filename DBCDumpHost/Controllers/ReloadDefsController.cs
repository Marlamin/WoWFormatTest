using Microsoft.AspNetCore.Mvc;

namespace DBCDumpHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReloadDefsController : ControllerBase
    {
        // GET: api/ReloadDefs
        [HttpGet]
        public string Get()
        {
            DefinitionManager.LoadDefinitions();
            return "Reloaded " + DefinitionManager.definitionLookup.Count + " definitions!";
        }
    }
}
