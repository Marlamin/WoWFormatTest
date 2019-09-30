using CASCLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using WoWFormatLib.FileReaders;

namespace ADTH20Test
{

    public class ADTExporter
    {
        public struct RenderBatch
        {
            public uint firstFace;
            public uint materialID;
            public uint numFaces;
            public uint groupID;
            public uint blendType;
        }

        public struct Vertex
        {
            public Vector3D Position;
            public Vector3D Normal;
            public Vector2D TexCoord;
            public Vector3D Color;
        }
        public struct Vector3D
        {
            public double X;
            public double Y;
            public double Z;
        }

        public struct Vector2D
        {
            public double X;
            public double Y;
        }

        public static void ExportADT(uint wdtFileDataID, byte tileX, byte tileY, BackgroundWorker exportworker = null)
        {
            if (exportworker == null)
            {
                exportworker = new BackgroundWorker();
                exportworker.WorkerReportsProgress = true;
            }

            var outdir = "export";

            var customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            var MaxSize = 51200 / 3.0;
            var TileSize = MaxSize / 32.0;
            var ChunkSize = TileSize / 16.0;
            var UnitSize = ChunkSize / 8.0;
            var UnitSizeHalf = UnitSize / 2.0;

            var wdtFilename = "world/maps/azeroth/azeroth.wdt";
           
            var mapName = Path.GetFileNameWithoutExtension(wdtFilename);
            var file = "world/maps/" + mapName + "/" + mapName + "_" + tileX.ToString() + "_" + tileY.ToString() + ".adt";

            var reader = new ADTReader();
            reader.LoadADT(wdtFileDataID, tileX, tileY, true, wdtFilename);

            if (reader.adtfile.chunks == null)
            {
                Logger.WriteLine("ADT OBJ Exporter: File {0} has no chunks, skipping export!", file);
                return;
            }

            Logger.WriteLine("ADT OBJ Exporter: Starting export of {0}..", file);

            Directory.CreateDirectory(Path.Combine(outdir, Path.GetDirectoryName(file)));

            exportworker.ReportProgress(0, "Loading ADT " + file);

            var renderBatches = new List<RenderBatch>();
            var verticelist = new List<Vertex>();
            var indicelist = new List<int>();
            var materials = new Dictionary<int, string>();

            var bakeQuality = "none";

            // Calculate ADT offset in world coordinates
            var adtStartX = ((reader.adtfile.x - 32) * TileSize) * -1;
            var adtStartY = ((reader.adtfile.y - 32) * TileSize) * -1;

            // Calculate first chunk offset in world coordinates
            var initialChunkX = adtStartY + (reader.adtfile.chunks[0].header.indexX * ChunkSize) * -1;
            var initialChunkY = adtStartX + (reader.adtfile.chunks[0].header.indexY * ChunkSize) * -1;

            uint ci = 0;
            for (var x = 0; x < 16; x++)
            {
                double xOfs = x / 16d;
                for (var y = 0; y < 16; y++)
                {
                    double yOfs = y / 16d;

                    var genx = (initialChunkX + (ChunkSize * x) * -1);
                    var geny = (initialChunkY + (ChunkSize * y) * -1);

                    var chunk = reader.adtfile.chunks[ci];

                    var off = verticelist.Count();

                    var batch = new RenderBatch();

                    for (int i = 0, idx = 0; i < 17; i++)
                    {
                        bool isSmallRow = (i % 2) != 0;
                        int rowLength = isSmallRow ? 8 : 9;

                        for (var j = 0; j < rowLength; j++)
                        {
                            var v = new Vertex();

                            v.Normal = new Vector3D
                            {
                                X = (double)chunk.normals.normal_0[idx] / 127,
                                Y = (double)chunk.normals.normal_2[idx] / 127,
                                Z = (double)chunk.normals.normal_1[idx] / 127
                            };

                            var px = geny - (j * UnitSize);
                            var py = chunk.vertices.vertices[idx++] + chunk.header.position.Z;
                            var pz = genx - (i * UnitSizeHalf);

                            v.Position = new Vector3D
                            {
                                X = px,
                                Y = py,
                                Z = pz
                            };

                            if ((i % 2) != 0)
                                v.Position.X = (px - UnitSizeHalf);

                            double ofs = j;
                            if (isSmallRow)
                                ofs += 0.5;

                            if (bakeQuality == "high")
                            {
                                double tx = ofs / 8d;
                                double ty = 1 - (i / 16d);
                                v.TexCoord = new Vector2D { X = tx, Y = ty };
                            }
                            else
                            {
                                double tx = -(v.Position.X - initialChunkY) / TileSize;
                                double ty = (v.Position.Z - initialChunkX) / TileSize;

                                v.TexCoord = new Vector2D { X = tx, Y = ty };
                            }
                            verticelist.Add(v);
                        }
                    }

                    batch.firstFace = (uint)indicelist.Count();

                    // Stupid C# and its structs
                    var holesHighRes = new byte[8];
                    holesHighRes[0] = chunk.header.holesHighRes_0;
                    holesHighRes[1] = chunk.header.holesHighRes_1;
                    holesHighRes[2] = chunk.header.holesHighRes_2;
                    holesHighRes[3] = chunk.header.holesHighRes_3;
                    holesHighRes[4] = chunk.header.holesHighRes_4;
                    holesHighRes[5] = chunk.header.holesHighRes_5;
                    holesHighRes[6] = chunk.header.holesHighRes_6;
                    holesHighRes[7] = chunk.header.holesHighRes_7;

                    for (int j = 9, xx = 0, yy = 0; j < 145; j++, xx++)
                    {
                        if (xx >= 8)
                        { xx = 0; ++yy; }
                        var isHole = true;

                        // Check if chunk is using low-res holes
                        if ((chunk.header.flags & 0x10000) == 0)
                        {
                            // Calculate current hole number
                            var currentHole = (int)Math.Pow(2,
                                    Math.Floor(xx / 2f) * 1f +
                                    Math.Floor(yy / 2f) * 4f);

                            // Check if current hole number should be a hole
                            if ((chunk.header.holesLowRes & currentHole) == 0)
                            {
                                isHole = false;
                            }
                        }

                        else
                        {
                            // Check if current section is a hole
                            if (((holesHighRes[yy] >> xx) & 1) == 0)
                            {
                                isHole = false;
                            }
                        }

                        if (!isHole)
                        {
                            indicelist.AddRange(new int[] { off + j + 8, off + j - 9, off + j });
                            indicelist.AddRange(new int[] { off + j - 9, off + j - 8, off + j });
                            indicelist.AddRange(new int[] { off + j - 8, off + j + 9, off + j });
                            indicelist.AddRange(new int[] { off + j + 9, off + j + 8, off + j });

                            // Generates quads instead of 4x triangles
                            //indicelist.AddRange(new int[] { off + j + 8, off + j - 9, off + j - 8 });
                            //indicelist.AddRange(new int[] { off + j - 8, off + j + 9, off + j + 8 });

                        }

                        if ((j + 1) % (9 + 8) == 0)
                            j += 9;
                    }

                    if (bakeQuality == "high")
                    {
                        materials.Add((int)ci + 1, Path.GetFileNameWithoutExtension(file).Replace(" ", "") + "_" + ci);
                        batch.materialID = ci + 1;
                    }
                    else
                    {
                        if (!materials.ContainsKey(1))
                        {
                            materials.Add(1, Path.GetFileNameWithoutExtension(file).Replace(" ", ""));
                        }
                        batch.materialID = (uint)materials.Count();
                    }

                    batch.numFaces = (uint)(indicelist.Count()) - batch.firstFace;

                    var layermats = new List<uint>();


                    renderBatches.Add(batch);
                    ci++;
                }
            }

            exportworker.ReportProgress(75, "Exporting terrain textures..");

            if (bakeQuality != "none")
            {
                var mtlsw = new StreamWriter(Path.Combine(outdir, Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file).Replace(" ", "") + ".mtl"));

                //No idea how MTL files really work yet. Needs more investigation.
                foreach (var material in materials)
                {
                    mtlsw.WriteLine("newmtl " + material.Value.Replace(" ", ""));
                    mtlsw.WriteLine("Ka 1.000000 1.000000 1.000000");
                    mtlsw.WriteLine("Kd 0.640000 0.640000 0.640000");
                    mtlsw.WriteLine("map_Ka " + material.Value.Replace(" ", "") + ".png");
                    mtlsw.WriteLine("map_Kd " + material.Value.Replace(" ", "") + ".png");
                }

                mtlsw.Close();
            }

