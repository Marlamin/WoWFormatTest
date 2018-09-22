using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WoWFormatLib.Structs.WMO;
using WoWFormatLib.Utils;

namespace WoWFormatLib.FileReaders
{
    public class WMOReader
    {
        private WMO wmofile;
        private byte lodLevel;

        public WMO LoadWMO(Stream wmo, byte lod = 0)
        {
            lodLevel = lod;

            ReadWMO(wmo);
            return wmofile;
        }

        public WMO LoadWMO(int filedataid, byte lod = 0)
        {
            lodLevel = lod;

            if (CASC.cascHandler.FileExists(filedataid))
            {
                using (var wmoStream = CASC.cascHandler.OpenFile(filedataid))
                {
                    ReadWMO(wmoStream);
                }
            }
            else
            {
                throw new FileNotFoundException("File " + filedataid + " was not found");
            }

            return wmofile;
        }

        public WMO LoadWMO(string filename, byte lod = 0)
        {
            lodLevel = lod;

            if (CASC.cascHandler.FileExists(filename))
            {
                using (var wmoStream = CASC.cascHandler.OpenFile(filename))
                {
                    ReadWMO(wmoStream);
                }
            }
            else
            {
                throw new FileNotFoundException("File " + filename + " was not found");
            }

            return wmofile;
        }

