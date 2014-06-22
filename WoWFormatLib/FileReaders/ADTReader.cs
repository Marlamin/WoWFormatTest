using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using WoWFormatLib.Utils;

namespace WoWFormatLib.FileReaders
{
    public class ADTReader
    {
        private List<String> m2Files;
        private List<String> wmoFiles;
        private List<String> blpFiles;
        private string basedir;

        public ADTReader(string basedir)
        {
            this.basedir = basedir;
        }

        public void LoadADT(string mapname, int x, int y)
        {
            m2Files = new List<string>();
            wmoFiles = new List<string>();
            blpFiles = new List<string>();

            var adtname = "World\\Maps\\" + mapname + "\\" + mapname + "_" + y + "_" + x;
            var filename = Path.Combine(basedir, adtname); // x and y are flipped because blizzard

            if (!File.Exists(filename + ".adt")) { throw new FileNotFoundException(adtname + ".adt"); }
            if (!File.Exists(filename + "_obj0.adt")) { throw new FileNotFoundException(adtname + "_obj0.adt"); }
            if (!File.Exists(filename + "_obj1.adt")) { throw new FileNotFoundException(adtname + "_obj1.adt"); }
            if (!File.Exists(filename + "_tex0.adt")) { throw new FileNotFoundException(adtname + "_tex0.adt"); }
            if (!File.Exists(filename + "_tex1.adt")) { throw new FileNotFoundException(adtname + "_tex1.adt"); }

            var adt = File.Open(filename + ".adt", FileMode.Open);

            var bin = new BinaryReader(adt);
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
                    case "MCNK":
                        ReadMCNKChunk(chunk, bin);
                        continue;
                    case "MHDR":
                        ReadMHDRChunk(chunk, bin);
                        continue;
                    case "MH2O":
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

            using (var adtobj0 = File.Open(filename + "_obj0.adt", FileMode.Open))
            {
                ReadObjFile(mapname, x, y, filename, adtobj0, "OBJ0", ref chunk);
            }

            using (FileStream adttex0 = File.Open(filename + "_tex0.adt", FileMode.Open))
            {
                ReadTexFile(mapname, x, y, filename, adttex0, "TEX0", ref chunk);
            }
        }

        private void ReadMHDRChunk(BlizzHeader chunk, BinaryReader bin)
        {
            var pad = bin.ReadUInt32();
            var offsInfo = bin.ReadUInt32();
            var offsTex = bin.ReadUInt32();
            var offsModels = bin.ReadUInt32();
            var offsModelsIds = bin.ReadUInt32();
            var offsMapObejcts = bin.ReadUInt32();
            var offsMapObejctsIds = bin.ReadUInt32();
            var offsDoodsDef = bin.ReadUInt32();
            var offsObjectsDef = bin.ReadUInt32();
            var pad1 = bin.ReadUInt32();
            var pad2 = bin.ReadUInt32();
            var pad3 = bin.ReadUInt32();
            var pad4 = bin.ReadUInt32();
            var pad5 = bin.ReadUInt32();
            var pad6 = bin.ReadUInt32();
            var pad7 = bin.ReadUInt32();
        }

        private void ReadObjFile(string mapname, int x, int y, string filename, FileStream adtObjStream, string objName, ref BlizzHeader chunk)
        {
            var bin = new BinaryReader(adtObjStream);
            long position = 0;

            //Console.WriteLine("Loading {0}_{1} {2} ADT for map {3}", y, x, objName, mapname);

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

        private void ReadTexFile(string mapname, int x, int y, string filename, FileStream adtTexStream, string texName, ref BlizzHeader chunk)
        {
            var bin = new BinaryReader(adtTexStream);
            long position = 0;
            //Console.WriteLine("Loading {0}_{1} {2} ADT for map {3}", y, x, texName, mapname);

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
            var wmoFilesChunk = bin.ReadBytes((int)chunk.Size);

            var str = new StringBuilder();

            for (int i = 0; i < wmoFilesChunk.Length; i++)
            {
                if (wmoFilesChunk[i] == '\0')
                {
                    if (str.Length > 1)
                    {
                        wmoFiles.Add(str.ToString());
                        var wmoreader = new WMOReader(basedir);
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
            var m2FilesChunk = bin.ReadBytes((int)chunk.Size);

            var str = new StringBuilder();

            for (var i = 0; i < m2FilesChunk.Length; i++)
            {
                if (m2FilesChunk[i] == '\0')
                {
                    if (str.Length > 1)
                    {
                        var m2reader = new M2Reader(basedir);
                        m2reader.LoadM2(str.ToString());
                        m2Files.Add(str.ToString());
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
            var blpFilesChunk = bin.ReadBytes((int)chunk.Size);

            var str = new StringBuilder();

            for (var i = 0; i < blpFilesChunk.Length; i++)
            {
                if (blpFilesChunk[i] == '\0')
                {
                    if (str.Length > 1)
                    {
                        blpFiles.Add(str.ToString());
                        if (!System.IO.File.Exists(System.IO.Path.Combine(basedir, str.ToString())))
                        {
                            Console.WriteLine("BLP file does not exist!!! {0}", str.ToString());
                            throw new FileNotFoundException(str.ToString());
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
        public void ReadMCNKChunk(BlizzHeader chunk, BinaryReader bin)
        {
            //this will be called 256 times per adt, needs to be v optimized
        }
    }
}
