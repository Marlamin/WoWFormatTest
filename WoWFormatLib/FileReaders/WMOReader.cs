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
using System.Text;
using WoWFormatLib.Structs.WMO;
using WoWFormatLib.Utils;

namespace WoWFormatLib.FileReaders
{
    public class WMOReader
    {
        public WMO wmofile;
        private bool _lod;

        public WMOReader()
        {
        }

        public void LoadWMO(string filename, bool lod = false)
        {
            _lod = lod;

            if (!CASC.FileExists(filename))
            {
                new WoWFormatLib.Utils.MissingFile(filename);
                return;
            }
            else
            {
                using (Stream wmoStream = CASC.OpenFile(filename))
                {
                    ReadWMO(filename, wmoStream);
                }
            }
        }

        public MODN[] ReadMODNChunk(BlizzHeader chunk, BinaryReader bin, uint num)
        {
            //List of M2 filenames, but are still named after MDXs internally. Have to rename!
            var m2FilesChunk = bin.ReadBytes((int)chunk.Size);
            List<String> m2Files = new List<string>();
            List<int> m2Offset = new List<int>();
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
            if (num != m2Files.Count) { throw new Exception("nModels does not match doodad count");  }

            var doodadNames = new MODN[num];
            for (var i = 0; i < num; i++)
            {
                doodadNames[i].filename = m2Files[i];
                doodadNames[i].startOffset = (uint)m2Offset[i];
            }
            return doodadNames;
        }

        public MOGI[] ReadMOGIChunk(BlizzHeader chunk, BinaryReader bin, uint num)
        {
            var groupInfo = new MOGI[num];
            for (var i = 0; i < num; i++)
            {
                groupInfo[i] = bin.Read<MOGI>();
            }
            return groupInfo;
        }

        public MOGN[] ReadMOGNChunk(BlizzHeader chunk, BinaryReader bin, uint num)
        {
            var wmoGroupsChunk = bin.ReadBytes((int)chunk.Size);
            var str = new StringBuilder();
            var nameList = new List<String>();
            List<int> nameOffset = new List<int>();
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

        public MOGP ReadMOGPChunk(BlizzHeader chunk, BinaryReader bin)
        {
            MOGP mogp = new MOGP();
            mogp.nameOffset = bin.ReadUInt32();
            mogp.descriptiveNameOffset = bin.ReadUInt32();
            mogp.flags = (MOGPFlags) bin.ReadUInt32();
            mogp.boundingBox1 = bin.Read<Vector3>();
            mogp.boundingBox2 = bin.Read<Vector3>();
            mogp.ofsPortals = bin.ReadUInt16();
            mogp.numPortals = bin.ReadUInt16();
            mogp.numBatchesA = bin.ReadUInt16();
            mogp.numBatchesB = bin.ReadUInt16();
            mogp.numBatchesC = bin.ReadUInt32();
            //mogp.fogIndices = bin.ReadBytes(4);
            bin.ReadBytes(4);
            mogp.liquidType = bin.ReadUInt32();
            mogp.groupID = bin.ReadUInt32();
            mogp.unk0 = bin.ReadUInt32();
            mogp.unk1 = bin.ReadUInt32();
            MemoryStream stream = new MemoryStream(bin.ReadBytes((int)chunk.Size));
            var subbin = new BinaryReader(stream);
            BlizzHeader subchunk;
            long position = 0;
            int MOTVi = 0;

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
                subchunk = new BlizzHeader(subbin.ReadChars(4), subbin.ReadUInt32());
                subchunk.Flip();
                position = stream.Position + subchunk.Size;
                //Console.WriteLine(subchunk.ToString());
                switch (subchunk.ToString())
                {
                    case "MOVI": //Vertex indices for triangles
                        mogp.indices = ReadMOVIChunk(subchunk, subbin);
                        //Console.WriteLine("Read " + mogp.indices.Length + " indices!");
                        break;

                    case "MOVT": //Vertices chunk
                        mogp.vertices = ReadMOVTChunk(subchunk, subbin);
                        break;

                    case "MOTV": //Texture coordinates
                        mogp.textureCoords[MOTVi++] = ReadMOTVChunk(subchunk, subbin);
                        break;

                    case "MONR": //Normals
                        mogp.normals = ReadMONRChunk(subchunk, subbin);
                        break;

                    case "MOBA": //Render batches
                        mogp.renderBatches = ReadMOBAChunk(subchunk, subbin);
                        break;

                    case "MOPY": //Material info for triangles, two bytes per triangle.
                        mogp.materialInfo = ReadMOPYChunk(subchunk, subbin);
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
                        throw new Exception(String.Format("Found unknown header at offset {1} \"{0}\" while we should've already read them all!", subchunk.ToString(), position.ToString()));
                }
            }
            //if(MOTVi == 0) { throw new Exception("Didn't parse any MOTV??");  } // antiportal groups have no motv
            return mogp;
        }

