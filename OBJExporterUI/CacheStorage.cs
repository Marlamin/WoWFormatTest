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

        public Dictionary<string, PreviewWindow.DoodadBatch> doodadBatches = new Dictionary<string, PreviewWindow.DoodadBatch>();
        public Dictionary<string, PreviewWindow.WorldModel> worldModelBatches = new Dictionary<string, PreviewWindow.WorldModel>();

        public CacheStorage()
        {
            
        }
    }
}
