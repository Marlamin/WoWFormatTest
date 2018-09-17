using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
            DefinitionManager.LoadDefinitions("definitions");
            return "Reloaded " + DefinitionManager.definitionLookup.Count + " definitions!";
        }
    }
}
