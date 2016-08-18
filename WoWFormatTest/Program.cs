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
        private static void LoadAllMaps(string basedir)
        {
            //var reader = new MapReader(basedir);
           // Dictionary<int, string> maps = reader.GetMaps();
           // foreach (KeyValuePair<int, string> map in maps)
          //  {
            //    LoadMap(map.Value, basedir);
            //}
        }

        private static void LoadFromListfile(string listfile, string basedir)
        {
            string line;
            StreamReader file = new System.IO.StreamReader(listfile);
            while ((line = file.ReadLine()) != null)
            {
                if (line.EndsWith(".m2", StringComparison.OrdinalIgnoreCase))
                {
                    //M2Reader reader = new M2Reader(basedir);
                    //reader.LoadM2(line);
                }
                else if (line.EndsWith(".wmo", StringComparison.OrdinalIgnoreCase))
                {
                    //WMOReader reader = new WMOReader(basedir);
                    //reader.LoadWMO(line);
                }
                else if (line.EndsWith(".wdt", StringComparison.OrdinalIgnoreCase))
                {
                    //WDTReader reader = new WDTReader(basedir);
                    //reader.LoadWDT(line);
                }
                else if (line.EndsWith(".tex", StringComparison.OrdinalIgnoreCase))
                {
                    //TEXReader reader = new TEXReader(basedir);
                    //reader.LoadTEX(line);
                }
                else if (line.EndsWith(".wdl", StringComparison.OrdinalIgnoreCase))
                {
                    //WDLReader reader = new WDLReader(basedir);
                    //reader.LoadWDL(line);
                }
                else if (
                    line.EndsWith(".blp", StringComparison.OrdinalIgnoreCase) || //Useless to read out for now
                    line.EndsWith(".adt", StringComparison.OrdinalIgnoreCase) || //Terrain files, already get read in WDTreader
                    line.EndsWith(".skin", StringComparison.OrdinalIgnoreCase) ||//Referenced in M2s, needs parser
                    line.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase) || //Sounds, no reason to read out for now
                    line.EndsWith(".anim", StringComparison.OrdinalIgnoreCase) ||//Referenced in M2s, needs parser
                    line.EndsWith(".lua", StringComparison.OrdinalIgnoreCase) || //Interface file, useless
                    line.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) || //Interface file, useless
                    line.EndsWith(".html", StringComparison.OrdinalIgnoreCase) ||//Used in credits, useless
                    line.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) || //component.wow-xxxx.txt where xxxx is locale, contains build no.
                    line.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase) || //Music, useless
                    line.EndsWith(".bls", StringComparison.OrdinalIgnoreCase) || //Blizzard Shaders
                    line.EndsWith(".sig", StringComparison.OrdinalIgnoreCase) || //Signature files
                    line.EndsWith(".avi", StringComparison.OrdinalIgnoreCase) || //Cinematics
                    line.EndsWith(".sbt", StringComparison.OrdinalIgnoreCase) || //Subtitles
                    line.EndsWith(".zmp", StringComparison.OrdinalIgnoreCase) || //?
                    line.EndsWith(".toc", StringComparison.OrdinalIgnoreCase) || //Table of Contents, addon manifest
                    line.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) || //Fonts
                    line.EndsWith(".dbc", StringComparison.OrdinalIgnoreCase) || //Client database, but useless to read out all of them
                    line.EndsWith(".lst", StringComparison.OrdinalIgnoreCase) || //.lst used in streaming
                    line.EndsWith(".phys", StringComparison.OrdinalIgnoreCase) ||//Physics (used in belt items for example)
                    line.EndsWith(".wtf", StringComparison.OrdinalIgnoreCase) || //Contain coordinates to worldport locations
                    line.EndsWith(".db2", StringComparison.OrdinalIgnoreCase) || //Client database (v2), needs parser
                    line.EndsWith(".wfx", StringComparison.OrdinalIgnoreCase) || //Shader effects
                    line.EndsWith(".what", StringComparison.OrdinalIgnoreCase) ||//what (leftovers from new ADT version experimenting)
                    line.EndsWith(".xsd", StringComparison.OrdinalIgnoreCase)    //Inteface XML schema
                )
                {
                    //Not yet!
                    if (!File.Exists(basedir + line))
                    {
                        //new WoWFormatLib.Utils.MissingFile(line);
                    }
                }
                else
                {
                    Console.WriteLine("Unknown file: " + line);
                }
            }
            file.Close();
            Console.WriteLine("Done.");
        }

        private static void LoadMap(string map, string basedir)
        {
            if (File.Exists(Path.Combine(basedir, "World\\Maps\\", map, map + ".wdt")))
            {
                Console.WriteLine("Loading " + map + "...");
               // var wdtreader = new WDTReader(basedir);
               // wdtreader.LoadWDT(map);
            }
            else
            {
                Console.WriteLine("Map \"" + map + "\" does not exist.");
            }
        }

        private static void Main(string[] args)
        {
            CASC.InitCasc(null, @"C:\World of Warcraft", "wow");
            Console.WriteLine("CASC loaded!");
            var reader = new M2Reader();
            var filename = @"item\objectcomponents\shield\shield_1h_artifactnorgannon_d_01.m2";
            reader.LoadM2(filename);
            var fileDataID = CASC.getFileDataIdByName(filename);

            for (int i = 0; i < reader.model.textures.Length; i++)
            {
                Console.WriteLine("Doing type " + reader.model.textures[i].type + " lookup for texture #" + i);
                switch (reader.model.textures[i].type)
                {
                    case 1:
                    case 2:
                    case 11:
                        uint[] cdifilenames = DBCHelper.getTexturesByModelFilename(fileDataID, (int)reader.model.textures[i].type);
                        for (int ti = 0; ti < cdifilenames.Length; ti++)
                        {
                            Console.WriteLine("Found (texture #" + ti + ") " + cdifilenames[ti]);
                            var blpreader = new BLPReader();
                            blpreader.LoadBLP((int)cdifilenames[ti]);
                        }
                        break;
                    default:
                        throw new Exception("Unhandled texture type");
                        break;
                }
            }
            Console.WriteLine("Done.");
            Console.ReadLine();
        }
    }
}