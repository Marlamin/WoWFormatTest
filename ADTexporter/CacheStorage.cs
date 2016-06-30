using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADTexporter
{
    class CacheStorage
    {
        public Dictionary<string, WoWFormatLib.Structs.M2.M2Model> models = new Dictionary<string, WoWFormatLib.Structs.M2.M2Model>();
        public Dictionary<string, int> materials = new Dictionary<string, int>();
        public Dictionary<string, WoWFormatLib.Structs.WMO.WMO> worldModels = new Dictionary<string, WoWFormatLib.Structs.WMO.WMO>();

        public Dictionary<string, DoodadBatch> doodadBatches = new Dictionary<string, DoodadBatch>();
        public Dictionary<string, WorldModel> worldModelBatches = new Dictionary<string, WorldModel>();

        public CacheStorage()
        {
            
        }
    }
}
