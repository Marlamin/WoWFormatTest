using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;

namespace WoWFormatTest
{
    class WMOReader
    {
        private List<String> blpFiles;
        private List<String> m2Files;
        public void LoadWMO(string filename)
        {
            var basedir = ConfigurationManager.AppSettings["basedir"];

            m2Files = new List<string>();
            blpFiles = new List<string>();

            var wmo = File.Open(basedir + filename, FileMode.Open);
            var bin = new BinaryReader(wmo);
            BlizzHeader chunk;

            long position = 0;
            while (position < wmo.Length)
            {
                wmo.Position = position;
                chunk = new BlizzHeader(bin.ReadChars(4), bin.ReadUInt32());
                chunk.Flip();
                position = wmo.Position + chunk.Size;

                switch (chunk.ToString())
                {
                    case "MVER": 
                        if (bin.ReadUInt32() != 17)
                        {
                            throw new Exception("Unsupported WMO version!");
                        } 
                        continue;
                    case "MOHD":
                        continue;
                    case "MOTX":
                        ReadMOTXChunk(chunk, bin);
                        continue;
                    case "MOMT":
                    case "MOGN":
                    case "MOGI":
                    case "MOSB":
                    case "MOPV":
                    case "MOPT":
                    case "MOPR":
                    case "MOVV":
                    case "MOVB":
                    case "MOLT":
                    case "MODS":
                        continue;
                    case "MODN":
                        ReadMODNChunk(chunk, bin);
                        continue;
                    case "MODD":
                    case "MFOG":
                    case "MCVP":
                        continue;

                    default:
                        throw new Exception(String.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunk.ToString(), position.ToString(), filename));
                }
            }
            wmo.Close();
        }

        public void ReadMOTXChunk(BlizzHeader chunk, BinaryReader bin)
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
                        blpFiles.Add(str.ToString());
                        //Console.WriteLine("         " + str.ToString());
                    }
                    str = new StringBuilder();
                }
                else
                {
                    str.Append((char)blpFilesChunk[i]);
                }
            }
        }

        public void ReadMODNChunk(BlizzHeader chunk, BinaryReader bin)
        {
            //List of M2 filenames, but are still named after MDXs internally. Have to rename!
            var m2FilesChunk = bin.ReadBytes((int)chunk.Size);

            var str = new StringBuilder();

            for (var i = 0; i < m2FilesChunk.Length; i++)
            {
                if (m2FilesChunk[i] == '\0')
                {
                    if (str.Length > 1)
                    {
                        m2Files.Add(str.ToString());
                        //Console.WriteLine("         " + str.ToString());
                        var m2reader = new M2Reader();
                        m2reader.LoadM2(str.ToString());
                        
                    }
                    str = new StringBuilder();
                }
                else
                {
                    str.Append((char)m2FilesChunk[i]);
                }
            }
        }
    }
}
