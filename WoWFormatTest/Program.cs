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
            //Console.WriteLine("CASC loaded!");

            var reader = new BLSReader();
            reader.LoadBLS("shaders/pixel/ps_5_0/terrain.bls");

            var shader = reader.shaderFile;

            Console.WriteLine("Shader loaded!");
        }
    }
}