using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WoWFormatLib.Utils;
using WoWFormatLib.Structs.WDT;

namespace WoWFormatLib.FileReaders
{
    public class WDTReader
    {
        public List<int[]> tiles;
        public WDT wdtfile;

        public WDTReader()
        {
        }

        public List<int[]> getTiles()
        {
            return tiles;
        }

        public void LoadWDT(string filename)
        {
            tiles = new List<int[]>();
            if (CASC.FileExists(filename))
            {
                using (Stream tex = CASC.OpenFile(filename))
                {
                    ReadWDT(filename, tex);
                }
            }
            else
            {
                new WoWFormatLib.Utils.MissingFile(filename);
                return;
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
                    var nobodycares = bin.ReadUInt32();
                    if (flags == 1)
                    {
                        var adtfilename = filename.Replace(".WDT", "_" + y + "_" + x + ".adt");
                        int[] xy = new int[] { y, x };
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
                //var wmoreader = new WMOReader();
                //wmoreader.LoadWMO(wmofilename);
            }
        }

        private MPHD ReadMPHDChunk(BinaryReader bin)
        {
            var mphd = new MPHD();
            mphd.flags = (mphdFlags) bin.ReadUInt32();
            mphd.something = bin.ReadUInt32();
            mphd.unused = new uint[] { bin.ReadUInt32(), bin.ReadUInt32(), bin.ReadUInt32(), bin.ReadUInt32(), bin.ReadUInt32(), bin.ReadUInt32() };
            return mphd;
        }

        private void ReadWDT(string filename, Stream wdt)
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
                    case "MPHD": wdtfile.mphd = ReadMPHDChunk(bin);
                        continue;
                    case "MPLT":
                    case "MODF": continue;
                    default:
                        throw new Exception(String.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunk.ToString(), position.ToString(), filename));
                }
            }
        }

    }
}