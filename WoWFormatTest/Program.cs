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
            //CASC.InitCasc(null, @"C:\World of Warcraft", "wow");
            CASC.InitCasc(null, null, "wowt");
            Console.WriteLine("CASC loaded!");

            if (!File.Exists("listfile.txt"))
            {
                throw new Exception("Listfile not found!");
            }

            var reader = new BLSReader();
            reader.LoadBLS(1694483);

            var shaderFile = reader.shaderFile;
            //foreach (var line in File.ReadAllLines("listfile.txt"))
            //{
            //    if (CASC.cascHandler.FileExists(line) && line.EndsWith(".bls", StringComparison.CurrentCultureIgnoreCase))
            //    {
            //        Console.WriteLine("Loading " + line);
            //        var reader = new BLSReader();
            //        reader.LoadBLS(line);
            //    }
            //}

        }
    }
}