using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WoWFormatLib;
using WoWFormatLib.DBC;
using WoWFormatLib.FileReaders;
using WoWFormatLib.Utils;

namespace MinimapCompiler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string mapname = "";

            string basedir = ConfigurationManager.AppSettings["basedir"];
            bool buildmaps = Boolean.Parse(ConfigurationManager.AppSettings["buildmaps"]);
            bool buildWMOmaps = Boolean.Parse(ConfigurationManager.AppSettings["buildwmomaps"]);

            Console.WriteLine("Initializing CASC..");

            if(basedir != String.Empty){
                CASC.InitCasc(null, basedir);
            }else{
                CASC.InitCasc(null, null, "wowt"); // Use PTR for now
            }

            Console.WriteLine("CASC initialized!");

            if (buildmaps == true)
            {
                DBCReader<MapRecord> reader = new DBCReader<MapRecord>("DBFilesClient\\Map.dbc");
                for (int i = 0; i < reader.recordCount; i++)
                {
                    //I used to check if WDT existed, but sometimes minimaps for maps without WDTs slip through the cracks

                    mapname = reader[i].Directory;

                    var min_x = 64;
                    var min_y = 64;

                    var max_x = 0;
                    var max_y = 0;

                    for (int cur_x = 0; cur_x < 64; cur_x++)
                    {
                        for (int cur_y = 0; cur_y < 64; cur_y++)
                        {
                            if (CASC.FileExists("World\\Minimaps\\" + mapname + "\\map" + cur_x + "_" + cur_y + ".blp"))
                            {
                                if (cur_x > max_x) { max_x = cur_x; }
                                if (cur_y > max_y) { max_y = cur_y; }

                                if (cur_x < min_x) { min_x = cur_x; }
                                if (cur_y < min_y) { min_y = cur_y; }
                            }
                        }
                    }

                    //Console.WriteLine("[" + mapname + "] MIN: (" + min_x + " " + min_y + ") MAX: (" + max_x + " " + max_y + ")");

                    var res_x = (((max_x - min_x) * 256) + 256);
                    var res_y = (((max_y - min_y) * 256) + 256);

                    if (res_x < 0 || res_y < 0)
                    {
                        Console.WriteLine("[" + mapname + "] " + "Skipping map, has no minimap tiles");
                        continue;
                    }

                    Console.WriteLine("[" + mapname + "] " + "Creating new image of " + res_x + "x" + res_y);

                    Bitmap bmp = new Bitmap(res_x, res_y);
                    Graphics g = Graphics.FromImage(bmp);
                    Font drawFont = new Font("Arial", 16);

                    for (int cur_x = 0; cur_x < 64; cur_x++)
                    {
                        for (int cur_y = 0; cur_y < 64; cur_y++)
                        {
                            if (CASC.FileExists("World\\Minimaps\\" + mapname + "\\map" + cur_x + "_" + cur_y + ".blp"))
                            { 
                                var blpreader = new BLPReader();
                                blpreader.LoadBLP("World\\Minimaps\\" + mapname + "\\map" + cur_x + "_" + cur_y + ".blp");
                                g.DrawImage(blpreader.bmp, (cur_x - min_x) * 256, (cur_y - min_y) * 256, new Rectangle(0, 0, 256, 256), GraphicsUnit.Pixel);
                            }
                        }
                    }
                    g.Dispose();
                    if (!Directory.Exists("done")) { Directory.CreateDirectory("done"); }
                    bmp.Save("done/" + mapname + ".png");
                }
            }
                /*

                foreach (string mapdir in Directory.GetDirectories(basedir + Path.Combine("World", "Minimaps")))
                {

                    mapname = mapdir.Replace(basedir, String.Empty).Replace("World" + Path.DirectorySeparatorChar + "Minimaps" + Path.DirectorySeparatorChar, String.Empty);
                    Console.WriteLine(mapname);
                    //Console.ReadLine();
                    if (mapname == "WMO") { continue; } //Skip WMO subdirectory
                    var min_x = 64;
                    var min_y = 64;

                    var max_x = 0;
                    var max_y = 0;
                    foreach (string s in Directory.GetFiles(basedir + Path.Combine("World", "Minimaps") + Path.DirectorySeparatorChar + mapname + Path.DirectorySeparatorChar, "map*.blp", SearchOption.AllDirectories))
                    {
                        var location = s.Substring(s.Length - 9, 5);

                        var x = int.Parse(location.Substring(0, 2));
                        var y = int.Parse(location.Substring(3, 2));

                        if (x > max_x) { max_x = x; }
                        if (y > max_y) { max_y = y; }

                        if (x < min_x) { min_x = x; }
                        if (y < min_y) { min_y = y; }
                    }

                    Console.WriteLine("[" + mapname + "] MIN: " + min_x + " " + min_y);
                    Console.WriteLine("[" + mapname + "] MAX: " + max_x + " " + max_y);

                    var res_x = (((max_x - min_x) * 256) + 256);
                    var res_y = (((max_y - min_y) * 256) + 256);

                    Console.WriteLine("[" + mapname + "] " + "Creating new image of " + res_x + "x" + res_y);
                    Bitmap bmp = new Bitmap(res_x, res_y);
                    Graphics g = Graphics.FromImage(bmp);
                    Font drawFont = new Font("Arial", 16);

                    foreach (string s in Directory.GetFiles(basedir + Path.Combine("World", "Minimaps") + Path.DirectorySeparatorChar + mapname + Path.DirectorySeparatorChar, "map*.blp", SearchOption.AllDirectories))
                    {
                        var location = s.Substring(s.Length - 9, 5);

                        var x = int.Parse(location.Substring(0, 2));
                        var y = int.Parse(location.Substring(3, 2));

                        var blpreader = new BLPReader();
                        blpreader.LoadBLP(s.Replace(basedir, ""));
                        g.DrawImage(blpreader.bmp, (x - min_x) * 256, (y - min_y) * 256, new Rectangle(0, 0, 256, 256), GraphicsUnit.Pixel);
                    }

                    g.Dispose();
                    if (!Directory.Exists("done")) { Directory.CreateDirectory("done"); }
                    bmp.Save("done/" + mapname + ".png");
                }
                 
            }

            if (buildWMOmaps == true)
            {
                string[] unwantedExtensions = new string[513];
                for (int i = 0; i < 512; i++)
                {
                    unwantedExtensions[i] = "_" + i.ToString().PadLeft(3, '0') + ".wmo";
                }
                unwantedExtensions[512] = "LOD1.wmo";
                foreach (string s in Directory.GetFiles(basedir + Path.Combine("World", "WMO"), "*.wmo", SearchOption.AllDirectories))
                {
                    if (!unwantedExtensions.Contains(s.Substring(s.Length - 8, 8)))
                    {
                        if (!s.Contains("LOD"))
                        {
                            WMO wmocompiler = new WMO(basedir);
                            wmocompiler.Compile(s.Replace(basedir, String.Empty));
                        }
                    }
                }
            }*/
        }
    }
}