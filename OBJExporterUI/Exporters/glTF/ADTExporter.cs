using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using WoWFormatLib.FileReaders;
using WoWFormatLib.Utils;

namespace OBJExporterUI.Exporters.glTF
{
    public class ADTExporter
    {
        public static void exportADT(string file, BackgroundWorker exportworker = null)
        {
            if (exportworker == null)
            {
                exportworker = new BackgroundWorker();
                exportworker.WorkerReportsProgress = true;
            }

            var outdir = ConfigurationManager.AppSettings["outdir"];

            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            float TileSize = 1600.0f / 3.0f; //533.333
            float ChunkSize = TileSize / 16.0f; //33.333
            float UnitSize = ChunkSize / 8.0f; //4.166666
            float MapMidPoint = 32.0f / ChunkSize;

            var mapname = file.Replace("world/maps/", "").Substring(0, file.Replace("world/maps/", "").IndexOf("/"));
            var coord = file.Replace("world/maps/" + mapname + "/" + mapname, "").Replace(".adt", "").Split('_');

            List<Structs.RenderBatch> renderBatches = new List<Structs.RenderBatch>();
            List<Structs.Vertex> verticelist = new List<Structs.Vertex>();
            List<int> indicelist = new List<Int32>();
            Dictionary<int, string> materials = new Dictionary<int, string>();

            if (!Directory.Exists(Path.Combine(outdir, Path.GetDirectoryName(file))))
            {
                Directory.CreateDirectory(Path.Combine(outdir, Path.GetDirectoryName(file)));
            }

            if (!CASC.cascHandler.FileExists(file))
            {
                Console.WriteLine("File " + file + " does not exist");
                return;
            }

            exportworker.ReportProgress(0, "Loading ADT " + file);

            ADTReader reader = new ADTReader();
            reader.LoadADT(file);

            if (reader.adtfile.chunks == null)
            {
                return;
            }

            var initialChunkY = reader.adtfile.chunks[0].header.position.Y;
            var initialChunkX = reader.adtfile.chunks[0].header.position.X;

            var stream = new FileStream(Path.Combine(outdir, file.Replace(".adt", ".bin")), FileMode.OpenOrCreate);
            var writer = new BinaryWriter(stream);

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
                    if (xx >= 8) { xx = 0; ++yy; }
                    bool isHole = true;

                    if ((chunk.header.flags & 0x10000) == 0)
                    {
                        var currentHole = (int)Math.Pow(2,
                                Math.Floor(xx / 2f) * 1f +
                                Math.Floor(yy / 2f) * 4f);

                        if ((chunk.header.holesLowRes & currentHole) == 0)
                        {
                            isHole = false;
                        }
                    }

                    else
                    {
                        if (((holesHighRes[yy] >> xx) & 1) == 0)
                        {
                            isHole = false;
                        }
                    }

                    if (!isHole)
                    {
                        indicelist.AddRange(new Int32[] { off + j + 8, off + j - 9, off + j });
                        indicelist.AddRange(new Int32[] { off + j - 9, off + j - 8, off + j });
                        indicelist.AddRange(new Int32[] { off + j - 8, off + j + 9, off + j });
                        indicelist.AddRange(new Int32[] { off + j + 9, off + j + 8, off + j });

                        // Generates quads instead of 4x triangles
                        /*
                        indicelist.AddRange(new Int32[] { off + j + 8, off + j - 9, off + j - 8 });
                        indicelist.AddRange(new Int32[] { off + j - 8, off + j + 9, off + j + 8 });
                        */
                    }

                    if ((j + 1) % (9 + 8) == 0) j += 9;
                }

                batch.materialID = (uint)materials.Count();

                batch.numFaces = (uint)(indicelist.Count()) - batch.firstFace;

                renderBatches.Add(batch);
            }

            ConfigurationManager.RefreshSection("appSettings");

            if (ConfigurationManager.AppSettings["exportEverything"] == "True")
            {
                exportworker.ReportProgress(25, "Exporting WMOs");

                for (int mi = 0; mi < reader.adtfile.objects.worldModels.entries.Count(); mi++)
                {
                    var wmo = reader.adtfile.objects.worldModels.entries[mi];

                    var filename = reader.adtfile.objects.wmoNames.filenames[wmo.mwidEntry];

                    if (!File.Exists(Path.GetFileNameWithoutExtension(filename).ToLower() + ".obj"))
                    {
                        WMOExporter.exportWMO(filename, null, Path.Combine(outdir, Path.GetDirectoryName(file)));
                    }
                }

                exportworker.ReportProgress(50, "Exporting M2s");

                for (int mi = 0; mi < reader.adtfile.objects.models.entries.Count(); mi++)
                {
                    var doodad = reader.adtfile.objects.models.entries[mi];

                    var filename = reader.adtfile.objects.m2Names.filenames[doodad.mmidEntry];

                    if (!File.Exists(Path.GetFileNameWithoutExtension(filename).ToLower() + ".obj"))
                    {
                        //M2Exporter.exportM2(filename, null, Path.Combine(outdir, Path.GetDirectoryName(file)));
                    }
                }
            }

            exportworker.ReportProgress(85, "Exporting terrain geometry..");

            var indices = indicelist.ToArray();

            var adtname = Path.GetFileNameWithoutExtension(file);

            foreach (var vertex in verticelist)
            {
                //objsw.WriteLine("v " + vertex.Position.X + " " + vertex.Position.Y + " " + vertex.Position.Z);
                //objsw.WriteLine("vt " + vertex.TexCoord.X + " " + -vertex.TexCoord.Y);
                //objsw.WriteLine("vn " + vertex.Normal.X + " " + vertex.Normal.Y + " " + vertex.Normal.Z);
            }

            foreach (var renderBatch in renderBatches)
            {
                var i = renderBatch.firstFace;
                if (materials.ContainsKey((int)renderBatch.materialID)) {
                    //objsw.WriteLine("usemtl " + materials[(int)renderBatch.materialID]); objsw.WriteLine("s 1");
                }
                while (i < (renderBatch.firstFace + renderBatch.numFaces))
                {
                    //objsw.WriteLine("f " + (indices[i + 2] + 1) + "/" + (indices[i + 2] + 1) + "/" + (indices[i + 2] + 1) + " " + (indices[i + 1] + 1) + "/" + (indices[i + 1] + 1) + "/" + (indices[i + 1] + 1) + " " + (indices[i] + 1) + "/" + (indices[i] + 1) + "/" + (indices[i] + 1));
                    i = i + 3;
                }
            }
        }
    }
}
