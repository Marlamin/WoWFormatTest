using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWFormatLib.Utils;

namespace MinimapCompiler
{
    public static class Listfile
    {
        public static Dictionary<uint, string> fdidToNameMap = new Dictionary<uint, string>();
        public static Dictionary<string, uint> nameToFDIDMap = new Dictionary<string, uint>();

        public static void Load()
        {
            if (!File.Exists("listfile.csv"))
            {
                throw new FileNotFoundException("Listfile.csv not found, download it at http://wow.tools/files/");
            }

            foreach (var line in File.ReadAllLines("listfile.csv"))
            {
                if (line.Length == 0)
                    continue;

                var splitLine = line.Split(';');

                if (!splitLine[1].StartsWith("world"))
                    continue;

                var fdid = uint.Parse(splitLine[0]);

                if (!CASC.FileExists(fdid))
                    continue;

                fdidToNameMap.Add(fdid, splitLine[1]);
                nameToFDIDMap.Add(splitLine[1], fdid);
            }
        }
    }
}
