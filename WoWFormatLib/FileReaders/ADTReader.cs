using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WoWFormatLib.Structs.ADT;
using WoWFormatLib.Utils;

namespace WoWFormatLib.FileReaders
{
    public class ADTReader
    {
        private string basedir;
        private List<String> blpFiles;
        private List<String> m2Files;
        private List<String> wmoFiles;

        public ADTReader(string basedir)
        {
            this.basedir = basedir;
        }

        public void LoadADT(string filename)
        {
            m2Files = new List<string>();
            wmoFiles = new List<string>();
            blpFiles = new List<string>();

            filename = Path.ChangeExtension(filename, ".adt");

            if (!File.Exists(Path.Combine(basedir, filename))) { new WoWFormatLib.Utils.MissingFile(filename); return; }
            if (!File.Exists(Path.Combine(basedir, filename).Replace(".adt", "_obj0.adt"))) { new WoWFormatLib.Utils.MissingFile(filename.Replace(".adt", "_obj0.adt")); return; }
            if (!File.Exists(Path.Combine(basedir, filename).Replace(".adt", "_obj1.adt"))) { new WoWFormatLib.Utils.MissingFile(filename.Replace(".adt", "_obj1.adt")); return; }
            if (!File.Exists(Path.Combine(basedir, filename).Replace(".adt", "_tex0.adt"))) { new WoWFormatLib.Utils.MissingFile(filename.Replace(".adt", "_tex0.adt")); return; }
            if (!File.Exists(Path.Combine(basedir, filename).Replace(".adt", "_tex1.adt"))) { new WoWFormatLib.Utils.MissingFile(filename.Replace(".adt", "_tex1.adt")); return; }

            var adt = File.Open(Path.Combine(basedir, filename), FileMode.Open);

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

            using (var adtobj0 = File.Open(Path.Combine(basedir, filename).Replace(".adt", "_obj0.adt"), FileMode.Open))
            {
                ReadObjFile(filename, adtobj0, ref chunk);
            }

            using (FileStream adttex0 = File.Open(Path.Combine(basedir, filename).Replace(".adt", "_tex0.adt"), FileMode.Open))
            {
                ReadTexFile(filename, adttex0, ref chunk);
            }
        }

        public void ReadMCNKChunk(BlizzHeader chunk, BinaryReader bin)
        {
            //Has subchunks :(
            //MCVT subchunk has 145 floats
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

        private void ReadObjFile(string filename, FileStream adtObjStream, ref BlizzHeader chunk)
        {
            var bin = new BinaryReader(adtObjStream);
            long position = 0;

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

        private void ReadTexFile(string filename, FileStream adtTexStream, ref BlizzHeader chunk)
        {
            var bin = new BinaryReader(adtTexStream);
            long position = 0;

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
    }
}