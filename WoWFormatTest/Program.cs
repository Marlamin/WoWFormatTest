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
        private static void Main(string[] args)
        {
            CASC.InitCasc(null, null, "wowt");

            if (!File.Exists("listfile.txt"))
            {
                throw new Exception("Listfile not found!");
            }

            foreach (var line in File.ReadAllLines("listfile.txt"))
            {
                var cleaned = line.Replace("/", "\\");
                if (CASC.cascHandler.FileExists(cleaned) && cleaned.EndsWith(".adt", StringComparison.CurrentCultureIgnoreCase) && !cleaned.Contains("_obj") && !cleaned.Contains("_lod") && !cleaned.Contains("_tex"))
                {
                    Console.WriteLine("Loading " + cleaned);
                    var reader = new ADTReader();
                    reader.LoadADT(cleaned, true);
                }
            }

        }
    }
}