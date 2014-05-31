using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace WoWFormatTest
{
    class ADTReader
    {
        private List<String> m2Files;
        private List<String> wmoFiles;
        private List<String> blpFiles;

        public void LoadADT(string mapname, int x, int y)
        {
            string basedir = ConfigurationManager.AppSettings["basedir"];

            m2Files = new List<string>();
            wmoFiles = new List<string>();
            blpFiles = new List<string>();

            Console.WriteLine("Loading {0}_{1} ADT for map {2}", y, x, mapname);

            string filename = Path.Combine(basedir, "World\\Maps\\" + mapname + "\\" + mapname + "_" + y + "_" + x); // x and y are flipped because blizzard

            FileStream adt = File.Open(filename + ".adt", FileMode.Open);

            BinaryReader bin = new BinaryReader(adt);
            BlizzHeader chunk = null;
            long position = 0;
            while (position < adt.Length)
            {
                adt.Position = position;
                chunk = new BlizzHeader(bin.ReadChars(4), bin.ReadUInt32());
                chunk.Flip();
                position = adt.Position + chunk.Size;

                switch (chunk.ToString())
                {
                    case "MVER": 
                        if (bin.ReadUInt32() != 18)
                        {
                            throw new Exception("Unsupported ADT version!");
                        } 
                        continue;
                    case "MHDR":
                    case "MH2O":
                    case "MCNK":
                    case "MFBO":
                    //model.blob stuff
                    case "MBMH":
                    case "MBBB":
                    case "MBMI":
                    case "MBNV": continue;
                    default:
                        throw new Exception(String.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunk.ToString(), position.ToString(), filename));
                }
            }

            adt.Close();

            using (FileStream adtobj0 = File.Open(filename + "_obj0.adt", FileMode.Open))
            {
                ReadObjHeader(mapname, x, y, filename, adtobj0, "OBJ0", ref chunk);
            }

            using (FileStream adtobj1 = File.Open(filename + "_obj1.adt", FileMode.Open))
            {
                ReadObjHeader(mapname, x, y, filename, adtobj1, "OBJ1", ref chunk);
            }

            using (FileStream adttex0 = File.Open(filename + "_tex0.adt", FileMode.Open))
            {
                ReadTexHeader(mapname, x, y, filename, adttex0, "TEX0", ref chunk);
            }

            using (FileStream adttex1 = File.Open(filename + "_tex1.adt", FileMode.Open))
            {
                ReadTexHeader(mapname, x, y, filename, adttex1, "TEX1", ref chunk);
            }
        }

        private void ReadObjHeader(string mapname, int x, int y, string filename, FileStream adtObjStream, string objName, ref BlizzHeader chunk)
        {
            BinaryReader bin = new BinaryReader(adtObjStream);
            long position = 0;

            Console.WriteLine("Loading {0}_{1} {2} ADT for map {3}", y, x, objName, mapname);

            while (position < adtObjStream.Length)
            {
                adtObjStream.Position = position;
                chunk = new BlizzHeader(bin.ReadChars(4), bin.ReadUInt32());
                chunk.Flip();
                position = adtObjStream.Position + chunk.Size;

                if (chunk.Is("MVER")) { if (bin.ReadUInt32() != 18) { throw new Exception("Unsupported ADT version!"); } continue; }
                if (chunk.Is("MMDX")) { ReadMMDXChunk(chunk, bin); continue; }
                if (chunk.Is("MMID")) { continue; }
                if (chunk.Is("MWMO")) { ReadMWMOChunk(chunk, bin); continue; }
                if (chunk.Is("MWID")) { continue; }
                if (chunk.Is("MDDF")) { continue; }
                if (chunk.Is("MODF")) { continue; }
                if (chunk.Is("MCNK")) { continue; }

                throw new Exception(String.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunk.ToString(), position.ToString(), filename));
            }
        }

        private void ReadTexHeader(string mapname, int x, int y, string filename, FileStream adtTexStream, string texName, ref BlizzHeader chunk)
        {
            BinaryReader bin = new BinaryReader(adtTexStream);
            long position = 0;
            Console.WriteLine("Loading {0}_{1} {2} ADT for map {3}", y, x, texName, mapname);

            while (position < adtTexStream.Length)
            {
                adtTexStream.Position = position;
                chunk = new BlizzHeader(bin.ReadChars(4), bin.ReadUInt32());
                chunk.Flip();
                position = adtTexStream.Position + chunk.Size;

                if (chunk.Is("MVER")) { if (bin.ReadUInt32() != 18) { throw new Exception("Unsupported ADT version!"); } continue; }
                if (chunk.Is("MAMP")) { continue; }
                if (chunk.Is("MTEX")) { ReadMTEXChunk(chunk, bin); continue; }
                if (chunk.Is("MCNK")) { continue; }
                if (chunk.Is("MTXP")) { continue; }

                throw new Exception(String.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunk.ToString(), position.ToString(), filename));
            }
        }

        public void ReadMWMOChunk(BlizzHeader chunk, BinaryReader bin)
        {
            //List of WMO filenames
            byte[] wmoFilesChunk = bin.ReadBytes((int)chunk.Size);

            StringBuilder str = new StringBuilder();

            for (int i = 0; i < wmoFilesChunk.Length; i++)
            {
                if (wmoFilesChunk[i] == '\0')
                {
                    if (str.Length > 1)
                    {
                        wmoFiles.Add(str.ToString());
                        Console.WriteLine("     " + str.ToString());
                        WMOReader wmoreader = new WMOReader();
                        wmoreader.LoadWMO(str.ToString());
                    }
                    str = new StringBuilder();
                }
                else
                {
                    str.Append((char)wmoFilesChunk[i]);
                }
            }
        }

        public void ReadMMDXChunk(BlizzHeader chunk, BinaryReader bin)
        {
            //List of M2 filenames, but are still named after MDXs internally. Have to rename!
            byte[] m2FilesChunk = bin.ReadBytes((int)chunk.Size);

            StringBuilder str = new StringBuilder();

            for (int i = 0; i < m2FilesChunk.Length; i++)
            {
                if (m2FilesChunk[i] == '\0')
                {
                    if (str.Length > 1)
                    {
                        m2Files.Add(str.ToString());
                        Console.WriteLine("     " + str.ToString());
                    }
                    str = new StringBuilder();
                }
                else
                {
                    str.Append((char)m2FilesChunk[i]);
                }
            }
        }

        public void ReadMTEXChunk(BlizzHeader chunk, BinaryReader bin)
        {
            //List of BLP filenames
            byte[] blpFilesChunk = bin.ReadBytes((int)chunk.Size);

            StringBuilder str = new StringBuilder();

            for (int i = 0; i < blpFilesChunk.Length; i++)
            {
                if (blpFilesChunk[i] == '\0')
                {
                    if (str.Length > 1)
                    {
                        blpFiles.Add(str.ToString());
                        Console.WriteLine("     " + str.ToString());
                    }
                    str = new StringBuilder();
                }
                else
                {
                    str.Append((char)blpFilesChunk[i]);
                }
            }
        }
    }
}
