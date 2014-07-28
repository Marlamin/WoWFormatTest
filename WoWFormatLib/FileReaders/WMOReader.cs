using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using WoWFormatLib.Utils;
using WoWFormatLib.Structs.WMO;

namespace WoWFormatLib.FileReaders
{
    public class WMOReader
    {
        public WMO wmo; 
        private List<String> blpFiles;
        private List<String> m2Files;
        private List<String> wmoGroups;

        private string basedir;

        public WMOReader(string basedir)
        {
            this.basedir = basedir;
        }

        public void LoadWMO(string filename)
        {
            m2Files = new List<string>();
            blpFiles = new List<string>();
            wmoGroups = new List<string>();
            if (File.Exists(Path.Combine(basedir, filename)))
            {
                using (FileStream wmoStream = File.Open(Path.Combine(basedir, filename), FileMode.Open))
                {
                    ReadWMO(filename, wmoStream);
                }
            }
            else
            {
                new WoWFormatLib.Utils.MissingFile(filename);
            }

        }

        private void ReadWMO(string filename, FileStream wmo)
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
                        UInt32 wmover = bin.ReadUInt32();
                        if (wmover != 17)
                        {
                            throw new Exception("Unsupported WMO version! (" + wmover + ")");
                        }
                        continue;
                    case "MOTX":
                        ReadMOTXChunk(chunk, bin);
                        continue;
                    case "MOVV":
                       // ReadMOVVChunk(chunk, bin);
                        continue;
                    case "MOHD":
                        ReadMOHDChunk(chunk, bin, filename);
                        continue;
                    case "MOGN":
                        ReadMOGNChunk(chunk, bin);
                        continue;
                    case "MOGP":
                        ReadMOGPChunk(chunk, bin);
                        continue;
                    case "MODN":
                    case "MOMT":
                    case "MOGI":
                    case "MOSB":
                    case "MOPV":
                    case "MOPT":
                    case "MOPR":
                    case "MOVB":
                    case "MOLT":
                    case "MODS":
                    case "MODD":
                    case "MFOG":
                    case "MCVP":
                        continue;
                    default:
                        throw new Exception(String.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunk.ToString(), position.ToString(), filename));
                }
            }
        }

        public void ReadMOGPChunk(BlizzHeader chunk, BinaryReader bin)
        {
            bin.ReadBytes(68); //read rest of header
            MemoryStream stream = new MemoryStream(bin.ReadBytes((int)chunk.Size));
            var subbin = new BinaryReader(stream);
            BlizzHeader subchunk;
            long position = 0;
            while (position < stream.Length)
            {
                stream.Position = position;
                subchunk = new BlizzHeader(subbin.ReadChars(4), subbin.ReadUInt32());
                subchunk.Flip();
                position = stream.Position + subchunk.Size;

                switch (subchunk.ToString())
                {
                    case "MVER":
                        UInt32 wmover = subbin.ReadUInt32();
                        if (wmover != 17)
                        {
                            throw new Exception("Unsupported WMO version! (" + wmover + ")");
                        }
                        continue;
                    case "MOPY": //Material info for triangles, two bytes per triangle. 
                    case "MOVI": //Vertex indices for triangles
                    case "MOVT": //Vertices chunk
                       // ReadMOVTChunk();
                    case "MONR": //Normals
                    case "MOTV": //Texture coordinates
                    case "MOBA": //Render batches
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
                        continue;
                    default:
                        throw new Exception(String.Format("Found unknown header at offset {1} \"{0}\" while we should've already read them all!", subchunk.ToString(), position.ToString()));
                }
            }
        }
        public void ReadMOVTChunk(BlizzHeader chunk, BinaryReader bin)
        {

        }
        public void ReadMOGNChunk(BlizzHeader chunk, BinaryReader bin)
        {
            //List of group names for the groups in this map object.
            var wmoGroupsChunk = bin.ReadBytes((int)chunk.Size);

            var str = new StringBuilder();
            int group = 0;

            for (var i = 0; i < wmoGroupsChunk.Length; i++)
            {
                if (wmoGroupsChunk[i] == '\0')
                {
                    if (str.Length > 1)
                    {
                        wmoGroups.Add(str.ToString());
                        //Console.WriteLine("         " + str.ToString() + " (group file " + group.ToString() + ")");
                        group++;
                    }
                    str = new StringBuilder();
                }
                else
                {
                    str.Append((char)wmoGroupsChunk[i]);
                }
            }
        }

        public void ReadMOHDChunk(BlizzHeader chunk, BinaryReader bin, string filename)
        {
            //Header for the map object. 64 bytes.
           // var MOHDChunk = bin.ReadBytes((int)chunk.Size);
            var nMaterials = bin.ReadUInt32();
            var nGroups = bin.ReadUInt32();
            var nPortals = bin.ReadUInt32();
            var nLights = bin.ReadUInt32();
            var nModels = bin.ReadUInt32();
        
            //Console.WriteLine("         " + nGroups.ToString() + " group(s)");
            
            for (int i = 0; i < nGroups; i++)
            {
                var groupfilename = filename.Replace(".WMO", "_" + i.ToString().PadLeft(3, '0') + ".WMO");
                groupfilename = filename.Replace(".wmo", "_" + i.ToString().PadLeft(3, '0') + ".wmo");
                if (!System.IO.File.Exists(System.IO.Path.Combine(basedir, groupfilename)))
                {
                    new WoWFormatLib.Utils.MissingFile(groupfilename);
                }
            }
        }

        public void ReadMOTXChunk(BlizzHeader chunk, BinaryReader bin)
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
                        str.Replace("..", ".");
                        blpFiles.Add(str.ToString());
                        if (!System.IO.File.Exists(System.IO.Path.Combine(basedir, str.ToString())))
                        {
                            new WoWFormatLib.Utils.MissingFile(str.ToString());
                        }
                        else
                        {
                           // Console.WriteLine(str.ToString() + " exists!");
                            // Console.ReadLine();
                        }
                        //Console.WriteLine("         " + str.ToString());
                    }
                    str = new StringBuilder();
                }
                else
                {
                    str.Append((char)blpFilesChunk[i]);
                }
            }
        }

        public void ReadMODNChunk(BlizzHeader chunk, BinaryReader bin)
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
                        m2Files.Add(str.ToString());
                        var m2reader = new M2Reader(basedir);
                        m2reader.LoadM2(str.ToString());
                        
                    }
                    str = new StringBuilder();
                }
                else
                {
                    str.Append((char)m2FilesChunk[i]);
                }
            }
        }


    }
}
