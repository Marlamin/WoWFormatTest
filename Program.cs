using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSDBCReader;
using System.IO;
using System.Configuration;

namespace WoWFormatTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //Just reading stuff from HDD, screw mpqs
            string basedir = ConfigurationManager.AppSettings["basedir"];
            MapReader reader = new MapReader();
            Dictionary<int, string> maps = reader.GetMaps();
            foreach (KeyValuePair<int, string> map in maps)
            {
                if (File.Exists(Path.Combine(basedir, "World\\Maps\\", map.Value, map.Value + ".wdt")))
                {
                    Console.WriteLine("Loading " + map.Value + "...");
                    WDTReader wdtreader = new WDTReader();
                    wdtreader.LoadWDT(map.Value);
                }
                else
                {
                    Console.WriteLine("Map \"" + map.Value + "\" DOES NOT EXIST!");
                }
            }
        }
    }
}
