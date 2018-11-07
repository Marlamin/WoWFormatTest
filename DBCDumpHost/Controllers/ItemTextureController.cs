using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CascStorageLib;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace DBCDumpHost.Controllers
{
    [Route("api/itemtexture")]
    [ApiController]
    public class ItemTextureController : ControllerBase
    {
        // GET: data/
        [HttpGet]
        public string Get()
        {
            return "No filedataid given!";
        }

        public IDictionary LoadDBC(string name, string build)
        {
            var filename = Path.Combine(SettingManager.dbcDir, build, "dbfilesclient", name + ".db2");
            var rawType = DefinitionManager.CompileDefinition(filename, build);
            var type = typeof(Storage<>).MakeGenericType(rawType);
            return (IDictionary)Activator.CreateInstance(type, filename);
        }

        // GET: data/name
        [HttpGet("{filedataid}")]
        public uint[] Get(int filedataid, string build)
        {
            var modelFileData = LoadDBC("modelfiledata", build);
            var itemDisplayInfo = LoadDBC("itemdisplayinfo", build);
            var textureFileData = LoadDBC("texturefiledata", build);

            var returnList = new List<uint>();

            if (modelFileData.Contains(filedataid))
            {
                dynamic mfdEntry = modelFileData[filedataid];

                foreach (dynamic idiEntry in itemDisplayInfo.Values)
                {
                    if (idiEntry.ModelResourcesID[0] != mfdEntry.ModelResourcesID)
                    {
                        continue;
                    }

                    foreach (dynamic tfdEntry in textureFileData.Values)
                    {
                        if (tfdEntry.MaterialResourcesID == idiEntry.ModelMaterialResourcesID[0])
                        {
                            returnList.Add((uint)tfdEntry.FileDataID);
                        }
                    }
                }
            }

            return returnList.ToArray();
        }
    }
}