        /* PARENT */
        private void ReadWMO(Stream wmo)
        {
            using (var bin = new BinaryReader(wmo))
            {
                long position = 0;
                while (position < wmo.Length)
                {
                    wmo.Position = position;

                    var chunkName = (WMOChunks)bin.ReadUInt32();
                    var chunkSize = bin.ReadUInt32();

                    position = wmo.Position + chunkSize;

                    switch (chunkName)
                    {
                        case WMOChunks.MVER:
                            wmofile.version = bin.Read<MVER>();
                            if (wmofile.version.version != 17)
                            {
                                throw new Exception("Unsupported WMO version! (" + wmofile.version.version + ")");
                            }
                            break;
                        case WMOChunks.MOHD:
                            wmofile.header = bin.Read<MOHD>();
                            break;
                        case WMOChunks.MOTX:
                            wmofile.textures = ReadMOTXChunk(chunkSize, bin);
                            break;
                        case WMOChunks.MOMT:
                            wmofile.materials = ReadMOMTChunk(chunkSize, bin);
                            break;
                        case WMOChunks.MOGN:
                            wmofile.groupNames = ReadMOGNChunk(chunkSize, bin);
                            break;
                        case WMOChunks.MOGI:
                            wmofile.groupInfo = ReadMOGIChunk(chunkSize, bin);
                            break;
                        case WMOChunks.MODS:
                            wmofile.doodadSets = ReadMODSChunk(chunkSize, bin);
                            break;
                        case WMOChunks.MODI:
                            wmofile.doodadIds = ReadMODIChunk(chunkSize, bin);
                            break;
                        case WMOChunks.MODN:
                            wmofile.doodadNames = ReadMODNChunk(chunkSize, bin);
                            break;
                        case WMOChunks.MODD:
                            wmofile.doodadDefinitions = ReadMODDChunk(chunkSize, bin);
                            break;
                        case WMOChunks.MOSB:
                            wmofile.skybox = ReadMOSBChunk(chunkSize, bin);
                            break;
                        case WMOChunks.GFID:
                            wmofile.groupFileDataIDs = ReadGFIDChunk(chunkSize, bin);
                            break;
                        case WMOChunks.MOPV: // MOPV Portal Vertices
                        case WMOChunks.MOPR: // MOPR Portal References
                        case WMOChunks.MOPT: // MOPT Portal Information
                        case WMOChunks.MOVV: // MOVV Visible block vertices
                        case WMOChunks.MOVB: // MOVB Visible block list
                        case WMOChunks.MOLT: // MOLT Lighting Information
                        case WMOChunks.MFOG: // MFOG Fog Information
                        case WMOChunks.MCVP: // MCVP Convex Volume Planes
                        case WMOChunks.MOUV: // MOUV Animated texture UVs
                        case WMOChunks.MLSP: // ?
                            break;
                        default:
                            throw new Exception(string.Format("Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunkName.ToString("X"), position.ToString()));
                    }
                }
            }

            var groupFiles = new WMOGroupFile[wmofile.header.nGroups];

            if(wmofile.header.nLod != 0 && (lodLevel + 1) > wmofile.header.nLod)
            {
                throw new Exception("Requested LOD (" + lodLevel + ") exceeds the max LOD for this WMO (" + (wmofile.header.nLod - 1) + ")");
            }

            var start = wmofile.header.nGroups * lodLevel;

            for (var i = 0; i < wmofile.header.nGroups; i++)
            {
                var groupFileDataID = wmofile.groupFileDataIDs[start + i];

                if (lodLevel == 3 && groupFileDataID == 0) // if lod is 3 and there's no lod3 available, fall back to lod1
                {
                    groupFileDataID = wmofile.groupFileDataIDs[i + (wmofile.header.nGroups * 2)];
                }

                if (lodLevel >= 2 && groupFileDataID == 0) // if lod is 2 or higher and there's no lod2 available, fall back to lod1
                {
                    groupFileDataID = wmofile.groupFileDataIDs[i + (wmofile.header.nGroups * 1)];
                }

                if (lodLevel >= 1 && groupFileDataID == 0) // if lod is 1 or higher check if lod1 available, fall back to lod0
                {
                    groupFileDataID = wmofile.groupFileDataIDs[i];
                }

                if (CASC.IsCASCInit && CASC.cascHandler.FileExists(groupFileDataID))
                {
                    using (var wmoStream = CASC.cascHandler.OpenFile(groupFileDataID))
                    {
                        groupFiles[i] = ReadWMOGroupFile(groupFileDataID, wmoStream);
                    }
                }
            }

            wmofile.group = groupFiles;
        }

        private int[] ReadGFIDChunk(uint size, BinaryReader bin)
        {
            var count = size / 4;
            var gfids = new int[count];
            for (var i = 0; i < count; i++)
            {
                gfids[i] = bin.ReadInt32();
            }
            return gfids;
        }

        private MOTX[] ReadMOTXChunk(uint size, BinaryReader bin)
        {
            //List of BLP filenames
            var blpFilesChunk = bin.ReadBytes((int)size);
            var blpFiles = new List<string>();
            var blpOffset = new List<int>();
            var str = new StringBuilder();

            var buildingString = false;
            for (var i = 0; i < blpFilesChunk.Length; i++)
            {
                if (blpFilesChunk[i] == '\0')
                {
                    if (buildingString)
                    {
                        str.Replace("..", ".");
                        blpFiles.Add(str.ToString());
                        blpOffset.Add(i - str.ToString().Length);
                    }
                    buildingString = false;
                    str = new StringBuilder();
                }
                else
                {
                    buildingString = true;
                    str.Append((char)blpFilesChunk[i]);
                }
            }

            var textures = new MOTX[blpFiles.Count];
            for (var i = 0; i < blpFiles.Count; i++)
            {
                textures[i].filename = blpFiles[i];
                textures[i].startOffset = (uint)blpOffset[i];
            }

            return textures;
        }
        private MOMT[] ReadMOMTChunk(uint size, BinaryReader bin)
        {
            var count = size / 64;
            var materials = new MOMT[count];
            for (var i = 0; i < count; i++)
            {
                materials[i] = bin.Read<MOMT>();
            }
            return materials;
        }
        private MOGN[] ReadMOGNChunk(uint size, BinaryReader bin)
        {
            var wmoGroupsChunk = bin.ReadBytes((int)size);
            var str = new StringBuilder();
            var nameList = new List<string>();
            var nameOffset = new List<int>();
            for (var i = 0; i < wmoGroupsChunk.Length; i++)
            {
                if (wmoGroupsChunk[i] == '\0')
                {
                    if (str.Length > 1)
                    {
                        nameOffset.Add(i - str.ToString().Length);
                        nameList.Add(str.ToString());
                    }
                    str = new StringBuilder();
                }
                else
                {
                    str.Append((char)wmoGroupsChunk[i]);
                }
            }

            var groupNames = new MOGN[nameList.Count];
            for (var i = 0; i < nameList.Count; i++)
            {
                groupNames[i].name = nameList[i];
                groupNames[i].offset = nameOffset[i];
            }
            return groupNames;
        }
        private MOGI[] ReadMOGIChunk(uint size, BinaryReader bin)
        {
            var count = size / 32;
            var groupInfo = new MOGI[count];
            for (var i = 0; i < count; i++)
            {
                groupInfo[i] = bin.Read<MOGI>();
            }
            return groupInfo;
        }
        private MODS[] ReadMODSChunk(uint size, BinaryReader bin)
        {
            var numDoodadSets = size / 32;
            var doodadSets = new MODS[numDoodadSets];
            for (var i = 0; i < numDoodadSets; i++)
            {
                doodadSets[i].setName = new string(bin.ReadChars(20)).Replace("\0", string.Empty);
                doodadSets[i].firstInstanceIndex = bin.ReadUInt32();
                doodadSets[i].numDoodads = bin.ReadUInt32();
                doodadSets[i].unused = bin.ReadUInt32();
            }
            return doodadSets;
        }
        private MODN[] ReadMODNChunk(uint size, BinaryReader bin)
        {
            //List of M2 filenames, but are still named after MDXs internally. Have to rename!
            var m2FilesChunk = bin.ReadBytes((int)size);
            var m2Files = new List<string>();
            var m2Offset = new List<int>();
            var str = new StringBuilder();

            for (var i = 0; i < m2FilesChunk.Length; i++)
            {
                if (m2FilesChunk[i] == '\0')
                {
                    if (str.Length > 1)
                    {
                        str.Replace("..", ".");
                        str.Replace(".mdx", ".m2");

                        m2Files.Add(str.ToString());
                        m2Offset.Add(i - str.ToString().Length);
                    }
                    str = new StringBuilder();
                }
                else
                {
                    str.Append((char)m2FilesChunk[i]);
                }
            }

            var num = m2Files.Count();

            var doodadNames = new MODN[num];
            for (var i = 0; i < num; i++)
            {
                doodadNames[i].filename = m2Files[i];
                doodadNames[i].startOffset = (uint)m2Offset[i];
            }
            return doodadNames;
        }
        private uint[] ReadMODIChunk(uint size, BinaryReader bin)
        {
            var numIds = size / 4;
            var fileDataIDs = new uint[numIds];
            for(var i = 0; i < numIds; i++)
            {
                fileDataIDs[i] = bin.ReadUInt32();
            }
            return fileDataIDs;
        }
        private MODD[] ReadMODDChunk(uint size, BinaryReader bin)
        {
            var numDoodads = size / 40;
            var doodads = new MODD[numDoodads];
            for (var i = 0; i < numDoodads; i++)
            {
                var raw_offset = bin.ReadBytes(3);
                doodads[i].offset = (uint)(raw_offset[0] | raw_offset[1] << 8 | raw_offset[2] << 16);
                doodads[i].flags = bin.ReadByte();
                doodads[i].position = bin.Read<Vector3>();
                doodads[i].rotation = bin.Read<Quaternion>();
                doodads[i].scale = bin.ReadSingle();
                doodads[i].color = bin.ReadBytes(4);
            }
            return doodads;
        }

        private string ReadMOSBChunk(uint size, BinaryReader bin)
        {
            return bin.ReadCString();
        }

        /* GROUP */
        private WMOGroupFile ReadWMOGroupFile(int filedataid, Stream wmo)
        {
            var groupFile = new WMOGroupFile();

            using (var bin = new BinaryReader(wmo))
            {
                long position = 0;
                while (position < wmo.Length)
                {
                    wmo.Position = position;
                    var chunkName = (WMOChunks)bin.ReadUInt32();
                    var chunkSize = bin.ReadUInt32();
                    position = wmo.Position + chunkSize;

                    switch (chunkName)
                    {
                        case WMOChunks.MVER:
                            groupFile.version = bin.Read<MVER>();
                            if (wmofile.version.version != 17)
                            {
                                throw new Exception("Unsupported WMO version! (" + wmofile.version.version + ")");
                            }
                            continue;
                        case WMOChunks.MOGP:
                            groupFile.mogp = ReadMOGPChunk(chunkSize, bin);
                            continue;
                        default:
                            throw new Exception(string.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunkName.ToString("X"), position.ToString(), filedataid));
                    }
                }
            }
            
            return groupFile;
        }
        private MOGP ReadMOGPChunk(uint size, BinaryReader bin)
        {
            var mogp = new MOGP()
            {
                nameOffset = bin.ReadUInt32(),
                descriptiveNameOffset = bin.ReadUInt32(),
                flags = (MOGPFlags)bin.ReadUInt32(),
                boundingBox1 = bin.Read<Vector3>(),
                boundingBox2 = bin.Read<Vector3>(),
                ofsPortals = bin.ReadUInt16(),
                numPortals = bin.ReadUInt16(),
                numBatchesA = bin.ReadUInt16(),
                numBatchesB = bin.ReadUInt16(),
                numBatchesC = bin.ReadUInt32(),
                unused = bin.ReadUInt32(),
                liquidType = bin.ReadUInt32(),
                groupID = bin.ReadUInt32(),
                unk0 = bin.ReadUInt32(),
                unk1 = bin.ReadUInt32()
            };

            using (var stream = new MemoryStream(bin.ReadBytes((int)size)))
            using (var subbin = new BinaryReader(stream))
            {
                long position = 0;
                var MOTVi = 0;

                if (mogp.flags.HasFlag(MOGPFlags.Flag_0x40000000))
                {
                    mogp.textureCoords = new MOTV[3][];
                }
                else
                {
                    mogp.textureCoords = new MOTV[2][];
                }

                while (position < stream.Length)
                {
                    stream.Position = position;

                    var subChunkName = (WMOChunks)subbin.ReadUInt32();
                    var subChunkSize = subbin.ReadUInt32();

                    position = stream.Position + subChunkSize;

                    switch (subChunkName)
                    {
                        case WMOChunks.MOVI: // MOVI Vertex indices for triangles
                            mogp.indices = ReadMOVIChunk(subChunkSize, subbin);
                            break;
                        case WMOChunks.MOVT: // MOVT Vertices chunk
                            mogp.vertices = ReadMOVTChunk(subChunkSize, subbin);
                            break;
                        case WMOChunks.MOTV: // MOTV Texture coordinates
                            mogp.textureCoords[MOTVi++] = ReadMOTVChunk(subChunkSize, subbin);
                            break;
                        case WMOChunks.MONR: // MONR Normals
                            mogp.normals = ReadMONRChunk(subChunkSize, subbin);
                            break;
                        case WMOChunks.MOBA: // MOBA Render batches
                            mogp.renderBatches = ReadMOBAChunk(subChunkSize, subbin);
                            break;
                        case WMOChunks.MOPY: // MOPY Material info for triangles, two bytes per triangle.
                            mogp.materialInfo = ReadMOPYChunk(subChunkSize, subbin);
                            break;
                        case WMOChunks.MOBS: // MOBS Unk
                        case WMOChunks.MODR: // MODR Doodad references
                        case WMOChunks.MOBN: // MOBN Array of t_BSP_NODE
                        case WMOChunks.MOBR: // MOBR Face indices
                        case WMOChunks.MOLR: // MOLR Light references
                        case WMOChunks.MOCV: // MOCV Vertex colors
                        case WMOChunks.MDAL: // MDAL Unk (new in WoD)
                        case WMOChunks.MLIQ: // MLIQ Liquids
                        case WMOChunks.MOTA: // MOTA Tangent Array
                        case WMOChunks.MOPL: // MOPL Terrain Cutting PLanes
                        case WMOChunks.MOLP: // MOLP Points Lights
                        case WMOChunks.MOLS: // MOLS Spot Lights
                        case WMOChunks.MOPB: // MOPB Prepass Batches
                        case WMOChunks.MLSP: // ?
                            continue;
                        default:
                            throw new Exception(string.Format("Found unknown header at offset {1} \"{0}\" while we should've already read them all!", subChunkName.ToString("X"), position.ToString()));
                    }
                }
            }
            
            return mogp;
        }
        private MONR[] ReadMONRChunk(uint size, BinaryReader bin)
        {
            var numNormals = size / 12;
            var normals = new MONR[numNormals];
            for (var i = 0; i < numNormals; i++)
            {
                normals[i].normal = bin.Read<Vector3>();
            }
            return normals;
        }
        private MOVT[] ReadMOVTChunk(uint size, BinaryReader bin)
        {
            var numVerts = size / 12;
            var vertices = new MOVT[numVerts];
            for (var i = 0; i < numVerts; i++)
            {
                vertices[i].vector = bin.Read<Vector3>();
            }
            return vertices;
        }
        private MOBA[] ReadMOBAChunk(uint size, BinaryReader bin)
        {
            var numBatches = size / 24;
            var batches = new MOBA[numBatches];
            for (var i = 0; i < numBatches; i++)
            {
                batches[i] = bin.Read<MOBA>();
            }
            return batches;
        }
        private MOPY[] ReadMOPYChunk(uint size, BinaryReader bin)
        {
            var numMaterials = size / 2;
            var materials = new MOPY[numMaterials];
            for (var i = 0; i < numMaterials; i++)
            {
                materials[i] = bin.Read<MOPY>();
            }
            return materials;
        }
        private MOTV[] ReadMOTVChunk(uint size, BinaryReader bin)
        {
            var numCoords = size / 8;
            var textureCoords = new MOTV[numCoords];
            for (var i = 0; i < numCoords; i++)
            {
                textureCoords[i].X = bin.ReadSingle();
                textureCoords[i].Y = bin.ReadSingle();
            }
            return textureCoords;
        }
        private MOVI[] ReadMOVIChunk(uint size, BinaryReader bin)
        {
            var numIndices = size / 2;
            var indices = new MOVI[numIndices];
            for (var i = 0; i < numIndices; i++)
            {
                indices[i].indice = bin.ReadUInt16();
            }
            return indices;
        }
    }
}
