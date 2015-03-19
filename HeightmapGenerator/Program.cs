using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWFormatLib;
using WoWFormatLib.DBC;
using WoWFormatLib.FileReaders;
using WoWFormatLib.Structs.ADT;
using WoWFormatLib.Utils;

namespace HeightmapGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing CASC..");
            CASC.InitCasc(null, "D:/Games/World of Warcraft Public Test", "wowt"); // Use PTR for now
            Console.WriteLine("CASC initialized!");

            string mapname = "Draenor";

            
            int counter = 0;

            double lowest = double.MaxValue;
            double highest = double.MinValue;

            var min_x = 64;
            var min_y = 64;

            var max_x = 0;
            var max_y = 0;

            for (int y = 0; y <= 63; ++y)
            {
                for (int x = 0; x <= 63; ++x)
                {
                    string filename = "World/Maps/" + mapname + "/" + mapname + "_" + y + "_" + x + ".adt";
                    if (CASC.FileExists(filename))
                    {
                        ADTReader reader = new ADTReader();
                        reader.LoadADT(filename);

                        for (var i = 0; i < 16; ++i)
                        {
                            for (var j = 0; j < 16; ++j)
                            {
                                if (Math.Round(reader.adtfile.chunks[counter].header.position.Z) > highest) { highest = Math.Round(reader.adtfile.chunks[counter].header.position.Z); }
                                if (Math.Round(reader.adtfile.chunks[counter].header.position.Z) < lowest) { lowest = Math.Round(reader.adtfile.chunks[counter].header.position.Z); }
                                counter++;
                            }
                        }

                        if (x > max_x) { max_x = x; }
                        if (y > max_y) { max_y = y; }

                        if (x < min_x) { min_x = x; }
                        if (y < min_y) { min_y = y; }

                        counter = 0;
                    }
                }
            }
            
            
            Console.WriteLine("Highest: " + highest);
            Console.WriteLine("Lowest: " + lowest);

            highest = highest + (lowest * -1);
            lowest = (lowest * -1);
            
            var res_x = ((max_x - min_x) * 16) + 32;
            var res_y = ((max_y - min_y) * 16) + 32;

            var fullmap = new Bitmap(res_y, res_x);

            for (int y = 0; y <= 63; ++y)
            {
                for (int x = 0; x <= 63; ++x)
                {
                    string filename = "World/Maps/" + mapname + "/" + mapname + "_" + y + "_" + x + ".adt";
                    if (CASC.FileExists(filename))
                    {
                        ADTReader reader = new ADTReader();
                        reader.LoadADT(filename);

                        for (var i = 1; i < 17; ++i)
                        {
                            for (var j = 1; j < 17; ++j)
                            {
                                int greyness = (int)Math.Round((reader.adtfile.chunks[counter].header.position.Z + lowest) / highest * 255);

                                fullmap.SetPixel(((y - min_y) * 16) + j, ((x - min_x) * 16) + i, Color.FromArgb(greyness, greyness, greyness));

                                counter++;
                            }
                        }
                        counter = 0;
                    }
                    else
                    {
                        for (var i = 1; i < 17; ++i)
                        {
                            for (var j = 1; j < 17; ++j)
                            {
                                var target_x = ((x - min_x) * 16) + i;
                                var target_y = ((y - min_y) * 16) + j;

                                if ((target_x < res_x && target_x > 0) && (target_y < res_y && target_y > 0))
                                {
                                    fullmap.SetPixel(target_y, target_x, Color.FromArgb(0, 0, 0));
                                }
                            }
                        }
                    }
                }
            }

            fullmap.Save("Z:/" + mapname + "_heightmap.png");
        }
    }
}
