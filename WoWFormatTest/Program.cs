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
            string basedir = ConfigurationManager.AppSettings["basedir"];
            LoadAllMaps(basedir);
        }

        static void LoadAllMaps(string basedir)
        {
            var reader = new MapReader(basedir);
            Dictionary<int, string> maps = reader.GetMaps();
            foreach (KeyValuePair<int, string> map in maps)
            {
                LoadMap(map.Value, basedir);
            }
        }
        static void LoadMap(string map, string basedir)
        {
            
            if (File.Exists(Path.Combine(basedir, "World\\Maps\\", map, map + ".wdt")))
            {
                Console.WriteLine("Loading " + map + "...");
                var wdtreader = new WDTReader(basedir);
                try
                {
                    wdtreader.LoadWDT(map);
                }
                catch (FileNotFoundException e)
                {
                    using (StreamWriter sw = File.AppendText("C:\\Users\\Martin\\Desktop\\missingfiles.txt"))
                    {
                        sw.WriteLine(e.Message);
                    }
                }
               
            }
            else
            {
                Console.WriteLine("Map \"" + map + "\" does not exist.");
            }
        }

    }
}
