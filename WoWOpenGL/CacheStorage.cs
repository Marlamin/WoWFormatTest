using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWOpenGL
{
    class CacheStorage
    {
        public Dictionary<string, WoWFormatLib.Structs.M2.M2Model> models = new Dictionary<string, WoWFormatLib.Structs.M2.M2Model>();
        public Dictionary<string, int> materials = new Dictionary<string, int>();
        public Dictionary<string, WoWFormatLib.Structs.WMO.WMO> worldModels = new Dictionary<string, WoWFormatLib.Structs.WMO.WMO>();

        public Dictionary<string, TerrainWindow.DoodadBatch> doodadBatches = new Dictionary<string, TerrainWindow.DoodadBatch>();
        public Dictionary<string, TerrainWindow.WorldModel> worldModelBatches = new Dictionary<string, TerrainWindow.WorldModel>();

        public CacheStorage()
        {
            
        }
    }
}
