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

            Console.WriteLine("Loading " + y + "_" + x + " ADT for map " + mapname);

            string filename = Path.Combine(basedir, "World\\Maps\\" + mapname + "\\" + mapname + "_" + y + "_" + x); // x and y are flipped because blizzard

            FileStream adt = File.Open(filename + ".adt", FileMode.Open);

            BinaryReader bin = new BinaryReader(adt);
            BlizzHeader chunk;
            long position = 0;
            while (position < adt.Length)
            {
                adt.Position = position;
                chunk = new BlizzHeader(bin.ReadChars(4), bin.ReadUInt32());
                chunk.Flip();
                position = adt.Position + chunk.Size;

                if (chunk.Is("MVER")){ if (bin.ReadUInt32() != 18){throw new Exception("Unsupported ADT version!");} continue; }
                if (chunk.Is("MHDR")){ continue; }
                if (chunk.Is("MH2O")){ continue; }
                if (chunk.Is("MCNK")){ continue; }
                if (chunk.Is("MFBO")){ continue; }

                //model.blob stuff
                if (chunk.Is("MBMH")){ continue; }
                if (chunk.Is("MBBB")){ continue; }
                if (chunk.Is("MBMI")){ continue; }
                if (chunk.Is("MBNV")){ continue; }

                throw new Exception(String.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunk.ToString(), position.ToString(), filename));
            }

            FileStream adtobj0 = File.Open(filename + "_obj0.adt", FileMode.Open);
            bin = new BinaryReader(adtobj0);
            position = 0;
            Console.WriteLine("Loading " + y + "_" + x + " OBJ0 ADT for map " + mapname);

            while (position < adtobj0.Length)
            {
                adtobj0.Position = position;
                chunk = new BlizzHeader(bin.ReadChars(4), bin.ReadUInt32());
                chunk.Flip();
                position = adtobj0.Position + chunk.Size;

                if (chunk.Is("MVER")){ if (bin.ReadUInt32() != 18) { throw new Exception("Unsupported ADT version!"); } continue; }
                if (chunk.Is("MMDX")) { ReadMMDXChunk(chunk, bin); continue; }
                if (chunk.Is("MMID")) { continue; }
                if (chunk.Is("MWMO")) { ReadMWMOChunk(chunk, bin); continue; }
                if (chunk.Is("MWID")) { continue; }
                if (chunk.Is("MDDF")) { continue; }
                if (chunk.Is("MODF")) { continue; }
                if (chunk.Is("MCNK")) { continue; }

                throw new Exception(String.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunk.ToString(), position.ToString(), filename));
            }

            FileStream adtobj1 = File.Open(filename + "_obj1.adt", FileMode.Open);
            bin = new BinaryReader(adtobj1);
            position = 0;
            Console.WriteLine("Loading " + y + "_" + x + " OBJ1 ADT for map " + mapname);

            while (position < adtobj1.Length)
            {
                adtobj1.Position = position;
                chunk = new BlizzHeader(bin.ReadChars(4), bin.ReadUInt32());
                chunk.Flip();
                position = adtobj1.Position + chunk.Size;

                if (chunk.Is("MVER")){ if (bin.ReadUInt32() != 18) { throw new Exception("Unsupported ADT version!"); } continue; }
                if (chunk.Is("MMDX")) { ReadMMDXChunk(chunk, bin);  continue; }
                if (chunk.Is("MMID")) { continue; }
                if (chunk.Is("MWMO")) { ReadMWMOChunk(chunk, bin);  continue; }
                if (chunk.Is("MWID")) { continue; }
                if (chunk.Is("MDDF")) { continue; }
                if (chunk.Is("MODF")) { continue; }
                if (chunk.Is("MCNK")) { continue; }

                throw new Exception(String.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunk.ToString(), position.ToString(), filename));
            }

            FileStream adttex0 = File.Open(filename + "_tex0.adt", FileMode.Open);
            bin = new BinaryReader(adttex0);
            position = 0;
            Console.WriteLine("Loading " + y + "_" + x + " TEX0 ADT for map " + mapname);

            while (position < adttex0.Length)
            {
                adttex0.Position = position;
                chunk = new BlizzHeader(bin.ReadChars(4), bin.ReadUInt32());
                chunk.Flip();
                position = adttex0.Position + chunk.Size;

                if (chunk.Is("MVER")) { if (bin.ReadUInt32() != 18) { throw new Exception("Unsupported ADT version!"); } continue; }
                if (chunk.Is("MAMP")) { continue; }
                if (chunk.Is("MTEX")) { ReadMTEXChunk(chunk, bin);  continue; }
                if (chunk.Is("MCNK")) { continue; }
                if (chunk.Is("MTXP")) { continue; }

                throw new Exception(String.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunk.ToString(), position.ToString(), filename));
            }

            FileStream adttex1 = File.Open(filename + "_tex1.adt", FileMode.Open);
            bin = new BinaryReader(adttex1);
            position = 0;
            Console.WriteLine("Loading " + y + "_" + x + " TEX1 ADT for map " + mapname);

            while (position < adttex1.Length)
            {
                adttex1.Position = position;
                chunk = new BlizzHeader(bin.ReadChars(4), bin.ReadUInt32());
                chunk.Flip();
                position = adttex1.Position + chunk.Size;

                if (chunk.Is("MVER")) { if (bin.ReadUInt32() != 18) { throw new Exception("Unsupported ADT version!"); } continue; }
                if (chunk.Is("MAMP")) { continue; }
                if (chunk.Is("MTEX")) { ReadMTEXChunk(chunk, bin);  continue; }
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
                }else{
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
