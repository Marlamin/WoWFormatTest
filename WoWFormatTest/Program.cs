using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using WoWFormatLib.DBC;
using WoWFormatLib.FileReaders;
using WoWFormatLib.Utils;

namespace WoWFormatLib
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CASC.InitCasc(null, null, "wowt", CASCLib.LocaleFlags.enUS);

            if (!File.Exists("listfile.txt"))
            {
                throw new Exception("Listfile not found!");
            }

            foreach (var line in File.ReadAllLines("listfile.txt"))
            {
                if (line.EndsWith(".wdt", StringComparison.CurrentCultureIgnoreCase) && !line.Contains("_mpv") && !line.Contains("_fogs") && !line.Contains("_occ") && !line.Contains("_lgt"))
                {
                    if (!CASC.FileExists(line))
                        continue;

                    var reader = new WDTReader();
                    reader.LoadWDT(line);

                    if(reader.wdtfile.filedataids == null)
                    {
                        Console.WriteLine(line + " does not have MAID chunk");
                        continue;
                    }

                    foreach(var filedataids in reader.wdtfile.filedataids)
                    {
                        if (filedataids.rootADT != 0)
                        {
                            Console.WriteLine(line + " " + filedataids.rootADT);
                        }
                    }
                }
            }

        }
    }
}
