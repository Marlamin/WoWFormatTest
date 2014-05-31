using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSDBCReader;
using System.IO;
using System.Configuration;
using WoWFormatLib.DBC;
using WoWFormatLib.FileReaders;

namespace WoWFormatLib
{
    class Program
    {
        static void Main(string[] args)
        {
            LoadAllMaps();
        }

        static void LoadAllMaps()
        {
            var reader = new MapReader();
            Dictionary<int, string> maps = reader.GetMaps();
            foreach (KeyValuePair<int, string> map in maps)
            {
                LoadMap(map.Value);
            }
        }
        static void LoadMap(string map)
        {
            string basedir = ConfigurationManager.AppSettings["basedir"];
            if (File.Exists(Path.Combine(basedir, "World\\Maps\\", map, map + ".wdt")))
            {
                Console.WriteLine("Loading " + map + "...");
                var wdtreader = new WDTReader();
                wdtreader.LoadWDT(map);
            }
            else
            {
                Console.WriteLine("Map \"" + map + "\" DOES NOT EXIST!");
            }
        }

    }
}
