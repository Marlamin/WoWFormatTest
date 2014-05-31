using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace WoWFormatTest
{
    class WDTReader
    {
        public void LoadWDT(string map)
        {
            //Console.WriteLine("Loading WDT for map " + map);

            var basedir = ConfigurationManager.AppSettings["basedir"];
            var filename = Path.Combine(basedir, "World\\Maps\\", map, map + ".wdt");
            var wdt = File.Open(filename, FileMode.Open);
            var bin = new BinaryReader(wdt);
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

                    for (var x = 0; x < 64; x++)
                    {
                        for (var y = 0; y < 64; y++)
                        {
                            var flags = bin.ReadUInt32();
                            var unused = bin.ReadUInt32();
                            if (flags == 1)
                            {
                                //ADT exists
                                var adtreader = new ADTReader();
                                adtreader.LoadADT(map, x, y);
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

                        var str = new StringBuilder();
                        char c;
                        while ((c = bin.ReadChar()) != '\0')
                        {
                            str.Append(c);
                        }
                        var wmofilename = str.ToString();

                        var wmoreader = new WMOReader();
                        wmoreader.LoadWMO(wmofilename);
                    }

                    continue;
                }

                if (chunk.Is("MODF"))
                {
                    continue;
                }

                throw new Exception(String.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunk.ToString(), position.ToString(), filename));
            }
            wdt.Close();
        }
    }
}
