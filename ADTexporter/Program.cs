using System;
using CASCExplorer;
using WoWFormatLib.Utils;

namespace ADTexporter
{
    class Program
    {
        static void Main()
        {
            CASC.InitCasc(null, null, "wow_beta");
            var tw = new TerrainWindow("Azeroth_32_32", null);
            tw.Run();
        }
    }
}
