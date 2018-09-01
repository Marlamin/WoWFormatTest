using Newtonsoft.Json;
using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using WoWFormatLib.FileReaders;

namespace OBJExporterUI.Exporters.glTF
{
    public class ADTExporter
    {
        public static void ExportADT(string file, BackgroundWorker exportworker = null)
        {
            if (exportworker == null)
            {
                exportworker = new BackgroundWorker
                {
                    WorkerReportsProgress = true
                };
            }

            var outdir = ConfigurationManager.AppSettings["outdir"];

            var customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            var TileSize = 1600.0f / 3.0f; //533.333
            var ChunkSize = TileSize / 16.0f; //33.333
            var UnitSize = ChunkSize / 8.0f; //4.166666

            var mapname = file.Replace("world/maps/", "").Substring(0, file.Replace("world/maps/", "").IndexOf("/"));
            var coord = file.Replace("world/maps/" + mapname + "/" + mapname, "").Replace(".adt", "").Split('_');

            CASCLib.Logger.WriteLine("ADT glTF Exporter: Starting export of {0}..", file);

            if (!Directory.Exists(Path.Combine(outdir, Path.GetDirectoryName(file))))
            {
                Directory.CreateDirectory(Path.Combine(outdir, Path.GetDirectoryName(file)));
            }

            exportworker.ReportProgress(0, "Loading ADT " + file);

            var reader = new ADTReader();
            reader.LoadADT(file.Replace('/', '\\'));

            if (reader.adtfile.chunks == null)
            {
                CASCLib.Logger.WriteLine("ADT glTF Exporter: File {0} has no chunks, skipping export!", file);
                return;
            }

            var renderBatches = new List<Structs.RenderBatch>();
            var materials = new Dictionary<int, string>();

            var glTF = new glTF()
            {
                asset = new Asset()
                {
                    version = "2.0",
                    generator = "Marlamin's WoW Exporter " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                    copyright = "Contents are owned by Blizzard Entertainment"
                }
            };

            var initialChunkY = reader.adtfile.chunks[0].header.position.Y;
            var initialChunkX = reader.adtfile.chunks[0].header.position.X;

            ConfigurationManager.RefreshSection("appSettings");
            var bakeQuality = ConfigurationManager.AppSettings["bakeQuality"];

            var bufferViews = new List<BufferView>();
            var accessorInfo = new List<Accessor>();
            var meshes = new List<Mesh>();

            glTF.buffers = new Buffer[1];

            if (bakeQuality == "low" || bakeQuality == "medium")
            {
                glTF.images = new Image[1];
                glTF.materials = new Material[1];
                glTF.textures = new Texture[1];
            }
            else if (bakeQuality == "high")
            {
                glTF.images = new Image[256];
                glTF.materials = new Material[256];
                glTF.textures = new Texture[256];
            }
            var stream = new FileStream(Path.Combine(outdir, file.Replace(".adt", ".bin")), FileMode.OpenOrCreate);
            var writer = new BinaryWriter(stream);
            for (var c = 0; c < reader.adtfile.chunks.Count(); c++)
            {
                var chunk = reader.adtfile.chunks[c];

                var localVertices = new Structs.Vertex[145];
                for (int i = 0, idx = 0; i < 17; i++)
                {
                    for (var j = 0; j < (((i % 2) != 0) ? 8 : 9); j++)
                    {
                        var v = new Structs.Vertex
                        {
                            Normal = new Vector3(chunk.normals.normal_2[idx] / 127f, chunk.normals.normal_0[idx] / 127f, chunk.normals.normal_1[idx] / 127f),
                            Position = new Vector3(chunk.header.position.Y - (j * UnitSize), chunk.vertices.vertices[idx++] + chunk.header.position.Z, chunk.header.position.X - (i * UnitSize * 0.5f))
                        };

                        if ((i % 2) != 0) v.Position.X -= 0.5f * UnitSize;
                        if (bakeQuality == "low" || bakeQuality == "medium")
                        {
                            v.TexCoord = new Vector2(-(v.Position.X - initialChunkX) / TileSize, -(v.Position.Z - initialChunkY) / TileSize);
                        }
                        else if (bakeQuality == "high")
                        {
                            v.TexCoord = new Vector2(-(v.Position.X - initialChunkX) / ChunkSize, -(v.Position.Z - initialChunkY) / ChunkSize);
                        }
                        localVertices[idx - 1] = v;
                    }
                }

                var vPosBuffer = new BufferView()
                {
                    buffer = 0,
                    byteOffset = (uint)writer.BaseStream.Position,
                    target = 34962
                };

                var minPosX = float.MaxValue;
                var minPosY = float.MaxValue;
                var minPosZ = float.MaxValue;

                var maxPosX = float.MinValue;
                var maxPosY = float.MinValue;
                var maxPosZ = float.MinValue;

                // Position buffer
                foreach (var vertex in localVertices)
                {
                    writer.Write(vertex.Position.X);
                    writer.Write(vertex.Position.Y);
                    writer.Write(vertex.Position.Z);

                    if (vertex.Position.X < minPosX) minPosX = vertex.Position.X;
                    if (vertex.Position.Y < minPosY) minPosY = vertex.Position.Y;
                    if (vertex.Position.Z < minPosZ) minPosZ = vertex.Position.Z;

                    if (vertex.Position.X > maxPosX) maxPosX = vertex.Position.X;
                    if (vertex.Position.Y > maxPosY) maxPosY = vertex.Position.Y;
                    if (vertex.Position.Z > maxPosZ) maxPosZ = vertex.Position.Z;
                }

                vPosBuffer.byteLength = (uint)writer.BaseStream.Position - vPosBuffer.byteOffset;

                var posLoc = accessorInfo.Count();

                accessorInfo.Add(new Accessor()
                {
                    name = "vPos",
                    bufferView = bufferViews.Count(),
                    byteOffset = 0,
                    componentType = 5126,
                    count = 145,
                    type = "VEC3",
                    min = new float[] { minPosX, minPosY, minPosZ },
                    max = new float[] { maxPosX, maxPosY, maxPosZ }
                });

                bufferViews.Add(vPosBuffer);

                // Normal buffer
                var normalBuffer = new BufferView()
                {
                    buffer = 0,
                    byteOffset = (uint)writer.BaseStream.Position,
                    target = 34962
                };

                foreach (var vertex in localVertices)
                {
                    writer.Write(vertex.Normal.X);
                    writer.Write(vertex.Normal.Y);
                    writer.Write(vertex.Normal.Z);
                }

                normalBuffer.byteLength = (uint)writer.BaseStream.Position - normalBuffer.byteOffset;

                var normalLoc = accessorInfo.Count();

                accessorInfo.Add(new Accessor()
                {
                    name = "vNormal",
                    bufferView = bufferViews.Count(),
                    byteOffset = 0,
                    componentType = 5126,
                    count = 145,
                    type = "VEC3"
                });

                bufferViews.Add(normalBuffer);

                // Texcoord buffer
                var texCoordBuffer = new BufferView()
                {
                    buffer = 0,
                    byteOffset = (uint)writer.BaseStream.Position,
                    target = 34962
                };

                foreach (var vertex in localVertices)
                {
                    writer.Write(vertex.TexCoord.X);
                    writer.Write(vertex.TexCoord.Y);
                }

                texCoordBuffer.byteLength = (uint)writer.BaseStream.Position - texCoordBuffer.byteOffset;

                var texLoc = accessorInfo.Count();

                accessorInfo.Add(new Accessor()
                {
                    name = "vTex",
                    bufferView = bufferViews.Count(),
                    byteOffset = 0,
                    componentType = 5126,
                    count = 145,
                    type = "VEC2"
                });

                bufferViews.Add(texCoordBuffer);

                var indexBufferPos = bufferViews.Count();

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

                var indicelist = new List<int>();

                for (int j = 9, xx = 0, yy = 0; j < 145; j++, xx++)
                {
                    if (xx >= 8) { xx = 0; ++yy; }
                    var isHole = true;

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
                        indicelist.AddRange(new int[] { j + 8, j, j - 9 });
                        indicelist.AddRange(new int[] { j - 9, j, j - 8 });
                        indicelist.AddRange(new int[] { j - 8, j, j + 9 });
                        indicelist.AddRange(new int[] { j + 9, j, j + 8 });
                    }

                    if ((j + 1) % (9 + 8) == 0) j += 9;
                }

                accessorInfo.Add(new Accessor()
                {
                    name = "indices",
                    bufferView = indexBufferPos,
                    byteOffset = 0,
                    componentType = 5125,
                    count = (uint)indicelist.Count(),
                    type = "SCALAR"
                });

                var indiceBuffer = new BufferView()
                {
                    buffer = 0,
                    byteOffset = (uint)writer.BaseStream.Position,
                    target = 34963
                };

                for (var i = 0; i < indicelist.Count(); i++)
                {
                    writer.Write(indicelist[i]);
                }

                indiceBuffer.byteLength = (uint)writer.BaseStream.Position - indiceBuffer.byteOffset;

                bufferViews.Add(indiceBuffer);

                var mesh = new Mesh();
                mesh.primitives = new Primitive[1];
                mesh.primitives[0].attributes = new Dictionary<string, int>
                    {
                        { "POSITION", posLoc },
                        { "NORMAL", normalLoc },
                        { "TEXCOORD_0", texLoc }
                    };

                mesh.primitives[0].indices = (uint)accessorInfo.Count() - 1;
                if (bakeQuality == "low" || bakeQuality == "medium")
                {
                    mesh.primitives[0].material = 0;
                }
                else if (bakeQuality == "high")
                {
                    mesh.primitives[0].material = (uint)c;
                }

                mesh.primitives[0].mode = 4;
                mesh.name = "MCNK #" + c;
                meshes.Add(mesh);
               
                glTF.buffers[0].byteLength = (uint)writer.BaseStream.Length;
                glTF.buffers[0].uri = Path.GetFileNameWithoutExtension(file) + ".bin";

                if (bakeQuality == "low" || bakeQuality == "medium")
                {
                    glTF.images[0].uri = Path.GetFileNameWithoutExtension(file) + ".png";
                    glTF.textures[0].sampler = 0;
                    glTF.textures[0].source = 0;
                    glTF.materials[0].pbrMetallicRoughness = new PBRMetallicRoughness();
                    glTF.materials[0].pbrMetallicRoughness.baseColorTexture = new TextureIndex();
                    glTF.materials[0].pbrMetallicRoughness.baseColorTexture.index = 0;
                    glTF.materials[0].pbrMetallicRoughness.metallicFactor = 0.0f;
                    glTF.materials[0].alphaMode = "OPAQUE";
                    glTF.materials[0].alphaCutoff = 0.0f;
                }
                else if (bakeQuality == "high")
                {
                    glTF.images[c].uri = Path.GetFileNameWithoutExtension(file) + "_" + c + ".png";
                    glTF.textures[c].sampler = 0;
                    glTF.textures[c].source = (int)c;
                    glTF.materials[c].pbrMetallicRoughness = new PBRMetallicRoughness();
                    glTF.materials[c].pbrMetallicRoughness.baseColorTexture = new TextureIndex();
                    glTF.materials[c].pbrMetallicRoughness.baseColorTexture.index = (int)c;
                    glTF.materials[c].pbrMetallicRoughness.metallicFactor = 0.0f;
                    glTF.materials[c].alphaMode = "OPAQUE";
                    glTF.materials[c].alphaCutoff = 0.0f;
                }
            }

