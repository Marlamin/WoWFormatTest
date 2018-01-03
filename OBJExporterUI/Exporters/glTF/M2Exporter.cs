using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using WoWFormatLib.FileReaders;
using WoWFormatLib.Utils;
using CASCLib;
using Newtonsoft.Json;

namespace OBJExporterUI.Exporters.glTF
{
    class M2Exporter
    {
        public static void ExportM2(string file, BackgroundWorker exportworker = null, string destinationOverride = null)
        {
            if (exportworker == null)
            {
                exportworker = new BackgroundWorker();
                exportworker.WorkerReportsProgress = true;
            }

            Logger.WriteLine("M2 glTF Exporter: Loading file {0}...", file);

            exportworker.ReportProgress(5, "Reading M2..");

            var outdir = ConfigurationManager.AppSettings["outdir"];
            var reader = new M2Reader();
            reader.LoadM2(file);

            var customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            if (destinationOverride == null)
            {
                if (!Directory.Exists(Path.Combine(outdir, Path.GetDirectoryName(file))))
                {
                    Directory.CreateDirectory(Path.Combine(outdir, Path.GetDirectoryName(file)));
                }
            }

            file = file.ToLower();

            if (reader.model.vertices.Count() == 0)
            {
                Logger.WriteLine("M2 glTF Exporter: File {0} has no vertices, skipping export!", file);
                return;
            }



            exportworker.ReportProgress(25, "Generating glTF..");

            var glTF = new glTF()
            {
                asset = new Asset()
                {
                    version = "2.0",
                    generator = "Marlamin's WoW Exporter " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                    copyright = "Contents are owned by Blizzard Entertainment",
                    minVersion = "2.0"
                }
            };

            FileStream stream;

            if (destinationOverride == null)
            {
                stream = new FileStream(Path.Combine(outdir, file.Replace(".m2", ".bin")), FileMode.OpenOrCreate);
            }
            else
            {
                stream = new FileStream(Path.Combine(destinationOverride, Path.GetFileNameWithoutExtension(file).ToLower() + ".bin"), FileMode.OpenOrCreate);
            }

            var writer = new BinaryWriter(stream);
            var bufferViews = new List<BufferView>();
            var accessorInfo = new List<Accessor>();
            var meshes = new List<Mesh>();

            // Position bufferview
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

            for (var i = 0; i < reader.model.vertices.Count(); i++)
            {
                writer.Write(reader.model.vertices[i].position.X);
                writer.Write(reader.model.vertices[i].position.Z);
                writer.Write(reader.model.vertices[i].position.Y * -1);

                if (reader.model.vertices[i].position.X < minPosX) minPosX = reader.model.vertices[i].position.X;
                if (reader.model.vertices[i].position.Z < minPosY) minPosY = reader.model.vertices[i].position.Z;
                if (reader.model.vertices[i].position.Y * -1 < minPosZ) minPosZ = reader.model.vertices[i].position.Y * -1;

                if (reader.model.vertices[i].position.X > maxPosX) maxPosX = reader.model.vertices[i].position.X;
                if (reader.model.vertices[i].position.Z > maxPosY) maxPosY = reader.model.vertices[i].position.Z;
                if (reader.model.vertices[i].position.Y * -1 > maxPosZ) maxPosZ = reader.model.vertices[i].position.Y * -1;
            }

            vPosBuffer.byteLength = (uint)writer.BaseStream.Position - vPosBuffer.byteOffset;

            var posLoc = accessorInfo.Count();

            accessorInfo.Add(new Accessor()
            {
                name = "vPos",
                bufferView = bufferViews.Count(),
                byteOffset = 0,
                componentType = 5126,
                count = (uint)reader.model.vertices.Count(),
                type = "VEC3",
                min = new float[] { minPosX, minPosY, minPosZ },
                max = new float[] { maxPosX, maxPosY, maxPosZ }
            });

            bufferViews.Add(vPosBuffer);

            // Normal bufferview
            var normalBuffer = new BufferView()
            {
                buffer = 0,
                byteOffset = (uint)writer.BaseStream.Position,
                target = 34962
            };

            for (var i = 0; i < reader.model.vertices.Count(); i++)
            {
                writer.Write(reader.model.vertices[i].normal.X);
                writer.Write(reader.model.vertices[i].normal.Z);
                writer.Write(reader.model.vertices[i].normal.Y);
            }

            normalBuffer.byteLength = (uint)writer.BaseStream.Position - normalBuffer.byteOffset;

            var normalLoc = accessorInfo.Count();

            accessorInfo.Add(new Accessor()
            {
                name = "vNormal",
                bufferView = bufferViews.Count(),
                byteOffset = 0,
                componentType = 5126,
                count = (uint)reader.model.vertices.Count(),
                type = "VEC3"
            });

            bufferViews.Add(normalBuffer);

            // TexCoord bufferview
            var texCoordBuffer = new BufferView()
            {
                buffer = 0,
                byteOffset = (uint)writer.BaseStream.Position,
                target = 34962
            };

            for (var i = 0; i < reader.model.vertices.Count(); i++)
            {
                writer.Write(reader.model.vertices[i].textureCoordX);
                writer.Write(reader.model.vertices[i].textureCoordY);
            }

            texCoordBuffer.byteLength = (uint)writer.BaseStream.Position - texCoordBuffer.byteOffset;

            var texLoc = accessorInfo.Count();

            accessorInfo.Add(new Accessor()
            {
                name = "vTex",
                bufferView = bufferViews.Count(),
                byteOffset = 0,
                componentType = 5126,
                count = (uint)reader.model.vertices.Count(),
                type = "VEC2"
            });

            bufferViews.Add(texCoordBuffer);

            // Joints bufferview
            var jointBuffer = new BufferView()
            {
                buffer = 0,
                byteOffset = (uint)writer.BaseStream.Position,
                target = 34962
            };

            for (var i = 0; i < reader.model.vertices.Count(); i++)
            {
                writer.Write(reader.model.vertices[i].boneIndices_0);
                writer.Write(reader.model.vertices[i].boneIndices_1);
                writer.Write(reader.model.vertices[i].boneIndices_2);
                writer.Write(reader.model.vertices[i].boneIndices_3);
            }

            jointBuffer.byteOffset = (uint)writer.BaseStream.Position - jointBuffer.byteOffset;

            var jointLoc = accessorInfo.Count();

            accessorInfo.Add(new Accessor()
            {
                name = "vJoint",
                bufferView = bufferViews.Count(),
                byteOffset = 0,
                componentType = 5121,
                count = (uint)reader.model.vertices.Count(),
                type = "VEC4"
            });

            bufferViews.Add(jointBuffer);

            // Weight bufferview
            var weightBuffer = new BufferView()
            {
                buffer = 0,
                byteOffset = (uint)writer.BaseStream.Position,
                target = 34962
            };

            for (var i = 0; i < reader.model.vertices.Count(); i++)
            {
                writer.Write(reader.model.vertices[i].boneWeight_0);
                writer.Write(reader.model.vertices[i].boneWeight_1);
                writer.Write(reader.model.vertices[i].boneWeight_2);
                writer.Write(reader.model.vertices[i].boneWeight_3);
            }

            weightBuffer.byteOffset = (uint)writer.BaseStream.Position - weightBuffer.byteOffset;

            var weightLoc = accessorInfo.Count();

            accessorInfo.Add(new Accessor()
            {
                name = "vWeight",
                bufferView = bufferViews.Count(),
                byteOffset = 0,
                componentType = 5121,
                count = (uint)reader.model.vertices.Count(),
                type = "VEC4"
            });

            bufferViews.Add(weightBuffer);

            // End of element bufferviews
            var indexBufferPos = bufferViews.Count();
            var materialBlends = new Dictionary<int, ushort>();

            for (var i = 0; i < reader.model.skins[0].submeshes.Count(); i++)
            {
                var batch = reader.model.skins[0].submeshes[i];

                accessorInfo.Add(new Accessor()
                {
                    name = "indices",
                    bufferView = indexBufferPos,
                    byteOffset = reader.model.skins[0].submeshes[i].startTriangle * 2,
                    componentType = 5123,
                    count = reader.model.skins[0].submeshes[i].nTriangles,
                    type = "SCALAR"
                });

                var mesh = new Mesh();
                mesh.name = "Group #" + i;
                mesh.primitives = new Primitive[1];
                mesh.primitives[0].attributes = new Dictionary<string, int>
                    {
                        { "POSITION", posLoc },
                        { "NORMAL", normalLoc },
                        { "TEXCOORD_0", texLoc },
                        { "JOINTS_0", jointLoc },
                        { "WEIGHTS_0", weightLoc }
                    };

                mesh.primitives[0].indices = (uint)accessorInfo.Count() - 1;
                mesh.primitives[0].mode = 4;

                meshes.Add(mesh);
                // Texture stuff
                for (var tu = 0; tu < reader.model.skins[0].textureunit.Count(); tu++)
                {
                    if (reader.model.skins[0].textureunit[tu].submeshIndex == i)
                    {
                        mesh.primitives[0].material = reader.model.texlookup[reader.model.skins[0].textureunit[tu].texture].textureID;

                        // todo
                        if (!materialBlends.ContainsKey(i))
                        {
                            // add texture 
                            materialBlends.Add(i, reader.model.renderflags[reader.model.skins[0].textureunit[tu].renderFlags].blendingMode);
                        }
                        else
                        {
                            // already exists
                            Logger.WriteLine("Material "+ mesh.primitives[0].material + " already exists in blend map with value " + materialBlends[i]);
                        }
                    }
                }
            }

            var indiceBuffer = new BufferView()
            {
                buffer = 0,
                byteOffset = (uint)writer.BaseStream.Position,
                target = 34963
            };

            for (var i = 0; i < reader.model.skins[0].triangles.Count(); i++)
            {
                var t = reader.model.skins[0].triangles[i];
                writer.Write(t.pt1);
                writer.Write(t.pt2);
                writer.Write(t.pt3);
            }

            indiceBuffer.byteLength = (uint)writer.BaseStream.Position - indiceBuffer.byteOffset;
            bufferViews.Add(indiceBuffer);

            glTF.bufferViews = bufferViews.ToArray();
            glTF.accessors = accessorInfo.ToArray();

            glTF.buffers = new Buffer[1];
            glTF.buffers[0].byteLength = (uint)writer.BaseStream.Length;
            glTF.buffers[0].uri = Path.GetFileNameWithoutExtension(file) + ".bin";

            writer.Close();
            writer.Dispose();

            exportworker.ReportProgress(65, "Exporting textures..");

            var materialCount = reader.model.textures.Count();

            glTF.images = new Image[materialCount];
            glTF.textures = new Texture[materialCount];
            glTF.materials = new Material[materialCount];

            var textureID = 0;
            var materials = new Structs.Material[reader.model.textures.Count()];

            for (var i = 0; i < reader.model.textures.Count(); i++)
            {
                var textureFileDataID = 840426;
                materials[i].flags = reader.model.textures[i].flags;
                switch (reader.model.textures[i].type)
                {
                    case 0:
                        textureFileDataID = CASC.getFileDataIdByName(reader.model.textures[i].filename);
                        break;
                    case 1:
                    case 2:
                    case 11:
                        var fileDataID = CASC.getFileDataIdByName(file);
                        var cdifilenames = WoWFormatLib.DBC.DBCHelper.getTexturesByModelFilename(fileDataID, (int)reader.model.textures[i].type);
                        for (var ti = 0; ti < cdifilenames.Count(); ti++)
                        {
                            textureFileDataID = (int)cdifilenames[0];
                        }
                        break;
                    default:
                        Console.WriteLine("      Falling back to placeholder texture");
                        break;
                }

                materials[i].textureID = textureID + i;

                materials[i].filename = textureFileDataID.ToString();

                glTF.materials[i].name = materials[i].filename;
                glTF.materials[i].pbrMetallicRoughness = new PBRMetallicRoughness();
                glTF.materials[i].pbrMetallicRoughness.baseColorTexture = new TextureIndex();
                glTF.materials[i].pbrMetallicRoughness.baseColorTexture.index = i;
                glTF.materials[i].pbrMetallicRoughness.metallicFactor = 0.0f;

                glTF.materials[i].alphaMode = "MASK";
                glTF.materials[i].alphaCutoff = 0.5f;

                glTF.images[i].uri = "tex_" + materials[i].filename + ".png";
                glTF.textures[i].sampler = 0;
                glTF.textures[i].source = i;

                var blpreader = new BLPReader();
                blpreader.LoadBLP(textureFileDataID);
                
                try
                {
                    if (destinationOverride == null)
                    {
                        blpreader.bmp.Save(Path.Combine(outdir, Path.GetDirectoryName(file), glTF.images[i].uri));
                    }
                    else
                    {
                        blpreader.bmp.Save(Path.Combine(outdir, destinationOverride, glTF.images[i].uri));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            exportworker.ReportProgress(85, "Writing files..");

            glTF.samplers = new Sampler[1];
            glTF.samplers[0].name = "Default Sampler";
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

            if (destinationOverride == null)
            {
                File.WriteAllText(Path.Combine(outdir, file.Replace(".m2", ".gltf")), JsonConvert.SerializeObject(glTF, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                }));
            }
            else
            {
                File.WriteAllText(Path.Combine(destinationOverride, Path.GetFileName(file.ToLower()).Replace(".m2", ".gltf")), JsonConvert.SerializeObject(glTF, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                }));
            }
            /*
             * objsw.WriteLine("g " + Path.GetFileNameWithoutExtension(file));

            foreach (var renderbatch in renderbatches)
            {
                var i = renderbatch.firstFace;
                objsw.WriteLine("o " + Path.GetFileNameWithoutExtension(file) + renderbatch.groupID);
                objsw.WriteLine("usemtl tex_" + materials[renderbatch.materialID].filename);
                objsw.WriteLine("s 1");
                while (i < (renderbatch.firstFace + renderbatch.numFaces))
                {
                    objsw.WriteLine("f " + (indices[i] + 1) + "/" + (indices[i] + 1) + "/" + (indices[i] + 1) + " " + (indices[i + 1] + 1) + "/" + (indices[i + 1] + 1) + "/" + (indices[i + 1] + 1) + " " + (indices[i + 2] + 1) + "/" + (indices[i + 2] + 1) + "/" + (indices[i + 2] + 1));
                    i = i + 3;
                }
            }

            objsw.Close();

            // Only export phys when exporting a single M2, causes issues for some users when combined with WMO/ADT
            if (destinationOverride == null)
            {
                exportworker.ReportProgress(90, "Exporting collision..");

                objsw = new StreamWriter(Path.Combine(outdir, file.Replace(".m2", ".phys.obj")));

                objsw.WriteLine("# Written by Marlamin's WoW Exporter. Original file: " + file);

                for (int i = 0; i < reader.model.boundingvertices.Count(); i++)
                {
                    objsw.WriteLine("v " +
                         reader.model.boundingvertices[i].vertex.X + " " +
                         reader.model.boundingvertices[i].vertex.Z + " " +
                        -reader.model.boundingvertices[i].vertex.Y);
                }

                for (int i = 0; i < reader.model.boundingtriangles.Count(); i++)
                {
                    var t = reader.model.boundingtriangles[i];
                    objsw.WriteLine("f " + (t.index_0 + 1) + " " + (t.index_1 + 1) + " " + (t.index_2 + 1));
                }

                objsw.Close();
            }

            // https://en.wikipedia.org/wiki/Wavefront_.obj_file#Basic_materials
            // http://wiki.unity3d.com/index.php?title=ExportOBJ
            // http://web.cse.ohio-state.edu/~hwshen/581/Site/Lab3_files/Labhelp_Obj_parser.htm
            */
        }
    }
}
