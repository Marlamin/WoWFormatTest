using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WoWFormatLib.Utils;
using WoWFormatLib.Structs.WDT;
using System.Linq;

namespace WoWFormatLib.FileReaders
{
    public class WDTReader
    {
        public List<int[]> tiles;
        public WDT wdtfile;

        public List<int[]> getTiles()
        {
            return tiles;
        }

        public void LoadWDT(string filename)
        {
            tiles = new List<int[]>();
            if (CASC.cascHandler.FileExists(filename))
            {
                using (Stream tex = CASC.cascHandler.OpenFile(filename))
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

        private void ReadMAINChunk(BinaryReader bin, uint size, String filename)
        {
            if (size != 4096 * 8)
            {
                throw new Exception("MAIN size is wrong! (" + size.ToString() + ")");
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
            long position = 0;
            while (position < wdt.Length)
            {
                wdt.Position = position;

                var chunkName = new string(bin.ReadChars(4).Reverse().ToArray());
                var chunkSize = bin.ReadUInt32();

                position = wdt.Position + chunkSize;

                switch (chunkName)
                {
                    case "MVER":
                        ReadMVERChunk(bin);
                        break;
                    case "MAIN":
                        ReadMAINChunk(bin, chunkSize, filename);
                        break;
                    case "MWMO":
                        ReadMWMOChunk(bin);
                        break;
                    case "MPHD":
                        wdtfile.mphd = ReadMPHDChunk(bin);
                        break;
                    case "MPLT":
                    case "MODF":
                        continue;
                    default:
                        throw new Exception(String.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunkName, position.ToString(), filename));
                }
            }
        }

    }
}