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
        public List<int[]> tiles;

        public WDTReader(string basedir)
        {
            this.basedir = basedir;
        }


        public void LoadWDT(string filename)
        {
            tiles = new List<int[]>();
            if (File.Exists(Path.Combine(basedir, filename)))
            {
                using (FileStream wdt = File.Open(Path.Combine(basedir, filename), FileMode.Open))
                {
                    ReadWDT(filename, wdt);
                }
            }
            else
            {
                throw new Exception("WDT not found!");
            }

        }

        private void ReadWDT(string filename, FileStream wdt)
        {
            filename = Path.ChangeExtension(filename, "WDT");
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
                    case "MAIN": ReadMAINChunk(bin, chunk, filename);
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

        private void ReadMAINChunk(BinaryReader bin, BlizzHeader chunk, String filename)
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
                        var adtfilename = filename.Replace(".WDT", "_" + y + "_" + x + ".adt"); //blizz flips these
                        adtreader.LoadADT(adtfilename);
                        int[] xy = new int[] {y, x};
                        tiles.Add(xy);
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

        //TODO there's probably a better way to do this
        public List<int[]> getTiles()
        {
            return tiles;
        }
    }
}
