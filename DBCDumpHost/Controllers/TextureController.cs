using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace DBCDumpHost.Controllers
{
    [Route("api/itemtexture")]
    [Route("api/texture")]
    [ApiController]
    public class TextureController : ControllerBase
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
            var creatureModelData = DBCManager.LoadDBC("creaturemodeldata", build, true);
            var creatureDisplayInfo = DBCManager.LoadDBC("creaturedisplayinfo", build, true);

            var returnList = new Dictionary<uint, List<uint>>();

            if (modelFileData.Contains(filedataid))
            {
                dynamic mfdEntry = modelFileData[filedataid];

                foreach (dynamic idiEntry in itemDisplayInfo.Values)
                {
                    if (idiEntry.ModelResourcesID[0] != mfdEntry.ModelResourcesID && idiEntry.ModelResourcesID[1] != mfdEntry.ModelResourcesID)
                    {
                        continue;
                    }

                    var textureFileDataList = new List<uint>();

                    foreach (dynamic tfdEntry in textureFileData.Values)
                    {
                        if (tfdEntry.MaterialResourcesID == idiEntry.ModelMaterialResourcesID[0] || tfdEntry.MaterialResourcesID == idiEntry.ModelMaterialResourcesID[1])
                        {
                            textureFileDataList.Add((uint)tfdEntry.FileDataID);
                        }
                    }

                    returnList.Add((uint)idiEntry.ID, textureFileDataList);
                }

                foreach (dynamic cmdEntry in creatureModelData.Values)
                {
                    if (cmdEntry.FileDataID != filedataid)
                    {
                        continue;
                    }

                    foreach (dynamic cdiEntry in creatureDisplayInfo.Values)
                    {
                        if (cdiEntry.ModelID != cmdEntry.ID)
                        {
                            continue;
                        }

                        //returnList.Add((uint)cdiEntry.ID, new List<uint> { (uint)cdiEntry.TextureVariationFileDataID[0], (uint)cdiEntry.TextureVariationFileDataID[1], (uint)cdiEntry.TextureVariationFileDataID[2] });
                        returnList.Add((uint)cdiEntry.ID, new List<uint> { (uint)cdiEntry.TextureVariationFileDataID[0] });
                    }

                    break;
                }
            }

            return returnList;
        }
    }
}
