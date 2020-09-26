using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CASCLib;
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
            string basedir = ConfigurationManager.AppSettings["basedir"];

            Console.WriteLine("Initializing CASC..");

            if(basedir != string.Empty){
                CASC.InitCasc(new BackgroundWorkerEx(), basedir, "wowt");
            }else{
                CASC.InitCasc(new BackgroundWorkerEx(), null, "wowt");
            }

            Console.WriteLine("CASC initialized!");
            Console.WriteLine("Current patch: " + CASC.BuildName);

            Console.Write("Loading listfile..");
            Listfile.Load();
            Console.WriteLine("..done!");
            // shalaran 1247268 
            //var wmoFileDataID = uint.Parse(args[0]);
            //var wmocompiler = new WMO();
            //wmocompiler.Compile(wmoFileDataID);

            var linelist = new List<(uint, string)>();

            foreach (var entry in Listfile.fdidToNameMap)
            {
                if (entry.Value.StartsWith("world/wmo") && entry.Value.EndsWith(".wmo"))
                {
                    linelist.Add((entry.Key, entry.Value));
                }
            }

            string[] unwantedExtensions = new string[513];
            for (int i = 0; i < 512; i++)
            {
                unwantedExtensions[i] = "_" + i.ToString().PadLeft(3, '0') + ".wmo";
            }

            foreach ((uint fdid, string s) in linelist)
            {
                if (s.Length > 8 && !unwantedExtensions.Contains(s.Substring(s.Length - 8, 8)))
                {
                    if ((s.Contains("lod0") || s.Contains("lod1") || s.Contains("lod2") || s.Contains("lod3"))) continue;
                    
                    Console.WriteLine(s);
                    try
                    {
                        var wmocompiler = new WMO();
                        wmocompiler.Compile(fdid);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Encountered exception while compiling minimap for WMO " + fdid + " (" + s +")");
                        Console.WriteLine(e.Message);
                    }

                }
            }
        }
    }
}
