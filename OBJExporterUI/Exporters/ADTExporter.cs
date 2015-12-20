using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using WoWFormatLib.FileReaders;
using WoWFormatLib.Utils;

namespace OBJExporterUI
{
    public class ADTExporter
    {
        public static void exportADT(string file, BackgroundWorker exportworker = null)
        {
            if (exportworker == null)
            {
                exportworker = new BackgroundWorker();
            }

            var outdir = ConfigurationManager.AppSettings["outdir"];

            float TileSize = 1600.0f / 3.0f; //533.333
            float ChunkSize = TileSize / 16.0f; //33.333
            float UnitSize = ChunkSize / 8.0f; //4.166666 // ~~fun fact time with marlamin~~ this /2 ends up being pixelspercoord on minimap
            float MapMidPoint = 32.0f / ChunkSize;

            var mapname = file.Replace("world\\maps\\", "").Substring(0, file.Replace("world\\maps\\", "").IndexOf("\\"));
            var coord = file.Replace("world\\maps\\" + mapname + "\\" + mapname, "").Replace(".adt", "").Split('_');

            var centerx = int.Parse(coord[1]);
            var centery = int.Parse(coord[2]);

            List<Structs.RenderBatch> renderBatches = new List<Structs.RenderBatch>();
            List<Structs.Vertex> verticelist = new List<Structs.Vertex>();
            List<int> indicelist = new List<Int32>();
            Dictionary<int, string> materials = new Dictionary<int, string>();

            var distance = 1;

            // Create output directory
            if (!Directory.Exists(Path.Combine(outdir, Path.GetDirectoryName(file))))
            {
                Directory.CreateDirectory(Path.Combine(outdir, Path.GetDirectoryName(file)));
            }

            for (int y = centery; y < centery + distance; y++)
            {
                for (int x = centerx; x < centerx + distance; x++)
                {
                    var curfile = "world\\maps\\" + mapname + "\\" + mapname + "_" + x + "_" + y + ".adt";

                    if (!CASC.FileExists(file))
                    {
                        Console.WriteLine("File " + file + " does not exist");
                        continue;
                    }

                    exportworker.ReportProgress(0, "Loading ADT " + curfile);

                    ADTReader reader = new ADTReader();
                    reader.LoadADT(curfile);

                    // No chunks? Let's get the hell out of here
                    if (reader.adtfile.chunks == null)
                    {
                        continue;
                    }
                    if (CASC.FileExists("world\\maptextures\\" + mapname + "\\" + mapname + "_" + y + "_" + x + ".blp"))
                    {
                        materials.Add(materials.Count() + 1, "mat" + y.ToString() + x.ToString());

                        var blpreader = new BLPReader();

                        blpreader.LoadBLP(curfile.Replace("maps", "maptextures").Replace(".adt", ".blp"));

                        try
                        {
                            blpreader.bmp.Save(Path.Combine(outdir, Path.GetDirectoryName(file), "mat" + y.ToString() + x.ToString() + ".png"));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }

                    //List<Material> materials = new List<Material>();

                    //for (int ti = 0; ti < reader.adtfile.textures.filenames.Count(); ti++)
                    //{
                    //    Material material = new Material();
                    //    material.filename = reader.adtfile.textures.filenames[ti];

                    //    //if (!WoWFormatLib.Utils.CASC.FileExists(material.filename)) { continue; }

                    //    material.textureID = BLPLoader.LoadTexture(reader.adtfile.textures.filenames[ti], cache);

                    //    materials.Add(material);
                    //}

                    var initialChunkY = reader.adtfile.chunks[0].header.position.Y;
                    var initialChunkX = reader.adtfile.chunks[0].header.position.X;

                    for (uint c = 0; c < reader.adtfile.chunks.Count(); c++)
                    {
                        var chunk = reader.adtfile.chunks[c];

                        int off = verticelist.Count();

                        Structs.RenderBatch batch = new Structs.RenderBatch();

                        for (int i = 0, idx = 0; i < 17; i++)
                        {
                            for (int j = 0; j < (((i % 2) != 0) ? 8 : 9); j++)
                            {
                                Structs.Vertex v = new Structs.Vertex();
                                v.Normal = new OpenTK.Vector3(chunk.normals.normal_2[idx] / 127f, chunk.normals.normal_0[idx] / 127f, chunk.normals.normal_1[idx] / 127f);
                                v.Position = new OpenTK.Vector3(chunk.header.position.Y - (j * UnitSize), chunk.vertices.vertices[idx++] + chunk.header.position.Z, chunk.header.position.X - (i * UnitSize * 0.5f));
                                if ((i % 2) != 0) v.Position.X -= 0.5f * UnitSize;
                                v.TexCoord = new Vector2(-(v.Position.X - initialChunkX) / TileSize, -(v.Position.Z - initialChunkY) / TileSize);
                                verticelist.Add(v);
                            }
                        }

                        batch.firstFace = (uint)indicelist.Count();

                        for (int j = 9; j < 145; j++)
                        {
                            indicelist.AddRange(new Int32[] { off + j + 8, off + j - 9, off + j });
                            indicelist.AddRange(new Int32[] { off + j - 9, off + j - 8, off + j });
                            indicelist.AddRange(new Int32[] { off + j - 8, off + j + 9, off + j });
                            indicelist.AddRange(new Int32[] { off + j + 9, off + j + 8, off + j });
                            if ((j + 1) % (9 + 8) == 0) j += 9;
                        }

                        batch.materialID = (uint)materials.Count();

                        batch.numFaces = (uint)(indicelist.Count()) - batch.firstFace;

                        //var layermats = new List<uint>();
                        //var alphalayermats = new List<int>();

                        //for (int li = 0; li < reader.adtfile.texChunks[c].layers.Count(); li++)
                        //{
                        //    if (reader.adtfile.texChunks[c].alphaLayer != null)
                        //    {
                        //        alphalayermats.Add(BLPLoader.GenerateAlphaTexture(reader.adtfile.texChunks[c].alphaLayer[li].layer));
                        //    }
                        //    layermats.Add((uint)cache.materials[reader.adtfile.textures.filenames[reader.adtfile.texChunks[c].layers[li].textureId].ToLower()]);
                        //}

                        //batch.materialID = layermats.ToArray();
                        //batch.alphaMaterialID = alphalayermats.ToArray();

                        renderBatches.Add(batch);
                    }
                }
            }

            var mtlsw = new StreamWriter(Path.Combine(outdir, Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file).Replace(" ", "") + ".mtl"));

            //No idea how MTL files really work yet. Needs more investigation.
            foreach (var material in materials)
            {
                mtlsw.WriteLine("newmtl " + material.Value);
                mtlsw.WriteLine("Ka 1.000000 1.000000 1.000000");
                mtlsw.WriteLine("Kd 0.640000 0.640000 0.640000");
                mtlsw.WriteLine("map_Ka " + material.Value + ".png");
                mtlsw.WriteLine("map_Kd " + material.Value + ".png");
            }

            mtlsw.Close();

            var indices = indicelist.ToArray();

            var adtname = Path.GetFileNameWithoutExtension(file);

            var objsw = new StreamWriter(Path.Combine(outdir, file.Replace(".adt", ".obj")));

            objsw.WriteLine("# Written by Marlamin's WoW OBJExporter. Original file: " + file);
            objsw.WriteLine("mtllib " + Path.GetFileNameWithoutExtension(file).Replace(" ", "") + ".mtl");
            objsw.WriteLine("g " + adtname);

            foreach (var vertex in verticelist)
            {
                objsw.WriteLine("v " + vertex.Position.X + " " + vertex.Position.Y + " " + vertex.Position.Z);
                objsw.WriteLine("vt " + vertex.TexCoord.X + " " + -vertex.TexCoord.Y);
                objsw.WriteLine("vn " + vertex.Normal.X + " " + vertex.Normal.Y + " " + vertex.Normal.Z);
            }

            foreach (var renderBatch in renderBatches)
            {
                var i = renderBatch.firstFace;
                if (materials.ContainsKey((int)renderBatch.materialID)) { objsw.WriteLine("usemtl " + materials[(int)renderBatch.materialID]); objsw.WriteLine("s 1"); }
                while (i < (renderBatch.firstFace + renderBatch.numFaces))
                {
                    objsw.WriteLine("f " + (indices[i] + 1) + "/" + (indices[i] + 1) + "/" + (indices[i] + 1) + " " + (indices[i + 1] + 1) + "/" + (indices[i + 1] + 1) + "/" + (indices[i + 1] + 1) + " " + (indices[i + 2] + 1) + "/" + (indices[i + 2] + 1) + "/" + (indices[i + 2] + 1));
                    i = i + 3;
                }
            }

            objsw.Close();
        }
    }
}