        public MOHD ReadMOHDChunk(BlizzHeader chunk, BinaryReader bin, string filename)
        {
            //Header for the map object. 64 bytes.
            // var MOHDChunk = bin.ReadBytes((int)chunk.Size);
            var header = new MOHD();
            header.nMaterials = bin.ReadUInt32();
            header.nGroups = bin.ReadUInt32();
            header.nPortals = bin.ReadUInt32();
            header.nLights = bin.ReadUInt32();
            header.nModels = bin.ReadUInt32();

            //Console.WriteLine("         " + nGroups.ToString() + " group(s)");

            return header;
        }

        public MONR[] ReadMONRChunk(BlizzHeader chunk, BinaryReader bin)
        {
            var numNormals = chunk.Size / (sizeof(float) * 3);
            //Console.WriteLine(numNormals + " normals!");
            var normals = new MONR[numNormals];
            for (var i = 0; i < numNormals; i++)
            {
                normals[i].normal = bin.Read<Vector3>();
            }
            return normals;
        }

        public MOTX[] ReadMOTXChunk(BlizzHeader chunk, BinaryReader bin)
        {
            //List of BLP filenames
            var blpFilesChunk = bin.ReadBytes((int)chunk.Size);
            List<String> blpFiles = new List<string>();
            List<int> blpOffset = new List<int>();
            var str = new StringBuilder();

            for (var i = 0; i < blpFilesChunk.Length; i++)
            {
                if (blpFilesChunk[i] == '\0')
                {
                    str.Replace("..", ".");
                    blpFiles.Add(str.ToString());
                    blpOffset.Add(i - str.ToString().Length);
                    if (!CASC.FileExists(str.ToString()))
                    {
                        new WoWFormatLib.Utils.MissingFile(str.ToString());
                    }
                   
                    str = new StringBuilder();
                }
                else
                {
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

        public MOVT[] ReadMOVTChunk(BlizzHeader chunk, BinaryReader bin)
        {
            var numVerts = chunk.Size / (sizeof(float) * 3);
            //Console.WriteLine(numVerts + " vertices!");
            var vertices = new MOVT[numVerts];
            for (var i = 0; i < numVerts; i++)
            {
                vertices[i].vector = bin.Read<Vector3>();
            }
            return vertices;
        }

        private MOBA[] ReadMOBAChunk(BlizzHeader subchunk, BinaryReader subbin)
        {
            var numBatches = subchunk.Size / 24; //24 bytes per MOBA
            //Console.WriteLine(numBatches + " batches!");
            var batches = new MOBA[numBatches];
            for (var i = 0; i < numBatches; i++)
            {
                batches[i] = subbin.Read<MOBA>();
            }
            return batches;
        }

        private MOMT[] ReadMOMTChunk(BlizzHeader chunk, BinaryReader bin, uint num)
        {
            var materials = new MOMT[num];
            //Console.WriteLine(num + " materials!");
            for (var i = 0; i < num; i++)
            {
                materials[i] = bin.Read<MOMT>();
                bin.ReadBytes(16);
            }
            return materials;
        }

        private MOPY[] ReadMOPYChunk(BlizzHeader subchunk, BinaryReader subbin)
        {
            var numMaterials = subchunk.Size / 2;
            //Console.WriteLine(numMaterials + " material infos!");
            var materials = new MOPY[numMaterials];
            for (var i = 0; i < numMaterials; i++)
            {
                materials[i] = subbin.Read<MOPY>();
            }
            return materials;
        }

        private MOTV[] ReadMOTVChunk(BlizzHeader subchunk, BinaryReader subbin)
        {
            var numCoords = subchunk.Size / (sizeof(float) * 2);
            //Console.WriteLine(numCoords + " texturecords!");
            var textureCoords = new MOTV[numCoords];
            for (var i = 0; i < numCoords; i++)
            {
                textureCoords[i].X = subbin.ReadSingle();
                textureCoords[i].Y = subbin.ReadSingle();
            }
            return textureCoords;
        }

        private MOVI[] ReadMOVIChunk(BlizzHeader chunk, BinaryReader bin)
        {
            var numIndices = chunk.Size / sizeof(ushort);
            //Console.WriteLine(numIndices + " indices!");
            var indices = new MOVI[numIndices];
            for (var i = 0; i < numIndices; i++)
            {
                indices[i].indice = bin.ReadUInt16();
            }
            return indices;
        }

        private MODD[] ReadMODDChunk(BlizzHeader chunk, BinaryReader bin)
        {
            var numDoodads = chunk.Size / 40;
            var doodads = new MODD[numDoodads];
            for (var i = 0; i < numDoodads; i++)
            {
                var raw_offset = bin.ReadBytes(3);
                doodads[i].offset = (uint) (raw_offset[0] | raw_offset[1] << 8 | raw_offset[2] << 16);
                doodads[i].flags = bin.ReadByte();
                doodads[i].position = bin.Read<Vector3>();
                doodads[i].rotation = bin.Read<Quaternion>();
                doodads[i].scale = bin.ReadSingle();
                doodads[i].color = bin.ReadBytes(4);
            }
            return doodads;
        }

        private object ReadMOVVChunk(BlizzHeader chunk, BinaryReader bin)
        {
            throw new NotImplementedException();
        }

        private void ReadWMO(string filename, Stream wmo)
        {
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
                        wmofile.version = bin.Read<MVER>();
                        if (wmofile.version.version != 17)
                        {
                            throw new Exception("Unsupported WMO version! (" + wmofile.version.version + ") (" + filename + ")");
                        }
                        continue;
                    case "MOTX":
                        wmofile.textures = ReadMOTXChunk(chunk, bin);
                        continue;
                    case "MOHD":
                        wmofile.header = ReadMOHDChunk(chunk, bin, filename);
                        continue;
                    case "MOGN":
                        wmofile.groupNames = ReadMOGNChunk(chunk, bin, wmofile.header.nGroups);
                        continue;
                    case "MOGI":
                        wmofile.groupInfo = ReadMOGIChunk(chunk, bin, wmofile.header.nGroups);
                        continue;
                    case "MOMT":
                        wmofile.materials = ReadMOMTChunk(chunk, bin, wmofile.header.nMaterials);
                        continue;
                    case "MODN":
                        wmofile.doodadNames = ReadMODNChunk(chunk, bin, wmofile.header.nModels);
                        continue;
                    case "MODD":
                        wmofile.doodadDefinitions = ReadMODDChunk(chunk, bin);
                        continue;
                    case "MOGP":
                    //ReadMOGPChunk(chunk, bin);
                    //continue;
                    case "MOSB":
                    case "MOPV":
                    case "MOPT":
                    case "MOPR":
                    case "MOVV": //Visible block vertices
                    case "MOVB": //Visible block list
                    case "MOLT":
                    case "MODS":
                    
                    case "MFOG":
                    case "MCVP":
                    case "GFID": // Legion
                        continue;
                    default:
                        throw new Exception(String.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunk.ToString(), position.ToString(), filename));
                }
            }
            //open group files
            WMOGroupFile[] groupFiles = new WMOGroupFile[wmofile.header.nGroups];
            for (int i = 0; i < wmofile.header.nGroups; i++)
            {
                string groupfilename = filename.ToLower().Replace(".wmo", "_" + i.ToString().PadLeft(3, '0') + ".wmo");

                if (_lod)
                {
                    if (CASC.FileExists(groupfilename.Replace(".wmo", "_lod2.wmo")))
                    {
                        groupfilename = groupfilename.Replace(".wmo", "_lod2.wmo");
                        Console.WriteLine("[LOD] Loading LOD 2 for group " + i);
                    }
                    else if (CASC.FileExists(groupfilename.Replace(".wmo", "_lod1.wmo")))
                    {
                        groupfilename = groupfilename.Replace(".wmo", "_lod1.wmo");
                        Console.WriteLine("[LOD] Loading LOD 1 for group " + i);
                    }
                    else
                    {
                        Console.WriteLine("[LOD] No LOD " + i);
                    }
                }
                
                if (!CASC.FileExists(groupfilename))
                {
                    new WoWFormatLib.Utils.MissingFile(groupfilename);
                    return;
                }
                else
                {
                    using (Stream wmoStream = CASC.OpenFile(groupfilename))
                    {
                        groupFiles[i] = ReadWMOGroupFile(groupfilename, wmoStream);
                    }
                }
            }

            wmofile.group = groupFiles;
        }

        private WMOGroupFile ReadWMOGroupFile(string filename, Stream wmo)
        {
            WMOGroupFile groupFile = new WMOGroupFile();

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
                        groupFile.version = bin.Read<MVER>();
                        if (wmofile.version.version != 17)
                        {
                            throw new Exception("Unsupported WMO version! (" + wmofile.version.version + ")");
                        }
                        continue;
                    case "MOGP":
                        groupFile.mogp = ReadMOGPChunk(chunk, bin);
                        continue;
                    default:
                        throw new Exception(String.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunk.ToString(), position.ToString(), filename));
                }
            }
            return groupFile;
        }
    }
}