            writer.Close();
            writer.Dispose();

            glTF.bufferViews = bufferViews.ToArray();
            glTF.accessors = accessorInfo.ToArray();

            glTF.samplers = new Sampler[1];
            glTF.samplers[0].minFilter = 9986;
            glTF.samplers[0].magFilter = 9729;
            glTF.samplers[0].wrapS = 10497;
            glTF.samplers[0].wrapT = 10497;

            glTF.scenes = new Scene[1];
            glTF.scenes[0].name = Path.GetFileNameWithoutExtension(file);

            glTF.nodes = new Node[meshes.Count()];
            var meshIDs = new List<int>();
            for (var i = 0; i < meshes.Count(); i++)
            {
                glTF.nodes[i].name = meshes[i].name;
                glTF.nodes[i].mesh = i;
                meshIDs.Add(i);
            }

            glTF.scenes[0].nodes = meshIDs.ToArray();

            glTF.meshes = meshes.ToArray();

            glTF.scene = 0;

            exportworker.ReportProgress(95, "Writing to file..");

            File.WriteAllText(Path.Combine(outdir, file.Replace(".adt", ".gltf")), JsonConvert.SerializeObject(glTF, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            }));

            ConfigurationManager.RefreshSection("appSettings");

            if (ConfigurationManager.AppSettings["exportWMO"] == "True" || ConfigurationManager.AppSettings["exportM2"] == "True")
            {
                if (ConfigurationManager.AppSettings["exportWMO"] == "True")
                {
                    exportworker.ReportProgress(25, "Exporting WMOs");

                    for (var mi = 0; mi < reader.adtfile.objects.worldModels.entries.Count(); mi++)
                    {
                        var wmo = reader.adtfile.objects.worldModels.entries[mi];

                        var filename = reader.adtfile.objects.wmoNames.filenames[wmo.mwidEntry];

                        if (!File.Exists(Path.GetFileNameWithoutExtension(filename).ToLower() + ".gltf"))
                        {
                            WMOExporter.ExportWMO(filename.ToLower(), null, Path.Combine(outdir, Path.GetDirectoryName(file)));
                        }
                    }

                }

                if (ConfigurationManager.AppSettings["exportM2"] == "True")
                {
                    exportworker.ReportProgress(50, "Exporting M2s");

                    for (var mi = 0; mi < reader.adtfile.objects.models.entries.Count(); mi++)
                    {
                        var doodad = reader.adtfile.objects.models.entries[mi];

                        var filename = reader.adtfile.objects.m2Names.filenames[doodad.mmidEntry];

                        if (!File.Exists(Path.GetFileNameWithoutExtension(filename).ToLower() + ".gltf"))
                        {
                            M2Exporter.ExportM2(filename.ToLower(), null, Path.Combine(outdir, Path.GetDirectoryName(file)));
                        }
                    }
                }
            }
        }
    }
}
