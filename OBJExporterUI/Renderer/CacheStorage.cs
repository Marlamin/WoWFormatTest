using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBJExporterUI
{
    class CacheStorage
    {
        public Dictionary<string, WoWFormatLib.Structs.M2.M2Model> models = new Dictionary<string, WoWFormatLib.Structs.M2.M2Model>();
        public Dictionary<string, int> materials = new Dictionary<string, int>();
        public Dictionary<string, WoWFormatLib.Structs.WMO.WMO> worldModels = new Dictionary<string, WoWFormatLib.Structs.WMO.WMO>();

        public Dictionary<string, Renderer.Structs.DoodadBatch> doodadBatches = new Dictionary<string, Renderer.Structs.DoodadBatch>();
        public Dictionary<string, Renderer.Structs.WorldModel> worldModelBatches = new Dictionary<string, Renderer.Structs.WorldModel>();

        public Dictionary<string, Renderer.Structs.Terrain> terrain = new Dictionary<string, Renderer.Structs.Terrain>();

        public CacheStorage()
        {
            
        }
    }
}
