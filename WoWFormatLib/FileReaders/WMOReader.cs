/*
        DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE 
                    Version 2, December 2004 

 Copyright (C) 2004 Sam Hocevar <sam@hocevar.net> 

 Everyone is permitted to copy and distribute verbatim or modified 
 copies of this license document, and changing it is allowed as long 
 as the name is changed. 

            DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE 
   TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION 

  0. You just DO WHAT THE FUCK YOU WANT TO.
*/
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
        public WMO wmofile;
        private bool _lod;

        public void LoadWMO(string filename, bool lod = false)
        {
            _lod = lod;

            if (CASC.cascHandler.FileExists(filename))
            {
                using (var wmoStream = CASC.cascHandler.OpenFile(filename))
                {
                    ReadWMO(filename, wmoStream);
                }
            }
            else
            {
                throw new FileNotFoundException("File " + filename + " was not found");
            }
        }

        /* PARENT */
        private void ReadWMO(string filename, Stream wmo)
        {
            using (var bin = new BinaryReader(wmo))
            {
                long position = 0;
                while (position < wmo.Length)
                {
                    wmo.Position = position;

                    var chunkName = new string(bin.ReadChars(4).Reverse().ToArray());
                    var chunkSize = bin.ReadUInt32();

                    position = wmo.Position + chunkSize;

                    switch (chunkName)
                    {
                        case "MVER":
                            wmofile.version = bin.Read<MVER>();
                            if (wmofile.version.version != 17)
                            {
                                throw new Exception("Unsupported WMO version! (" + wmofile.version.version + ") (" + filename + ")");
                            }
                            break;
                        case "MOHD":
                            wmofile.header = ReadMOHDChunk(bin, filename);
                            break;
                        case "MOTX":
                            wmofile.textures = ReadMOTXChunk(chunkSize, bin);
                            break;
                        case "MOMT":
                            wmofile.materials = ReadMOMTChunk(bin, wmofile.header.nMaterials);
                            break;
                        case "MOGN":
                            wmofile.groupNames = ReadMOGNChunk(chunkSize, bin, wmofile.header.nGroups);
                            break;
                        case "MOGI":
                            wmofile.groupInfo = ReadMOGIChunk(bin, wmofile.header.nGroups);
                            break;
                        case "MODS":
                            wmofile.doodadSets = ReadMODSChunk(chunkSize, bin);
                            break;
                        case "MODN":
                            wmofile.doodadNames = ReadMODNChunk(chunkSize, bin, wmofile.header.nModels);
                            break;
                        case "MODD":
                            wmofile.doodadDefinitions = ReadMODDChunk(chunkSize, bin);
                            break;
                        case "MOSB": // Skybox
                            wmofile.skybox = ReadMOSBChunk(chunkSize, bin);
                            break;
                        case "MOPV": // Portal Vertices
                        case "MOPR": // Portal References
                        case "MOPT": // Portal Information
                        case "MOVV": // Visible block vertices
                        case "MOVB": // Visible block list
                        case "MOLT": // Lighting Infroamtion
                        case "MFOG": // Fog Information
                        case "MCVP": // Convex Volume Planes
                        case "MOUV": // 7.3 - ?
                        case "GFID": // Legion
                            break;
                        default:
                            throw new Exception(string.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunkName, position.ToString(), filename));
                    }
                }
            }

            var groupFiles = new WMOGroupFile[wmofile.header.nGroups];
            for (var i = 0; i < wmofile.header.nGroups; i++)
            {
                var groupfilename = filename.ToLower().Replace(".wmo", "_" + i.ToString().PadLeft(3, '0') + ".wmo");

                if (_lod)
                {
                    if (CASC.cascHandler.FileExists(groupfilename.Replace(".wmo", "_lod2.wmo")))
                    {
                        groupfilename = groupfilename.Replace(".wmo", "_lod2.wmo");
                        Console.WriteLine("[LOD] Loading LOD 2 for group " + i);
                    }
                    else if (CASC.cascHandler.FileExists(groupfilename.Replace(".wmo", "_lod1.wmo")))
                    {
                        groupfilename = groupfilename.Replace(".wmo", "_lod1.wmo");
                        Console.WriteLine("[LOD] Loading LOD 1 for group " + i);
                    }
                    else
                    {
                        Console.WriteLine("[LOD] No LOD " + i);
                    }
                }

                if (CASC.cascHandler.FileExists(groupfilename))
                {
                    using (var wmoStream = CASC.cascHandler.OpenFile(groupfilename))
                    {
                        groupFiles[i] = ReadWMOGroupFile(groupfilename, wmoStream);
                    }
                }
            }

            wmofile.group = groupFiles;
        }
        private MOHD ReadMOHDChunk(BinaryReader bin, string filename)
        {
            //Header for the map object. 64 bytes.
            var header = new MOHD()
            {
                nMaterials = bin.ReadUInt32(),
                nGroups = bin.ReadUInt32(),
                nPortals = bin.ReadUInt32(),
                nLights = bin.ReadUInt32(),
                nModels = bin.ReadUInt32(),
                nDoodads = bin.ReadUInt32(),
                nSets = bin.ReadUInt32(),
                ambientColor = bin.ReadUInt32(),
                wmoID = bin.ReadUInt32()
            };

            return header;
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
        private MOMT[] ReadMOMTChunk(BinaryReader bin, uint num)
        {
            var materials = new MOMT[num];
            for (var i = 0; i < num; i++)
            {
                materials[i] = bin.Read<MOMT>();
            }
            return materials;
        }
        private MOGN[] ReadMOGNChunk(uint size, BinaryReader bin, uint num)
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

            if (nameList.Count != num)
            {
                //throw new Exception("List of group names does not equal number of groups");
            }
            var groupNames = new MOGN[nameList.Count];
            for (var i = 0; i < nameList.Count; i++)
            {
                groupNames[i].name = nameList[i];
                groupNames[i].offset = nameOffset[i];
            }
            return groupNames;
        }
        private MOGI[] ReadMOGIChunk(BinaryReader bin, uint num)
        {
            var groupInfo = new MOGI[num];
            for (var i = 0; i < num; i++)
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
        private MODN[] ReadMODNChunk(uint size, BinaryReader bin, uint num)
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
            if (num != m2Files.Count) { throw new Exception("nModels does not match doodad count"); }

            var doodadNames = new MODN[num];
            for (var i = 0; i < num; i++)
            {
                doodadNames[i].filename = m2Files[i];
                doodadNames[i].startOffset = (uint)m2Offset[i];
            }
            return doodadNames;
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
            return bin.ReadStringNull();
        }

        /* GROUP */
        private WMOGroupFile ReadWMOGroupFile(string filename, Stream wmo)
        {
            var groupFile = new WMOGroupFile();

            using (var bin = new BinaryReader(wmo))
            {
                long position = 0;
                while (position < wmo.Length)
                {
                    wmo.Position = position;
                    var chunkName = new string(bin.ReadChars(4).Reverse().ToArray());
                    var chunkSize = bin.ReadUInt32();
                    position = wmo.Position + chunkSize;

                    switch (chunkName)
                    {
                        case "MVER":
                            groupFile.version = bin.Read<MVER>();
                            if (wmofile.version.version != 17)
                            {
                                throw new Exception("Unsupported WMO version! (" + wmofile.version.version + ")");
                            }
                            continue;
                        case "MOGP":
                            groupFile.mogp = ReadMOGPChunk(chunkSize, bin);
                            continue;
                        default:
                            throw new Exception(string.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunkName, position.ToString(), filename));
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

                    var subChunkName = new string(subbin.ReadChars(4).Reverse().ToArray());
                    var subChunkSize = subbin.ReadUInt32();

                    position = stream.Position + subChunkSize;

                    switch (subChunkName)
                    {
                        case "MOVI": //Vertex indices for triangles
                            mogp.indices = ReadMOVIChunk(subChunkSize, subbin);
                            //Console.WriteLine("Read " + mogp.indices.Length + " indices!");
                            break;

                        case "MOVT": //Vertices chunk
                            mogp.vertices = ReadMOVTChunk(subChunkSize, subbin);
                            break;

                        case "MOTV": //Texture coordinates
                            mogp.textureCoords[MOTVi++] = ReadMOTVChunk(subChunkSize, subbin);
                            break;

                        case "MONR": //Normals
                            mogp.normals = ReadMONRChunk(subChunkSize, subbin);
                            break;

                        case "MOBA": //Render batches
                            mogp.renderBatches = ReadMOBAChunk(subChunkSize, subbin);
                            break;

                        case "MOPY": //Material info for triangles, two bytes per triangle.
                            mogp.materialInfo = ReadMOPYChunk(subChunkSize, subbin);
                            break;

                        case "MOBS": //Unk
                        case "MODR": //Doodad references
                        case "MOBN": //Array of t_BSP_NODE
                        case "MOBR": //Face indices
                        case "MOLR": //Light references
                        case "MOCV": //Vertex colors
                        case "MDAL": //Unk (new in WoD?)
                        case "MLIQ": //Liquids
                        case "MOTA": //Unknown
                        case "MOPL": //Unknown
                        case "MOLP": //Unknown
                        case "MOLS": //Unknown
                            continue;
                        default:
                            throw new Exception(string.Format("Found unknown header at offset {1} \"{0}\" while we should've already read them all!", subChunkName, position.ToString()));
                    }
                }
            }
            
            return mogp;
        }
        private MONR[] ReadMONRChunk(uint size, BinaryReader bin)
        {
            var numNormals = size / (sizeof(float) * 3);
            var normals = new MONR[numNormals];
            for (var i = 0; i < numNormals; i++)
            {
                normals[i].normal = bin.Read<Vector3>();
            }
            return normals;
        }
        private MOVT[] ReadMOVTChunk(uint size, BinaryReader bin)
        {
            var numVerts = size / (sizeof(float) * 3);
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
            var numCoords = size / (sizeof(float) * 2);
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
            var numIndices = size / sizeof(ushort);
            var indices = new MOVI[numIndices];
            for (var i = 0; i < numIndices; i++)
            {
                indices[i].indice = bin.ReadUInt16();
            }
            return indices;
        }
    }
}
