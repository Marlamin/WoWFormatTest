using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSDBCReader;
using System.IO;

namespace WoWFormatTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //Just reading stuff from HDD, screw mpqs
            string basedir = "C:\\WoD\\18322_full\\";
            MapReader reader = new MapReader();
            Dictionary<int, string> maps = reader.GetMaps();
            foreach (KeyValuePair<int, string> map in maps)
            {
                if (File.Exists(basedir + "World\\Maps\\" + map.Value + "\\" + map.Value + ".wdt"))
                {
                    WDTReader wdtreader = new WDTReader();
                    wdtreader.LoadWDT(basedir, map.Value);
                }
                else
                {
                    Console.WriteLine("Map \"" + map.Value + "\" DOES NOT EXIST!");
                }
            }
        }
    }
}
