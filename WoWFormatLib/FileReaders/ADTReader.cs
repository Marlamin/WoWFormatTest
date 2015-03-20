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
        public ADT adtfile;
        private List<String> blpFiles;
        private List<String> m2Files;
        private List<String> wmoFiles;

        public ADTReader()
        {
        }

        public void LoadADT(string filename, bool loadSecondaryADTs = false)
        {
            m2Files = new List<string>();
            wmoFiles = new List<string>();
            blpFiles = new List<string>();

            filename = Path.ChangeExtension(filename, ".adt");

            if (!CASC.FileExists(filename)) { new WoWFormatLib.Utils.MissingFile(filename); return; }
            if (!CASC.FileExists(filename.Replace(".adt", "_obj0.adt"))) { new WoWFormatLib.Utils.MissingFile(filename.Replace(".adt", "_obj0.adt")); return; }
            if (!CASC.FileExists(filename.Replace(".adt", "_obj1.adt"))) { new WoWFormatLib.Utils.MissingFile(filename.Replace(".adt", "_obj1.adt")); return; }
            if (!CASC.FileExists(filename.Replace(".adt", "_tex0.adt"))) { new WoWFormatLib.Utils.MissingFile(filename.Replace(".adt", "_tex0.adt")); return; }
            if (!CASC.FileExists(filename.Replace(".adt", "_tex1.adt"))) { new WoWFormatLib.Utils.MissingFile(filename.Replace(".adt", "_tex1.adt")); return; } 

            var adt = CASC.OpenFile(filename);

            var bin = new BinaryReader(adt);
            BlizzHeader chunk = null;
            long position = 0;
            int MCNKi = 0;
            adtfile.chunks = new MCNK[16 * 16];
            while (position < adt.Length)
            {
                adt.Position = position;
                chunk = new BlizzHeader(bin.ReadChars(4), bin.ReadUInt32());
                chunk.Flip();
                position = adt.Position + chunk.Size;

                switch (chunk.ToString())
                {
                    case "MVER":
                        uint version = bin.ReadUInt32();
                        if (version != 18)
                        {
                            throw new Exception("Unsupported ADT version!");
                        }
                        else
                        {
                            adtfile.version = version;
                        }
                        continue;
                    case "MCNK":
                        adtfile.chunks[MCNKi] = ReadMCNKChunk(chunk, bin);
                        MCNKi++;
                        continue;
                    case "MHDR":
                        adtfile.header = ReadMHDRChunk(chunk, bin);
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

            //OBJ1 and TEX1 are ignored atm
            if (loadSecondaryADTs)
            {
                using (var adtobj0 = CASC.OpenFile(filename.Replace(".adt", "_obj0.adt")))
                {
                    ReadObjFile(filename, adtobj0, ref chunk);
                }

                using (var adttex0 = CASC.OpenFile(filename.Replace(".adt", "_tex0.adt")))
                {
                    ReadTexFile(filename, adttex0, ref chunk);
                }
            }
        }

        public MCNK ReadMCNKChunk(BlizzHeader chunk, BinaryReader bin)
        {
            //256 of these chunks per file
            MCNK mapchunk = new MCNK();
            
            mapchunk.header = bin.Read<MCNKheader>();

            MemoryStream stream = new MemoryStream(bin.ReadBytes((int)chunk.Size - 128));
            
            var subbin = new BinaryReader(stream);
            
            BlizzHeader subchunk;
            
            long subpos = 0;

            while (subpos < stream.Length)
            {
                subbin.BaseStream.Position = subpos;
                subchunk = new BlizzHeader(subbin.ReadChars(4), subbin.ReadUInt32());
                subchunk.Flip();
                subpos = stream.Position + subchunk.Size;

                switch (subchunk.ToString())
                {
                    case "MCVT":
                        mapchunk.vertices = ReadMCVTSubChunk(subchunk, subbin);
                        break;
                    case "MCCV":
                        mapchunk.vertexshading = ReadMCCVSubChunk(subchunk, subbin);
                        break;
                    case "MCNR":
                    case "MCSE":
                    case "MCBB":
                    case "MCLV":
                        continue;
                    default:
                        throw new Exception(String.Format("Found unknown header at offset {1} \"{0}\" while we should've already read them all!", subchunk.ToString(), subpos.ToString()));
                }
            }
            return mapchunk;
        }

        public MCVT ReadMCVTSubChunk(BlizzHeader chunk, BinaryReader bin)
        {
            MCVT vtchunk = new MCVT();
            vtchunk.vertices = new float[145];
            for (int i = 0; i < 145; i++)
            {
                vtchunk.vertices[i] = bin.ReadSingle();
            }
            return vtchunk;
        }

        public MCCV ReadMCCVSubChunk(BlizzHeader chunk, BinaryReader bin)
        {
            MCCV vtchunk = new MCCV();
            vtchunk.red = new byte[145];
            vtchunk.green = new byte[145];
            vtchunk.blue = new byte[145];
            vtchunk.alpha = new byte[145];
            for (int i = 0; i < 145; i++)
            {
                vtchunk.red[i] = bin.ReadByte();
                vtchunk.green[i] = bin.ReadByte();
                vtchunk.blue[i] = bin.ReadByte();
                vtchunk.alpha[i] = bin.ReadByte();
            }
            return vtchunk;
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
                        var m2reader = new M2Reader();
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
                        if (!CASC.FileExists(str.ToString()))
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
                        //var wmoreader = new WMOReader();
                        //wmoreader.LoadWMO(str.ToString());
                    }
                    str = new StringBuilder();
                }
                else
                {
                    str.Append((char)wmoFilesChunk[i]);
                }
            }
        }

        private MHDR ReadMHDRChunk(BlizzHeader chunk, BinaryReader bin)
        {
            return bin.Read<MHDR>();
        }

        private void ReadObjFile(string filename, Stream adtObjStream, ref BlizzHeader chunk)
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

        private void ReadTexFile(string filename, Stream adtTexStream, ref BlizzHeader chunk)
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