            exportworker.ReportProgress(85, "Exporting terrain geometry..");

            var indices = indicelist.ToArray();

            var adtname = Path.GetFileNameWithoutExtension(file);

            var objsw = new StreamWriter(Path.Combine(outdir, Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file).Replace(" ", "") + ".obj"));

            objsw.WriteLine("# Written by Marlamin's WoW OBJExporter. Original file: " + file);
            if (bakeQuality != "none")
            {
                objsw.WriteLine("mtllib " + Path.GetFileNameWithoutExtension(file).Replace(" ", "") + ".mtl");
            }

            var verticeCounter = 1;
            var chunkCounter = 1;
            foreach (var vertex in verticelist)
            {
                //objsw.WriteLine("# C" + chunkCounter + ".V" + verticeCounter);
                objsw.WriteLine("v " + vertex.Position.X.ToString("R") + " " + vertex.Position.Y.ToString("R") + " " + vertex.Position.Z.ToString("R"));
                objsw.WriteLine("vt " + vertex.TexCoord.X + " " + vertex.TexCoord.Y);
                objsw.WriteLine("vn " + vertex.Normal.X.ToString("R") + " " + vertex.Normal.Y.ToString("R") + " " + vertex.Normal.Z.ToString("R"));
                verticeCounter++;
                if (verticeCounter == 146)
                {
                    chunkCounter++;
                    verticeCounter = 1;
                }
            }

            if (bakeQuality != "high")
            {
                objsw.WriteLine("g " + adtname.Replace(" ", ""));
                objsw.WriteLine("usemtl " + materials[1]);
                objsw.WriteLine("s 1");
            }

            for (int rbi = 0; rbi < renderBatches.Count(); rbi++)
            {
                var renderBatch = renderBatches[rbi];
                var i = renderBatch.firstFace;
                if (bakeQuality == "high" && materials.ContainsKey((int)renderBatch.materialID))
                {
                    objsw.WriteLine("g " + adtname.Replace(" ", "") + "_" + rbi);
                    objsw.WriteLine("usemtl " + materials[(int)renderBatch.materialID]);
                }
                while (i < (renderBatch.firstFace + renderBatch.numFaces))
                {
                    objsw.WriteLine("f " +
                        (indices[i + 2] + 1) + "/" + (indices[i + 2] + 1) + "/" + (indices[i + 2] + 1) + " " +
                        (indices[i + 1] + 1) + "/" + (indices[i + 1] + 1) + "/" + (indices[i + 1] + 1) + " " +
                        (indices[i] + 1) + "/" + (indices[i] + 1) + "/" + (indices[i] + 1));
                    i = i + 3;
                }
            }

            objsw.Close();

            Logger.WriteLine("ADT OBJ Exporter: Finished with export of {0}..", file);
        }
    }
}
