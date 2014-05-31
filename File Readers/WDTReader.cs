using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WoWFormatTest
{
    class WDTReader
    {
        public void LoadWDT(string basedir, string mapname)
        {
            Console.WriteLine("Loading WDT for map " + mapname);
            string filename = basedir + "World\\Maps\\" + mapname + "\\" + mapname + ".wdt";
            FileStream wdt = File.Open(filename, FileMode.Open);
            BinaryReader bin = new BinaryReader(wdt);
            BlizzHeader chunk;
            long position = 0;
            while (position < wdt.Length)
            {
                wdt.Position = position;
                
                chunk = new BlizzHeader(bin.ReadChars(4), bin.ReadUInt32());
                chunk.Flip();

                position = wdt.Position + chunk.Size;

                if (chunk.Is("MVER"))
                {
                    if (bin.ReadUInt32() != 18)
                    {
                        throw new Exception("Unsupported WDT version!");
                    }
                    continue;
                }

                if (chunk.Is("MPHD"))
                {
                    continue;
                }

                if (chunk.Is("MAIN"))
                {
                    if (chunk.Size != 4096 * 8)
                    {
                        throw new Exception("MAIN size is wrong! (" + chunk.Size.ToString() + ")");
                    }

                    for (int x = 0; x < 64; x++)
                    {
                        for (int y = 0; y < 64; y++)
                        {
                            UInt32 flags = bin.ReadUInt32();
                            UInt32 unused = bin.ReadUInt32();
                            if (flags == 1)
                            {
                                //ADT exists
                                ADTReader adtreader = new ADTReader();
                                adtreader.LoadADT(basedir, mapname, x, y);
                            }
                        }
                    }

                    continue;
                }

                if (chunk.Is("MWMO"))
                {
                    if (bin.ReadByte() != 0)
                    {
                        bin.BaseStream.Position = bin.BaseStream.Position - 1;

                        StringBuilder str = new StringBuilder();
                        char c;
                        while ((c = bin.ReadChar()) != '\0')
                        {
                            str.Append(c);
                        }
                        String wmofilename = str.ToString();

                        WMOReader wmoreader = new WMOReader();
                        wmoreader.LoadWMO(basedir, wmofilename);
                    }

                    continue;
                }

                if (chunk.Is("MODF"))
                {
                    continue;
                }

                throw new Exception(String.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunk.ToString(), position.ToString(), filename));
            }
        }
    }
}
