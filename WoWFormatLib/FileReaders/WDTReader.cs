using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WoWFormatLib.Structs.WDT;
using WoWFormatLib.Utils;

namespace WoWFormatLib.FileReaders
{
    public class WDTReader
    {
        public List<(byte, byte)> tiles = new List<(byte, byte)>();
        public Dictionary<(byte, byte), MapFileDataIDs> tileFiles = new Dictionary<(byte, byte), MapFileDataIDs>();
        public Dictionary<string, MapFileDataIDs> stringTileFiles = new Dictionary<string, MapFileDataIDs>();
        public WDT wdtfile;

        public void LoadWDT(string filename)
        {
            if (CASC.FileExists(filename))
            {
                LoadWDT(CASC.getFileDataIdByName(filename));
            }
            else
            {
                throw new FileNotFoundException("WDT " + filename + " does not exist");
            }
        }

        public void LoadWDT(uint filedataid)
        {
            using (var stream = CASC.OpenFile(filedataid))
            {
                ReadWDT(stream);
            }

            foreach(var entry in tileFiles)
            {
                stringTileFiles.Add(entry.Key.Item1 + "," + entry.Key.Item2, entry.Value);
            }
        }

        private void ReadMAINChunk(BinaryReader bin)
        {
            for (byte x = 0; x < 64; x++)
            {
                for (byte y = 0; y < 64; y++)
                {
                    var flags = bin.ReadUInt32();
                    bin.ReadUInt32();
                    if (flags == 1)
                    {
                        tiles.Add((y, x));
                    }
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
                //var wmoreader = new WMOReader();
                //wmoreader.LoadWMO(wmofilename);
            }
        }

        private MPHD ReadMPHDChunk(BinaryReader bin)
        {
            return bin.Read<MPHD>();
        }

        private void ReadMAIDChunk(BinaryReader bin)
        {
            for (byte x = 0; x < 64; x++)
            {
                for (byte y = 0; y < 64; y++)
                {
                    tileFiles.Add((y, x), bin.Read<MapFileDataIDs>());
                }
            }
        }
        private MODF ReadMODFChunk(BinaryReader bin)
        {
            return bin.Read<MODF>();
        }

        private void ReadWDT(Stream wdt)
        {
            var bin = new BinaryReader(wdt);
            long position = 0;
            while (position < wdt.Length)
            {
                wdt.Position = position;

                var chunkName = (WDTChunks)bin.ReadUInt32();
                var chunkSize = bin.ReadUInt32();

                position = wdt.Position + chunkSize;

                switch (chunkName)
                {
                    case WDTChunks.MVER:
                        if (bin.ReadUInt32() != 18)
                            throw new Exception("Unsupported WDT version!");
                        break;
                    case WDTChunks.MAIN:
                        ReadMAINChunk(bin);
                        break;
                    case WDTChunks.MWMO:
                        ReadMWMOChunk(bin);
                        break;
                    case WDTChunks.MPHD:
                        wdtfile.mphd = ReadMPHDChunk(bin);
                        break;
                    case WDTChunks.MAID:
                        ReadMAIDChunk(bin);
                        break;
                    case WDTChunks.MODF:
                        wdtfile.modf = ReadMODFChunk(bin);
                        break;
                    default:
                        throw new Exception(string.Format("Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunkName, position.ToString()));
                }
            }
        }
    }
}
