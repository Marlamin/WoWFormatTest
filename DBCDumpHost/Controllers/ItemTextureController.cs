using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

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
        public Dictionary<uint, List<uint>> Get(int filedataid, string build)
        {
            var modelFileData = DBCManager.LoadDBC("modelfiledata", build, true);
            var itemDisplayInfo = DBCManager.LoadDBC("itemdisplayinfo", build, true);
            var textureFileData = DBCManager.LoadDBC("texturefiledata", build, true);

            var returnList = new Dictionary<uint, List<uint>>();

            if (modelFileData.Contains(filedataid))
            {
                dynamic mfdEntry = modelFileData[filedataid];

                foreach (dynamic idiEntry in itemDisplayInfo.Values)
                {
                    var textureFileDataList = new List<uint>();
                    if (idiEntry.ModelResourcesID[0] != mfdEntry.ModelResourcesID && idiEntry.ModelResourcesID[1] != mfdEntry.ModelResourcesID)
                    {
                        continue;
                    }

                    foreach (dynamic tfdEntry in textureFileData.Values)
                    {
                        if (tfdEntry.MaterialResourcesID == idiEntry.ModelMaterialResourcesID[0] || tfdEntry.MaterialResourcesID == idiEntry.ModelMaterialResourcesID[1])
                        {
                            textureFileDataList.Add((uint)tfdEntry.FileDataID);
                        }
                    }

                    returnList.Add((uint)idiEntry.ID, textureFileDataList);
                }
            }

            return returnList;
        }
    }
}
