using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using WoWFormatLib.Utils;

namespace WoWFormatLib.FileReaders
{
    public class TEXReader
    {
        private string basedir;

        public TEXReader(string basedir)
        {
            this.basedir = basedir;
        }

        public void LoadTEX(string filename)
        {
            if (File.Exists(Path.Combine(basedir, filename)))
            {
                using (FileStream tex = File.Open(Path.Combine(basedir, filename), FileMode.Open))
                {
                    ReadTEX(filename, tex);
                }
            }
            else
            {
                if (!File.Exists(basedir + filename))
                {
                    new WoWFormatLib.Utils.MissingFile(filename);
                    return;
                }
            }

        }

        private void ReadTEX(string filename, FileStream tex)
        {
            var bin = new BinaryReader(tex);
            BlizzHeader chunk;
            long position = 0;
            while (position < tex.Length)
            {
                tex.Position = position;

                chunk = new BlizzHeader(bin.ReadChars(4), bin.ReadUInt32());
                chunk.Flip();

                position = tex.Position + chunk.Size;

                switch (chunk.ToString())
                {
                    case "TXVR": ReadTXVRChunk(bin);
                        continue;
                    case "TXFN": ReadTXFNChunk(bin, chunk);
                        continue;
                    case "TXBT":
                    case "TXMD": continue;
                    default:
                        throw new Exception(String.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunk.ToString(), position.ToString(), filename));
                }
            }
        }

        private void ReadTXFNChunk(BinaryReader bin, BlizzHeader chunk)
        {
            //List of BLP filenames
            var blpFilesChunk = bin.ReadBytes((int)chunk.Size);

            var str = new StringBuilder();

            for (var i = 0; i < blpFilesChunk.Length; i++)
            {
                if (blpFilesChunk[i] == '\0')
                {
                    if (str.Length > 1)
                    {
                        str.Replace("..", ".");
                        str.Append(".blp"); //Filenames in TEX dont have have BLP extensions
                        if (!System.IO.File.Exists(System.IO.Path.Combine(basedir, str.ToString())))
                        {
                            new WoWFormatLib.Utils.MissingFile(str.ToString());
                        }
                    }
                    str = new StringBuilder();
                }
                else
                {
                    str.Append((char)blpFilesChunk[i]);
                }
            }
        }

        private void ReadTXVRChunk(BinaryReader bin)
        {
            if (bin.ReadUInt32() != 0)
            {
                throw new Exception("Unsupported TEX version!");
            }
        }

    }
}
