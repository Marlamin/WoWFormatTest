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
        public List<string> blpFiles;
        public List<string> m2Files;
        public List<string> wmoFiles;
        private WoWFormatLib.Structs.WDT.WDT wdt;

        public ADTReader()
        {
        }

        public void LoadADT(string filename, bool loadSecondaryADTs = true, bool filenamesOnly = false, bool localFile = false)
        {
            m2Files = new List<string>();
            wmoFiles = new List<string>();
            blpFiles = new List<string>();

            filename = Path.ChangeExtension(filename, ".adt");

            if (!localFile)
            {
                if (!CASC.FileExists(filename)) { new WoWFormatLib.Utils.MissingFile(filename); return; }
                if (!CASC.FileExists(filename.Replace(".adt", "_obj0.adt"))) { new WoWFormatLib.Utils.MissingFile(filename.Replace(".adt", "_obj0.adt")); return; }
                if (!CASC.FileExists(filename.Replace(".adt", "_obj1.adt"))) { new WoWFormatLib.Utils.MissingFile(filename.Replace(".adt", "_obj1.adt")); return; }
                if (!CASC.FileExists(filename.Replace(".adt", "_tex0.adt"))) { new WoWFormatLib.Utils.MissingFile(filename.Replace(".adt", "_tex0.adt")); return; }
                if (!CASC.FileExists(filename.Replace(".adt", "_tex1.adt"))) { new WoWFormatLib.Utils.MissingFile(filename.Replace(".adt", "_tex1.adt")); return; }
            }
            else
            {
                if (!File.Exists(filename)) { throw new Exception("Missing file!"); }
            }

            Stream adt;

            if (!localFile)
            {
                var mapname = filename.Replace("World/Maps/", "").Substring(0, filename.Replace("World/Maps/", "").IndexOf("/"));

                if (CASC.FileExists("World/Maps/" + mapname + "/" + mapname + ".wdt"))
                {
                    var wdtr = new WDTReader();
                    wdtr.LoadWDT("World/Maps/" + mapname + "/" + mapname + ".wdt");
                    wdt = wdtr.wdtfile;
                }
                else
                {
                    throw new Exception("WDT does not exist, need this for MCAL flags!");
                }


                adt = CASC.OpenFile(filename);
            }
            else
            {
                adt = File.OpenRead(filename);
            }


            BlizzHeader chunk = null;
            if (filenamesOnly == false)
            {
                var bin = new BinaryReader(adt);
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
            }
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
                        mapchunk.vertexShading = ReadMCCVSubChunk(subchunk, subbin);
                        break;
                    case "MCNR":
                        mapchunk.normals = ReadMCNRSubChunk(subchunk, subbin);
                        break;
                    case "MCSE":
                        mapchunk.soundEmitters = ReadMCSESubChunk(subchunk, subbin);
                        break;
                    case "MCBB":
                        mapchunk.blendBatches = ReadMCBBSubChunk(subchunk, subbin);
                        break;
                    case "MCLQ":
                    case "MCLV":
                        continue;
                    default:
                        throw new Exception(String.Format("Found unknown header at offset {1} \"{0}\" while we should've already read them all!", subchunk.ToString(), subpos.ToString()));
                }
            }
            return mapchunk;
        }

        public TexMCNK ReadTexMCNKChunk(BlizzHeader chunk, BinaryReader bin)
        {
            //256 of these chunks per file
            TexMCNK mapchunk = new TexMCNK();

            MemoryStream stream = new MemoryStream(bin.ReadBytes((int)chunk.Size));

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
                    case "MCLY":
                        mapchunk.layers = ReadMCLYSubChunk(subchunk, subbin);
                        break;
                    case "MCAL":
                        mapchunk.alphaLayer = ReadMCALSubChunk(subchunk, subbin, mapchunk);
                        break;
                    case "MCSH":
                        continue;
                    default:
                        throw new Exception(String.Format("Found unknown header at offset {1} \"{0}\" while we should've already read them all!", subchunk.ToString(), subpos.ToString()));
                }
            }
            return mapchunk;
        }

        private MCAL[] ReadMCALSubChunk(BlizzHeader subchunk, BinaryReader subbin, TexMCNK mapchunk)
        {
            var mcal = new MCAL[mapchunk.layers.Length];

            mcal[0].layer = new byte[64 * 64];
            for(int i = 0; i < 64 * 64; i++)
            {
                mcal[0].layer[i] = 255;
            }

            uint read_offset = 0;

            for (int layer = 1; layer < mapchunk.layers.Length; ++layer)
            {
                // we assume that we have read as many bytes as this next layer's mcal offset. we then read depending on encoding
                if (mapchunk.layers[layer].offsetInMCAL != read_offset)
                {
                   throw new Exception("mismatch: layer before required more / less bytes than expected");
                }
                if (mapchunk.layers[layer].flags.HasFlag (mclyFlags.Flag_0x200))
                {
                     // first layer is always fully opaque -> you can let that out
                    // array of 3 x array of 64*64 chars: unpacked alpha values
                    mcal[layer].layer = new byte[64 * 64];

                    // sorry, I have no god damn idea about c#
                    // *x = value at x. x = pointer to data. ++x = advance üpointer a byte
                    uint in_offset = 0;
                    uint out_offset = 0;
                    while (out_offset < 4096)
                    {
                        byte info = subbin.ReadByte(); ++in_offset;
                        uint mode = (uint)(info & 0x80) >> 7; // 0 = copy, 1 = fill
                        uint count = (uint)(info & 0x7f); // do mode operation count times
                        
                        if (mode != 0)
                        {
                            byte val = subbin.ReadByte(); ++in_offset;
                            while (count --> 0 && out_offset < 4096)
                            {
                                mcal[layer].layer[out_offset] = val;
                                ++out_offset;
                            }
                           
                        }
                        else // mode == 1
                        {
                            while (count --> 0 && out_offset < 4096)
                            {
                                var val = subbin.ReadByte(); ++in_offset; 
                                mcal[layer].layer[out_offset] = val;
                                ++out_offset;
                            }
                        }
                    }
                    read_offset += in_offset;
                    if (out_offset != 4096) throw new Exception("we somehow overshoot. this should not be the case, except for broken adts");
                }
                else if (wdt.mphd.flags.HasFlag(WoWFormatLib.Structs.WDT.mphdFlags.Flag_0x4) || wdt.mphd.flags.HasFlag(WoWFormatLib.Structs.WDT.mphdFlags.Flag_0x80))
                {
                    mcal[layer].layer = subbin.ReadBytes(4096);
                    read_offset += 4096;
                }
                else
                {
                    mcal[layer].layer = new byte[64 * 64];  //uncompressed_2048
                    var mcal_data = subbin.ReadBytes(2048);
                    read_offset += 2048;
                    for (int i = 0; i < 2048; ++i)
                    {
                        // maybe nibbles swapped
                        mcal[layer].layer[2 * i + 0] = (byte) (((mcal_data[i] & 0x0F) >> 0) * 17);
                        mcal[layer].layer[2 * i + 1] = (byte) (((mcal_data[i] & 0xF0) >> 4) * 17);
                    }
                }
            }

            if (read_offset != subchunk.Size) throw new Exception("Haven't finished reading chunk but should be");

            return mcal;
            /*
            var mcal = new MCAL();

            var mphdFlag = false;
            var mclyFlag = false;

            if (wdt.mphd.flags.HasFlag(WoWFormatLib.Structs.WDT.mphdFlags.Flag_0x4)){
                mphdFlag = true;
            }

            if (mapchunk.layers[0].flags.HasFlag(mclyFlags.Flag_0x200)){
                mclyFlag = true;
            }

            mcal.alpha = new byte[64][];

            if(!mphdFlag && !mclyFlag)
            {
                for(int x = 0; x < 64; x++)
                {
                    mcal.alpha[x] = new byte[64];
                    for(int y = 0; y < 64; y++)
                    {
                        mcal.alpha[x][y] = subbin.ReadByte();
                    }
                }
            }
            else
            {
                throw new Exception("Unsupported MCAL detected!");
            }

            return mcal;
            */

        }

        public MCLY[] ReadMCLYSubChunk(BlizzHeader chunk, BinaryReader bin)
        {
            var count = chunk.Size / 16;
            MCLY[] mclychunks = new MCLY[count];
            for (int i = 0; i < count; i++)
            {
                mclychunks[i].textureId = bin.ReadUInt32();
                mclychunks[i].flags = (mclyFlags) bin.ReadUInt32();
                mclychunks[i].offsetInMCAL = bin.ReadUInt32();
                mclychunks[i].effectId = bin.ReadInt32();
            }

            return mclychunks;
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

        public MCNR ReadMCNRSubChunk(BlizzHeader chunk, BinaryReader bin)
        {
            MCNR nrchunk = new MCNR();
            nrchunk.normal_0 = new short[145];
            nrchunk.normal_1 = new short[145];
            nrchunk.normal_2 = new short[145];
            for (int i = 0; i < 145; i++)
            {
                nrchunk.normal_0[i] = bin.ReadSByte();
                nrchunk.normal_1[i] = bin.ReadSByte();
                nrchunk.normal_2[i] = bin.ReadSByte();
            }
            return nrchunk;
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

        public MCSE ReadMCSESubChunk(BlizzHeader subchunk, BinaryReader subbin)
        {
            MCSE sechunk = new MCSE();
            sechunk.raw = subbin.ReadBytes((int)subchunk.Size);
            return sechunk;
        }

        public MCBB[] ReadMCBBSubChunk(BlizzHeader subchunk, BinaryReader subbin)
        {
            var count = subchunk.Size / 20;
            MCBB[] bbchunk = new MCBB[count];
            for(int i = 0; i < count; i++)
            {
                bbchunk[i] = subbin.Read<MCBB>();
            }
            return bbchunk;
        }

        public MTEX ReadMTEXChunk(BlizzHeader chunk, BinaryReader bin)
        {
            MTEX txchunk = new MTEX();

            //List of BLP filenames
            var blpFilesChunk = bin.ReadBytes((int)chunk.Size);
            
            var str = new StringBuilder();

            for (var i = 0; i < blpFilesChunk.Length; i++)
            {
                if (blpFilesChunk[i] == '\0')
                {
                    blpFiles.Add(str.ToString());
                    if (!CASC.FileExists(str.ToString()))
                    {
                        Console.WriteLine("BLP file does not exist!!! {0}", str.ToString());
                        new WoWFormatLib.Utils.MissingFile(str.ToString());
                    }
                    str = new StringBuilder();
                }
                else
                {
                    str.Append((char)blpFilesChunk[i]);
                }
            }

            txchunk.filenames = blpFiles.ToArray();
            return txchunk;

        }

        private MHDR ReadMHDRChunk(BlizzHeader chunk, BinaryReader bin)
        {
            return bin.Read<MHDR>();
        }

        private void ReadObjFile(string filename, Stream adtObjStream, ref BlizzHeader chunk)
        {
            var bin = new BinaryReader(adtObjStream);
            long position = 0;

            adtfile.objects = new Obj();

            while (position < adtObjStream.Length)
            {
                adtObjStream.Position = position;
                chunk = new BlizzHeader(bin.ReadChars(4), bin.ReadUInt32());
                chunk.Flip();
                position = adtObjStream.Position + chunk.Size;

                if (chunk.Is("MVER")) { if (bin.ReadUInt32() != 18) { throw new Exception("Unsupported ADT version!"); } continue; }
                if (chunk.Is("MMDX")) { adtfile.objects.m2Names = ReadMMDXChunk(chunk, bin); continue; }
                if (chunk.Is("MMID")) { adtfile.objects.m2NameOffsets = ReadMMIDChunk(chunk, bin); continue; }
                if (chunk.Is("MWMO")) { adtfile.objects.wmoNames = ReadMWMOChunk(chunk, bin); continue; }
                if (chunk.Is("MWID")) { adtfile.objects.wmoNameOffsets = readMWIDChunk(chunk, bin);  continue; }
                if (chunk.Is("MDDF")) { adtfile.objects.models = ReadMWIDChunk(chunk, bin); continue; }
                if (chunk.Is("MODF")) { adtfile.objects.worldModels = ReadMODFChunk(chunk, bin); continue; }
                if (chunk.Is("MCNK")) { continue; } // Only has MCRD and other useless things nobody cares about!

                throw new Exception(String.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunk.ToString(), position.ToString(), filename));
            }
        }

        private MMDX ReadMMDXChunk(BlizzHeader chunk, BinaryReader bin)
        {
            //List of M2 filenames, but are still named after MDXs internally. Have to rename!
            var m2FilesChunk = bin.ReadBytes((int)chunk.Size);
            MMDX mmdx = new MMDX();
            var str = new StringBuilder();

            List<uint> offsets = new List<uint>();

            for (var i = 0; i < m2FilesChunk.Length; i++)
            {
                if (m2FilesChunk[i] == '\0')
                {
                    m2Files.Add(str.ToString());
                    offsets.Add((uint)(i - str.ToString().Length));
                    str = new StringBuilder();
                }
                else
                {
                    str.Append((char)m2FilesChunk[i]);
                }
            }

            mmdx.filenames = m2Files.ToArray();
            mmdx.offsets = offsets.ToArray();
            return mmdx;
        }

        private MMID ReadMMIDChunk(BlizzHeader chunk, BinaryReader bin)
        {
            var count = chunk.Size / 4;

            MMID mmid = new MMID();

            mmid.offsets = new uint[count];
            for(int i = 0; i < count; i++)
            {
                mmid.offsets[i] = bin.ReadUInt32();
            }

            return mmid;
        }

        private MWID readMWIDChunk(BlizzHeader chunk, BinaryReader bin)
        {
            var count = chunk.Size / 4;

            MWID mwid = new MWID();

            mwid.offsets = new uint[count];
            for (int i = 0; i < count; i++)
            {
                mwid.offsets[i] = bin.ReadUInt32();
            }

            return mwid;
        }

        private MWMO ReadMWMOChunk(BlizzHeader chunk, BinaryReader bin)
        {
            //List of WMO filenames
            var wmoFilesChunk = bin.ReadBytes((int)chunk.Size);

            MWMO mwmo = new MWMO();
            var str = new StringBuilder();

            List<uint> offsets = new List<uint>();

            for (int i = 0; i < wmoFilesChunk.Length; i++)
            {
                if (wmoFilesChunk[i] == '\0')
                {
                    wmoFiles.Add(str.ToString());
                    offsets.Add((uint)(i - str.ToString().Length));
                    str = new StringBuilder();
                }
                else
                {
                    str.Append((char)wmoFilesChunk[i]);
                }
            }

            mwmo.offsets = offsets.ToArray();
            mwmo.filenames = wmoFiles.ToArray();
            return mwmo;
        }
        private MDDF ReadMWIDChunk(BlizzHeader chunk, BinaryReader bin)
        {
            MDDF mddf = new MDDF();

            var count = chunk.Size / 36; //36 bytes per entry?

           // Console.WriteLine(count + " MDDF entries!");

            mddf.entries = new MDDFEntry[count];

            for (int i = 0; i < count; i++)
            {
                mddf.entries[i] = bin.Read<MDDFEntry>();
            }

            return mddf;
        }

        private MODF ReadMODFChunk(BlizzHeader chunk, BinaryReader bin)
        {
            MODF modf = new MODF();

            var count = chunk.Size / 64; //64 bytes per entry?

           // Console.WriteLine(count + " MODF entries!");

            modf.entries = new MODFEntry[count];
            for(int i = 0; i < count; i++)
            {
                modf.entries[i] = bin.Read<MODFEntry>();
            }

            return modf;
        }

        private void ReadTexFile(string filename, Stream adtTexStream, ref BlizzHeader chunk)
        {
            var bin = new BinaryReader(adtTexStream);
            long position = 0;
            int MCNKi = 0;
            adtfile.texChunks = new TexMCNK[16 * 16];

            while (position < adtTexStream.Length)
            {
                adtTexStream.Position = position;
                chunk = new BlizzHeader(bin.ReadChars(4), bin.ReadUInt32());
                chunk.Flip();
                position = adtTexStream.Position + chunk.Size;
                //Console.WriteLine("Chunk " + MCNKi);
                if (chunk.Is("MVER")) { if (bin.ReadUInt32() != 18) { throw new Exception("Unsupported ADT version!"); } continue; }
                if (chunk.Is("MAMP")) { continue; }
                if (chunk.Is("MTEX")) { adtfile.textures = ReadMTEXChunk(chunk, bin); continue; }
                if (chunk.Is("MCNK")) { adtfile.texChunks[MCNKi] = ReadTexMCNKChunk(chunk, bin); MCNKi++;  continue; }
                if (chunk.Is("MTXP")) { continue; }

                throw new Exception(String.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunk.ToString(), position.ToString(), filename));
            }
        }
    }
}