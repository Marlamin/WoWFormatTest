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

        // GET: data/name
        [HttpGet("{filedataid}")]
        public uint[] Get(int filedataid, string build)
        {
            var modelFileData = DBCManager.LoadDBC("modelfiledata", build, true);
            var itemDisplayInfo = DBCManager.LoadDBC("itemdisplayinfo", build, true);
            var textureFileData = DBCManager.LoadDBC("texturefiledata", build, true);

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
