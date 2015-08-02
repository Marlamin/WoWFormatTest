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
            CASC.InitCasc(); // Use PTR for now
            Console.WriteLine("CASC initialized!");

            var dbcreader = new DBCReader<MapRecord>("DBFilesClient\\Map.dbc");

            string mapfilter = "Draenor";
            //string mapfilter = null;
            for (int mi = 0; mi < dbcreader.recordCount; mi++)
            {
                string mapname = dbcreader[mi].Directory;
                if (mapfilter != null)
                {
                    if (mapname != mapfilter) { continue; }
                }
                Console.WriteLine("Creating image for map " + mapname + " (" + dbcreader[mi].Mapname_lang + ")");

                if (File.Exists("done/" + mapname + ".png"))
                {
                    continue;
                }

                float lowest = float.MaxValue;
                float highest = float.MinValue;

                float lowest_chunkpos = float.MaxValue;
                float highest_chunkpos = float.MinValue;

                var min_x = 64;
                var min_y = 64;

                var max_x = 0;
                var max_y = 0;

                Console.Write("Calculating heights..");
                for (int x = 0; x < 63; x++)
                {
                    for (int y = 0; y < 63; y++)
                    {
                        string filename = "World/Maps/" + mapname + "/" + mapname + "_" + y + "_" + x + ".adt";

                        if (CASC.FileExists(filename))
                        {
                            if (x > max_x) { max_x = x; }
                            if (y > max_y) { max_y = y; }

                            if (x < min_x) { min_x = x; }
                            if (y < min_y) { min_y = y; }

                            ADTReader reader = new ADTReader();
                            reader.LoadADT(filename);
                            for (var i = 0; i < 256; i++)
                            {
                                if (reader.adtfile.chunks[i].header.position.Z < lowest_chunkpos) { lowest_chunkpos = reader.adtfile.chunks[i].header.position.Z; }
                                if (reader.adtfile.chunks[i].header.position.Z > highest_chunkpos) { highest_chunkpos = reader.adtfile.chunks[i].header.position.Z; }

                                for (var j = 0; j < 145; ++j)
                                {
                                    //Console.WriteLine(reader.adtfile.chunks[i].vertices.vertices[j]);
                                    if (reader.adtfile.chunks[i].vertices.vertices[j] < lowest) { lowest = reader.adtfile.chunks[i].vertices.vertices[j]; }
                                    if (reader.adtfile.chunks[i].vertices.vertices[j] > highest) { highest = reader.adtfile.chunks[i].vertices.vertices[j]; }
                                }
                            }
                        }
                    }
                }
                Console.Write(" ..done!\n");
            
                Console.WriteLine("Highest: " + highest);
                Console.WriteLine("Lowest: " + lowest);

                Console.WriteLine("Highest chunkpos: " + highest_chunkpos);
                Console.WriteLine("Lowest chunkpos: " + lowest_chunkpos);

                if (lowest_chunkpos < 0)
                {
                    highest = highest + (lowest * -1) + highest_chunkpos;
                    lowest = (lowest * -1) + (lowest_chunkpos * -1);
                }
                else
                {
                    highest = highest + (lowest * -1) + highest_chunkpos;
                    lowest = (lowest * -1) + lowest_chunkpos;
                }

                Console.WriteLine("Highest post-flip: " + highest);
                Console.WriteLine("Lowest post-flip: " + lowest);

                Console.WriteLine("Highest chunkpos post-flip: " + highest_chunkpos);
                Console.WriteLine("Lowest chunkpos post-flip: " + lowest_chunkpos);

                Console.Write("Creating images..");
                for (int x = 0; x < 63; x++)
                {
                    for (int y = 0; y < 63; y++)
                    {
                        string filename = "World/Maps/" + mapname + "/" + mapname + "_" + y + "_" + x + ".adt";
                        if (CASC.FileExists(filename))
                        {
                            ADTReader reader = new ADTReader();
                            reader.LoadADT(filename);
                            var fullmap = new Bitmap(144, 144);
                            for (var a = 0; a < 256; a++)
                            {
                                //Console.WriteLine(reader.adtfile.chunks[a].header.indexX + " x " + reader.adtfile.chunks[a].header.indexY);

                                int img_x = 0;
                                int img_y = 0;

                                int counter = 0;
                                for (var i = 0; i < 17; ++i)
                                {
                                    for (var j = 0; j < (((i % 2) != 0) ? 8 : 9); ++j)
                                    {
                                        if (i % 2 == 0)
                                        {
                                            //only render these
                                            //Console.Write("(" + j +" " + i + ")" + counter + " |" );
                                            //int greyness = (int)Math.Round(((reader.adtfile.chunks[a].vertices.vertices[counter] + reader.adtfile.chunks[a].header.position.Z) + lowest) / highest * 255);
                                           // if (greyness > 255) { greyness = 255; } //those edge cases where rounding just makes it go over 255
                                            if (reader.adtfile.chunks[a].vertexshading.red != null && reader.adtfile.chunks[a].vertexshading.red[counter] != 127)
                                            {   
                                                fullmap.SetPixel(img_x + (int)(reader.adtfile.chunks[a].header.indexX * 9), img_y + (int)(reader.adtfile.chunks[a].header.indexY * 9),
                                                Color.FromArgb(reader.adtfile.chunks[a].vertexshading.blue[counter], reader.adtfile.chunks[a].vertexshading.green[counter], reader.adtfile.chunks[a].vertexshading.red[counter])
                                                );
                                            }
                                            else
                                            {
                                                int greyness = (int)Math.Round(((reader.adtfile.chunks[a].vertices.vertices[counter] + reader.adtfile.chunks[a].header.position.Z) + lowest) / highest * 255);
                                                if (greyness > 255 || greyness < 0) { greyness = 255; }
                                                fullmap.SetPixel(img_x + (int)(reader.adtfile.chunks[a].header.indexX * 9), img_y + (int)(reader.adtfile.chunks[a].header.indexY * 9), Color.FromArgb(greyness, greyness, greyness));
                                            }
                                        
                                            img_x++;
                                        }
                                        counter++;
                                    }
                                    if (i % 2 == 0)
                                    {
                                        img_y++;
                                        img_x = 0;
                                        //Console.Write("\n");
                                    }
                                }
                            }
                            fullmap.Save("Z:/adttest/" + mapname + "_" + y + "_" + x + ".png");
                        }
                    }
                }
                Console.Write(" ..done!\n");
            
                //Time to compile the full image
                var res_x = (((max_y - min_y) * 144) + 144);
                var res_y = (((max_x - min_x) * 144) + 144);

                if (res_x > 0 && res_y > 0)
                {
                    Console.WriteLine("[" + mapname + "] " + "Creating new image of " + res_x + "x" + res_y);

                    Bitmap bmp = new Bitmap(res_x, res_y);
                    Graphics g = Graphics.FromImage(bmp);

                    for (int cur_x = 0; cur_x < 64; cur_x++)
                    {
                        for (int cur_y = 0; cur_y < 64; cur_y++)
                        {
                            if (File.Exists("Z:/adttest/" + mapname + "_" + cur_x + "_" + cur_y + ".png"))
                            {
                                var bmpreader = new Bitmap("Z:/adttest/" + mapname + "_" + cur_x + "_" + cur_y + ".png");
                                g.DrawImage(bmpreader, (cur_x - min_x) * 144, (cur_y - min_y) * 144, new Rectangle(0, 0, 144, 144), GraphicsUnit.Pixel);
                                bmpreader.Dispose();
                                File.Delete("Z:/adttest/" + mapname + "_" + cur_x + "_" + cur_y + ".png");
                            }
                        }
                    }
                    g.Dispose();
                    if (!Directory.Exists("done")) { Directory.CreateDirectory("done"); }
                    bmp.Save("done/" + mapname + ".png");
                }
            }
        }
    }
}
