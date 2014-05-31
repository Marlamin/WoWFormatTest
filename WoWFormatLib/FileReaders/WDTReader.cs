using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using WoWFormatLib.Utils;

namespace WoWFormatLib.FileReaders
{
    public class WDTReader
    {
        private string basedir;        

        public WDTReader(string basedir)
        {
            this.basedir = basedir;
        }


        public void LoadWDT(string map)
        {
            //Console.WriteLine("Loading WDT for map " + map);

            var filename = Path.Combine(basedir, "World\\Maps\\", map, map + ".wdt");
            using (FileStream wdt = File.Open(filename, FileMode.Open))
            {
                ReadWDT(map, filename, wdt);
            }
        }

        private void ReadWDT(string map, string filename, FileStream wdt)
        {
            var bin = new BinaryReader(wdt);
            BlizzHeader chunk;
            long position = 0;
            while (position < wdt.Length)
            {
                wdt.Position = position;

                chunk = new BlizzHeader(bin.ReadChars(4), bin.ReadUInt32());
                chunk.Flip();

                position = wdt.Position + chunk.Size;

                switch (chunk.ToString())
                {
                    case "MVER": ReadMVERChunk(bin);
                        continue;
                    case "MAIN": ReadMAINChunk(map, bin, chunk);
                        continue;
                    case "MWMO": ReadMWMOChunk(bin);
                        continue;
                    case "MPHD":
                    case "MODF": continue;
                    default:
                        throw new Exception(String.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunk.ToString(), position.ToString(), filename));
                }
            }
        }

        private void ReadMWMOChunk(BinaryReader bin)
        {
            if (bin.ReadByte() != 0)
            {
                bin.BaseStream.Position = bin.BaseStream.Position - 1;

                var str = new StringBuilder();
                char c;
                while ((c = bin.ReadChar()) != '\0')
                {
                    str.Append(c);
                }
                var wmofilename = str.ToString();

                var wmoreader = new WMOReader(basedir);
                wmoreader.LoadWMO(wmofilename);
            }
        }

        private void ReadMAINChunk(string map, BinaryReader bin, BlizzHeader chunk)
        {
            if (chunk.Size != 4096 * 8)
            {
                throw new Exception("MAIN size is wrong! (" + chunk.Size.ToString() + ")");
            }

            for (var x = 0; x < 64; x++)
            {
                for (var y = 0; y < 64; y++)
                {
                    var flags = bin.ReadUInt32();
                    var unused = bin.ReadUInt32();
                    if (flags == 1)
                    {
                        //ADT exists
                        var adtreader = new ADTReader(basedir);
                        adtreader.LoadADT(map, x, y);
                    }
                }
            }
        }

        private void ReadMVERChunk(BinaryReader bin)
        {
            if (bin.ReadUInt32() != 18)
            {
                throw new Exception("Unsupported WDT version!");
            }
        }
    }
}
