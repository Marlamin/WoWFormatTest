using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using WoWFormatLib.Utils;

namespace WoWFormatLib.FileReaders
{
    public class WDLReader
    {
        private string basedir;

        public WDLReader(string basedir)
        {
            this.basedir = basedir;
        }

        public void LoadWDL(string filename)
        {
            if (File.Exists(Path.Combine(basedir,filename)))
            {
                using (FileStream wdl = File.Open(Path.Combine(basedir,filename), FileMode.Open))
                {
                    ReadWDL(filename, wdl);
                }
            }
            else
            {
                throw new Exception("WDL not found!");
            }

        }

        private void ReadWDL(string filename, FileStream wdl)
        {
            var bin = new BinaryReader(wdl);
            BlizzHeader chunk;
            long position = 0;
            while (position < wdl.Length)
            {
                wdl.Position = position;

                chunk = new BlizzHeader(bin.ReadChars(4), bin.ReadUInt32());
                chunk.Flip();

                position = wdl.Position + chunk.Size;

                switch (chunk.ToString())
                {
                    case "MVER": ReadMVERChunk(bin);
                        continue;
                    case "MWMO": ReadMWMOChunk(bin, chunk);
                        continue;
                    case "MWID":
                    case "MAOC": //New in WoD?!
                    case "MAOF": //contains MARE and MAHO subchunks
                    case "MARE":
                    case "MAHO":
                    case "MODF": continue;
                    default:
                        throw new Exception(String.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunk.ToString(), position.ToString(), filename));
                }
            }
        }

        private void ReadMWMOChunk(BinaryReader bin, BlizzHeader chunk)
        {
            var wmoFilesChunk = bin.ReadBytes((int)chunk.Size);

            var str = new StringBuilder();

            for (var i = 0; i < wmoFilesChunk.Length; i++)
            {
                if (wmoFilesChunk[i] == '\0')
                {
                    if (str.Length > 1)
                    {
                        if (!System.IO.File.Exists(System.IO.Path.Combine(basedir, str.ToString())))
                        {
                            Console.WriteLine("WMO file does not exist!!! {0}", str.ToString());
                            new WoWFormatLib.Utils.MissingFile(str.ToString());
                        }
                    }
                    str = new StringBuilder();
                }
                else
                {
                    str.Append((char)wmoFilesChunk[i]);
                }
            }
        }

        private void ReadMVERChunk(BinaryReader bin)
        {
            if (bin.ReadUInt32() != 18)
            {
                throw new Exception("Unsupported WDL version!");
            }
        }

    }
}
