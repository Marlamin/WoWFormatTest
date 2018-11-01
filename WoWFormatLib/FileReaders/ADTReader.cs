using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private Structs.WDT.WDT wdt;

        /* ROOT */
        public void LoadADT(string filename, bool loadSecondaryADTs = true)
        {
            m2Files = new List<string>();
            wmoFiles = new List<string>();
            blpFiles = new List<string>();

            filename = Path.ChangeExtension(filename, ".adt");

            if (!CASC.FileExists(filename) || !CASC.FileExists(filename.Replace(".adt", "_obj0.adt")) || !CASC.FileExists(filename.Replace(".adt", "_tex0.adt"))) {
                throw new FileNotFoundException("One or more ADT files for ADT " + filename + " could not be found.");
            }

            var mapname = filename.Replace("world\\maps\\", "").Substring(0, filename.Replace("world\\maps\\", "").IndexOf("\\"));

            if (CASC.FileExists("world\\maps\\" + mapname + "\\" + mapname + ".wdt"))
            {
                var wdtr = new WDTReader();
                wdtr.LoadWDT("world\\maps\\" + mapname + "\\" + mapname + ".wdt");
                wdt = wdtr.wdtfile;
            }
            else
            {
                throw new Exception("WDT does not exist, need this for MCAL flags!");
            }

            using (var adt = CASC.OpenFile(filename))
            using (var bin = new BinaryReader(adt))
            {
                long position = 0;
                var MCNKi = 0;
                adtfile.chunks = new MCNK[16 * 16];

                while (position < adt.Length)
                {
                    adt.Position = position;

                    var chunkName = (ADTChunks)bin.ReadUInt32();
                    var chunkSize = bin.ReadUInt32();

                    position = adt.Position + chunkSize;

                    switch (chunkName)
                    {
                        case ADTChunks.MVER:
                            var version = bin.ReadUInt32();
                            if (version != 18)
                            {
                                throw new Exception("Unsupported ADT version!");
                            }
                            else
                            {
                                adtfile.version = version;
                            }
                            break;
                        case ADTChunks.MCNK:
                            adtfile.chunks[MCNKi] = ReadMCNKChunk(chunkSize, bin);
                            MCNKi++;
                            break;
                        case ADTChunks.MHDR:
                            adtfile.header = bin.Read<MHDR>();
                            break;
                        case ADTChunks.MH2O:
                            adtfile.mh2o = ReadMH20SubChunk(chunkSize, bin);
                            break;
                        case ADTChunks.MFBO:
                        //model.blob stuff
                        case ADTChunks.MBMH:
                        case ADTChunks.MBBB:
                        case ADTChunks.MBMI:
                        case ADTChunks.MBNV:
                            break;
                        default:
                            throw new Exception(string.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunkName, position, filename));
                    }
                }
            }
            
            if (loadSecondaryADTs)
            {
                using (var adtobj0 = CASC.OpenFile(filename.Replace(".adt", "_obj0.adt")))
                {
                    ReadObjFile(adtobj0);
                }

                using (var adttex0 = CASC.OpenFile(filename.Replace(".adt", "_tex0.adt")))
                {
                    ReadTexFile(adttex0);
                }
            }
        }
        private MCNK ReadMCNKChunk(uint size, BinaryReader bin)
        {
            var mapchunk = new MCNK()
            {
                header = bin.Read<MCNKheader>()
            };

            using (var stream = new MemoryStream(bin.ReadBytes((int)size - 128)))
            using (var subbin = new BinaryReader(stream))
            {
                long subpos = 0;
                while (subpos < stream.Length)
                {
                    subbin.BaseStream.Position = subpos;

                    var subChunkName = (ADTChunks)subbin.ReadUInt32();
                    var subChunkSize = subbin.ReadUInt32();

                    subpos = stream.Position + subChunkSize;

                    switch (subChunkName)
                    {
                        case ADTChunks.MCVT:
                            mapchunk.vertices = ReadMCVTSubChunk(subbin);
                            break;
                        case ADTChunks.MCCV:
                            mapchunk.vertexShading = ReadMCCVSubChunk(subbin);
                            break;
                        case ADTChunks.MCNR:
                            mapchunk.normals = ReadMCNRSubChunk(subbin);
                            break;
                        case ADTChunks.MCSE:
                            mapchunk.soundEmitters = ReadMCSESubChunk(subChunkSize, subbin);
                            break;
                        case ADTChunks.MCBB:
                            mapchunk.blendBatches = ReadMCBBSubChunk(subChunkSize, subbin);
                            break;
                        case ADTChunks.MCLQ:
                        case ADTChunks.MCLV:
                            continue;
                        default:
                            throw new Exception(string.Format("Found unknown header at offset {1} \"{0}\" while we should've already read them all!", subChunkName, subpos.ToString()));
                    }
                }
            }

            return mapchunk;
        }
        private MCVT ReadMCVTSubChunk(BinaryReader bin)
        {
            var vtchunk = new MCVT()
            {
                vertices = new float[145]
            };

            for (var i = 0; i < 145; i++)
            {
                vtchunk.vertices[i] = bin.ReadSingle();
            }
            return vtchunk;
        }
        private MCCV ReadMCCVSubChunk(BinaryReader bin)
        {
            var vtchunk = new MCCV()
            {
                red = new byte[145],
                green = new byte[145],
                blue = new byte[145],
                alpha = new byte[145]
            };

            for (var i = 0; i < 145; i++)
            {
                vtchunk.red[i] = bin.ReadByte();
                vtchunk.green[i] = bin.ReadByte();
                vtchunk.blue[i] = bin.ReadByte();
                vtchunk.alpha[i] = bin.ReadByte();
            }

            return vtchunk;
        }
        private MCNR ReadMCNRSubChunk(BinaryReader bin)
        {
            var nrchunk = new MCNR()
            {
                normal_0 = new short[145],
                normal_1 = new short[145],
                normal_2 = new short[145]
            };

            for (var i = 0; i < 145; i++)
            {
                nrchunk.normal_0[i] = bin.ReadSByte();
                nrchunk.normal_1[i] = bin.ReadSByte();
                nrchunk.normal_2[i] = bin.ReadSByte();
            }

            return nrchunk;
        }
        private MCSE ReadMCSESubChunk(uint size, BinaryReader bin)
        {
            var sechunk = new MCSE()
            {
                raw = bin.ReadBytes((int)size)
            };

            return sechunk;
        }
        private MCBB[] ReadMCBBSubChunk(uint size, BinaryReader bin)
        {
            var count = size / 20;
            var bbchunk = new MCBB[count];
            for (var i = 0; i < count; i++)
            {
                bbchunk[i] = bin.Read<MCBB>();
            }
            return bbchunk;
        }
        private MH2O ReadMH20SubChunk(uint size, BinaryReader bin)
        {
            var chunk = new MH2O();
            chunk.headers = new MH2OHeader[256];
            for(var i = 0; i < 256; i++)
            {
                chunk.headers[i].offsetInstances = bin.ReadUInt32();
                chunk.headers[i].layerCount = bin.ReadUInt32();
                chunk.headers[i].offsetAttributes = bin.ReadUInt32();
            }
            return chunk;
        }
        /* OBJ */
        private void ReadObjFile(Stream adtObjStream)
        {
            long position = 0;

            adtfile.objects = new Obj();

            using (var bin = new BinaryReader(adtObjStream))
            {
                while (position < adtObjStream.Length)
                {
                    adtObjStream.Position = position;

                    var chunkName = (ADTChunks)bin.ReadUInt32();
                    var chunkSize = bin.ReadUInt32();
                    position = adtObjStream.Position + chunkSize;

                    switch (chunkName)
                    {
                        case ADTChunks.MVER:
                            if (bin.ReadUInt32() != 18) { throw new Exception("Unsupported ADT version!"); }
                            break;
                        case ADTChunks.MMDX:
                            adtfile.objects.m2Names = ReadMMDXChunk(chunkSize, bin);
                            break;
                        case ADTChunks.MMID:
                            adtfile.objects.m2NameOffsets = ReadMMIDChunk(chunkSize, bin);
                            break;
                        case ADTChunks.MWMO:
                            adtfile.objects.wmoNames = ReadMWMOChunk(chunkSize, bin);
                            break;
                        case ADTChunks.MWID:
                            adtfile.objects.wmoNameOffsets = ReadMWIDChunk(chunkSize, bin);
                            break;
                        case ADTChunks.MDDF:
                            adtfile.objects.models = ReadMDDFChunk(chunkSize, bin);
                            break;
                        case ADTChunks.MODF:
                            adtfile.objects.worldModels = ReadMODFChunk(chunkSize, bin);
                            break;
                        case ADTChunks.MLMB:
                        case ADTChunks.MCNK:
                            // TODO
                            break;
                        default:
                            throw new Exception(string.Format("Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunkName, position));
                    }
                }
            }
        }
        private MMDX ReadMMDXChunk(uint size, BinaryReader bin)
        {
            var m2FilesChunk = bin.ReadBytes((int)size);

            var mmdx = new MMDX();
            var str = new StringBuilder();

            var offsets = new List<uint>();

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
        private MMID ReadMMIDChunk(uint size, BinaryReader bin)
        {
            var count = size / 4;

            var mmid = new MMID()
            {
                offsets = new uint[count]
            };

            for (var i = 0; i < count; i++)
            {
                mmid.offsets[i] = bin.ReadUInt32();
            }

            return mmid;
        }
        private MWMO ReadMWMOChunk(uint size, BinaryReader bin)
        {
            var wmoFilesChunk = bin.ReadBytes((int)size);

            var mwmo = new MWMO();
            var str = new StringBuilder();

            var offsets = new List<uint>();

            for (var i = 0; i < wmoFilesChunk.Length; i++)
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

            mwmo.filenames = wmoFiles.ToArray();
            mwmo.offsets = offsets.ToArray();
            return mwmo;
        }
        private MWID ReadMWIDChunk(uint size, BinaryReader bin)
        {
            var count = size / 4;

            var mwid = new MWID()
            {
                offsets = new uint[count]
            };

            for (var i = 0; i < count; i++)
            {
                mwid.offsets[i] = bin.ReadUInt32();
            }

            return mwid;
        }
        private MDDF ReadMDDFChunk(uint size, BinaryReader bin)
        {
            var mddf = new MDDF();

            var count = size / 36; 

            mddf.entries = new MDDFEntry[count];

            for (var i = 0; i < count; i++)
            {
                mddf.entries[i] = bin.Read<MDDFEntry>();
                if (mddf.entries[i].flags.HasFlag(MDDFFlags.mddf_entry_is_filedataid))
                {
                    Console.WriteLine("ADT Reader: Found a filedataid reference while parsing MDDF: {0}", mddf.entries[i].mmidEntry);
                }
            }

            return mddf;
        }
        private MODF ReadMODFChunk(uint size, BinaryReader bin)
        {
            var modf = new MODF();

            var count = size / 64; 

            modf.entries = new MODFEntry[count];
            for (var i = 0; i < count; i++)
            {
                modf.entries[i] = bin.Read<MODFEntry>();
                if (modf.entries[i].flags.HasFlag(MODFFlags.modf_entry_is_filedataid))
                {
                    Console.WriteLine("ADT Reader: Found a filedataid reference while parsing MODF: {0}", modf.entries[i].mwidEntry);
                }
            }

            return modf;
        }

        /* TEX */
        private void ReadTexFile(Stream adtTexStream)
        {
            using (var bin = new BinaryReader(adtTexStream))
            {
                long position = 0;
                var MCNKi = 0;
                adtfile.texChunks = new TexMCNK[16 * 16];

                while (position < adtTexStream.Length)
                {
                    adtTexStream.Position = position;
                    var chunkName = (ADTChunks)bin.ReadUInt32();
                    var chunkSize = bin.ReadUInt32();

                    position = adtTexStream.Position + chunkSize;
                    switch (chunkName)
                    {
                        case ADTChunks.MVER:
                            if (bin.ReadUInt32() != 18)
                            { throw new Exception("Unsupported ADT version!"); }
                            break;
                        case ADTChunks.MTEX:
                            adtfile.textures = ReadMTEXChunk(chunkSize, bin);
                            break;
                        case ADTChunks.MCNK:
                            adtfile.texChunks[MCNKi] = ReadTexMCNKChunk(chunkSize, bin);
                            MCNKi++;
                            break;
                        case ADTChunks.MTXP:
                            adtfile.texParams = ReadMTXPChunk(chunkSize, bin);
                            break;
                        case ADTChunks.MHID: // Height texture fileDataIDs
                            adtfile.heightTextureFileDataIDs = ReadFileDataIDChunk(chunkSize, bin);
                            break;
                        case ADTChunks.MDID: // Diffuse texture fileDataIDs
                            adtfile.diffuseTextureFileDataIDs = ReadFileDataIDChunk(chunkSize, bin);
                            break;
                        case ADTChunks.MAMP:
                            break;
                        default:
                            throw new Exception(string.Format("Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunkName, position));
                    }
                }
            }
        }
        private TexMCNK ReadTexMCNKChunk(uint size, BinaryReader bin)
        {
            var mapchunk = new TexMCNK();

            using (var stream = new MemoryStream(bin.ReadBytes((int)size)))
            using (var subbin = new BinaryReader(stream))
            {
                long subpos = 0;

                while (subpos < stream.Length)
                {
                    subbin.BaseStream.Position = subpos;
                    var subChunkName = (ADTChunks)subbin.ReadUInt32();
                    var subChunkSize = subbin.ReadUInt32();

                    subpos = stream.Position + subChunkSize;

                    switch (subChunkName)
                    {
                        case ADTChunks.MCLY:
                            mapchunk.layers = ReadMCLYSubChunk(subChunkSize, subbin);
                            break;
                        case ADTChunks.MCAL:
                            mapchunk.alphaLayer = ReadMCALSubChunk(subChunkSize, subbin, mapchunk);
                            break;
                        case ADTChunks.MCSH:
                        case ADTChunks.MCMT:
                            break;
                        default:
                            throw new Exception(string.Format("Found unknown header at offset {1} \"{0}\" while we should've already read them all!", subChunkName, subpos.ToString()));
                    }
                }
            }

            return mapchunk;
        }
        private MTEX ReadMTEXChunk(uint size, BinaryReader bin)
        {
            var txchunk = new MTEX();

            //List of BLP filenames
            var blpFilesChunk = bin.ReadBytes((int)size);

            var str = new StringBuilder();

            for (var i = 0; i < blpFilesChunk.Length; i++)
            {
                if (blpFilesChunk[i] == '\0')
                {
                    blpFiles.Add(str.ToString());
                    if (!CASC.FileExists(str.ToString()))
                    {
                        Console.WriteLine("BLP file does not exist!!! {0}", str.ToString());
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
        private MTXP[] ReadMTXPChunk(uint size, BinaryReader bin)
        {
            var count = size / 16;

            var txparams = new MTXP[count];

            for(var i = 0; i < count; i++)
            {
                txparams[i] = bin.Read<MTXP>();
            }

            return txparams;
        }
        private MCAL[] ReadMCALSubChunk(uint size, BinaryReader bin, TexMCNK mapchunk)
        {
            var mcal = new MCAL[mapchunk.layers.Length];

            mcal[0].layer = new byte[64 * 64];
            for (var i = 0; i < 64 * 64; i++)
            {
                mcal[0].layer[i] = 255;
            }

            uint read_offset = 0;

            for (var layer = 1; layer < mapchunk.layers.Length; ++layer)
            {
                // we assume that we have read as many bytes as this next layer's mcal offset. we then read depending on encoding
                if (mapchunk.layers[layer].offsetInMCAL != read_offset)
                {
                    throw new Exception("mismatch: layer before required more / less bytes than expected");
                }
                if (mapchunk.layers[layer].flags.HasFlag(mclyFlags.Flag_0x200)) // Compressed
                {
                    //Console.WriteLine("Compressed");
                    // first layer is always fully opaque -> you can let that out
                    // array of 3 x array of 64*64 chars: unpacked alpha values
                    mcal[layer].layer = new byte[64 * 64];

                    // sorry, I have no god damn idea about c#
                    // *x = value at x. x = pointer to data. ++x = advance pointer a byte
                    uint in_offset = 0;
                    uint out_offset = 0;
                    while (out_offset < 4096)
                    {
                        var info = bin.ReadByte(); ++in_offset;
                        var mode = (uint)(info & 0x80) >> 7; // 0 = copy, 1 = fill
                        var count = (uint)(info & 0x7f); // do mode operation count times

                        if (mode != 0)
                        {
                            var val = bin.ReadByte(); ++in_offset;
                            while (count-- > 0 && out_offset < 4096)
                            {
                                mcal[layer].layer[out_offset] = val;
                                ++out_offset;
                            }

                        }
                        else // mode == 1
                        {
                            while (count-- > 0 && out_offset < 4096)
                            {
                                var val = bin.ReadByte(); ++in_offset;
                                mcal[layer].layer[out_offset] = val;
                                ++out_offset;
                            }
                        }
                    }
                    read_offset += in_offset;
                    if (out_offset != 4096) throw new Exception("we somehow overshoot. this should not be the case, except for broken adts");
                }
                else if (wdt.mphd.flags.HasFlag(Structs.WDT.mphdFlags.adt_has_big_alpha) || wdt.mphd.flags.HasFlag(Structs.WDT.mphdFlags.adt_has_height_texturing)) // Uncompressed (4096)
                {
                    //Console.WriteLine("Uncompressed (4096)");
                    mcal[layer].layer = bin.ReadBytes(4096);
                    read_offset += 4096;
                }
                else // Uncompressed (2048)
                {
                    //Console.WriteLine("Uncompressed (2048)");
                    mcal[layer].layer = new byte[64 * 64];
                    var mcal_data = bin.ReadBytes(2048);
                    read_offset += 2048;
                    for (var i = 0; i < 2048; ++i)
                    {
                        // maybe nibbles swapped
                        mcal[layer].layer[2 * i + 0] = (byte)(((mcal_data[i] & 0x0F) >> 0) * 17);
                        mcal[layer].layer[2 * i + 1] = (byte)(((mcal_data[i] & 0xF0) >> 4) * 17);
                    }
                }
            }

            if (read_offset != size) throw new Exception("Haven't finished reading chunk but should be");

            return mcal;
        }
        private MCLY[] ReadMCLYSubChunk(uint size, BinaryReader bin)
        {
            var count = size / 16;
            var mclychunks = new MCLY[count];
            for (var i = 0; i < count; i++)
            {
                mclychunks[i].textureId = bin.ReadUInt32();
                mclychunks[i].flags = (mclyFlags)bin.ReadUInt32();
                mclychunks[i].offsetInMCAL = bin.ReadUInt32();
                mclychunks[i].effectId = bin.ReadInt32();
            }

            return mclychunks;
        }

        private uint[] ReadFileDataIDChunk(uint size, BinaryReader bin)
        {
            var count = size / 4;
            var filedataids = new uint[count];
            for(var i = 0; i < count; i++)
            {
                filedataids[i] = bin.ReadUInt32();
            }
            return filedataids;
        }
    }
}
