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
            //LoadAllMaps(basedir);

                LoadFromListfile("C:\\WoD\\18443listfile.txt", basedir);

        }

        static void LoadFromListfile(string listfile, string basedir)
        {
            string line;
            StreamReader file = new System.IO.StreamReader(listfile);
            while ((line = file.ReadLine()) != null)
            {
                try
                {
                    if (line.EndsWith(".m2", StringComparison.OrdinalIgnoreCase))
                    {
                        //Console.WriteLine("Loading M2: " + line);
                        M2Reader reader = new M2Reader(basedir);
                        reader.LoadM2(line);
                    }
                    else if (line.EndsWith(".wmo", StringComparison.OrdinalIgnoreCase))
                    {
                        //Console.WriteLine("Loading WMO: " + line);
                        WMOReader reader = new WMOReader(basedir);
                        reader.LoadWMO(line);
                    }
                    else if (line.EndsWith(".wdt", StringComparison.OrdinalIgnoreCase))
                    {
                        //we do these with loadmaps
                        /*
                        Console.WriteLine("Loading WDT: " + line);
                        var filename = line.Replace("World\\Maps\\", "");
                        var splitfilename = filename.Split(new char[] { '\\' });
                        WDTReader reader = new WDTReader(basedir);
                        reader.LoadWDT(splitfilename[0]);
                         */
                    }
                    else if (line.EndsWith(".adt", StringComparison.OrdinalIgnoreCase))
                    {
                        //We don't need to load ADTs manually, they all get picked up via wdt
                    }
                    else if (
                        line.EndsWith(".blp", StringComparison.OrdinalIgnoreCase) || 
                        line.EndsWith(".wdl", StringComparison.OrdinalIgnoreCase) || 
                        line.EndsWith(".tex", StringComparison.OrdinalIgnoreCase) ||
                        line.EndsWith(".skin", StringComparison.OrdinalIgnoreCase) ||
                        line.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase) ||
                        line.EndsWith(".anim", StringComparison.OrdinalIgnoreCase) ||
                        line.EndsWith(".lua", StringComparison.OrdinalIgnoreCase) ||
                        line.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) ||
                        line.EndsWith(".html", StringComparison.OrdinalIgnoreCase) ||
                        line.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) ||
                        line.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) ||
                        line.EndsWith(".bls", StringComparison.OrdinalIgnoreCase) ||
                        line.EndsWith(".sig", StringComparison.OrdinalIgnoreCase) ||
                        line.EndsWith(".avi", StringComparison.OrdinalIgnoreCase) ||
                        line.EndsWith(".sbt", StringComparison.OrdinalIgnoreCase) ||
                        line.EndsWith(".zmp", StringComparison.OrdinalIgnoreCase) ||
                        line.EndsWith(".toc", StringComparison.OrdinalIgnoreCase) ||
                        line.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) ||
                        line.EndsWith(".dbc", StringComparison.OrdinalIgnoreCase) ||
                        line.EndsWith(".lst", StringComparison.OrdinalIgnoreCase) ||
                        line.EndsWith(".phys", StringComparison.OrdinalIgnoreCase) ||
                        line.EndsWith(".wtf", StringComparison.OrdinalIgnoreCase) ||
                        line.EndsWith(".db2", StringComparison.OrdinalIgnoreCase) ||
                        line.EndsWith(".xsd", StringComparison.OrdinalIgnoreCase)
                    )
                    {
                        //Not yet!
                    }
                    else
                    {
                        Console.WriteLine("Unknown file: " + line);
                    }
                }
                catch (FileNotFoundException e)
                {
                    using (StreamWriter sw = File.AppendText("C:\\Users\\Martin\\Desktop\\missingfiles.txt"))
                    {
                        sw.WriteLine(e.Message);
                    }
                }
            }
            file.Close();
            Console.WriteLine("Done.");
            Console.ReadLine();
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
