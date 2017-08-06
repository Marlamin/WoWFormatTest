using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using WoWFormatLib.FileReaders;
using OpenTK;
using System.IO;
using Newtonsoft.Json;

namespace OBJExporterUI.Exporters.glTF
{
    public class WMOExporter
    {
        public static void exportWMO(string file, BackgroundWorker exportworker = null, string destinationOverride = null)
        {
            if(exportworker == null)
            {
                exportworker = new BackgroundWorker();
                exportworker.WorkerReportsProgress = true;
            }

            Console.WriteLine("Loading WMO file..");

            exportworker.ReportProgress(5, "Reading WMO..");

            var outdir = ConfigurationManager.AppSettings["outdir"];
            WMOReader reader = new WMOReader();
            reader.LoadWMO(file);

            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            if (destinationOverride == null)
            {
                if (!Directory.Exists(Path.Combine(outdir, Path.GetDirectoryName(file))))
                {
                    Directory.CreateDirectory(Path.Combine(outdir, Path.GetDirectoryName(file)));
                }
            }

            exportworker.ReportProgress(30, "Reading WMO..");

            var glTF = new glTF()
            {
                asset = new Asset()
                {
                    version = "2.0",
                    generator = "Marlamin's WoW Exporter " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                    copyright = "Contents are owned by Blizzard Entertainment"
                }
            };

            uint totalVertices = 0;

            var groups = new Structs.WMOGroup[reader.wmofile.group.Count()];
            FileStream stream;

            if (destinationOverride == null)
            {
                stream = new FileStream(Path.Combine(outdir, file.Replace(".wmo", ".bin")), FileMode.OpenOrCreate);
            }
            else
            {
                stream = new FileStream(Path.Combine(outdir, destinationOverride, file.Replace(".wmo", ".bin")), FileMode.OpenOrCreate);
            }

            var writer = new BinaryWriter(stream);

            var bufferViews = new List<BufferView>();
            var accessorInfo = new List<Accessor>();

            for (int g = 0; g < reader.wmofile.group.Count(); g++)
            {
                Console.WriteLine("Loading group #" + g);
                if (reader.wmofile.group[g].mogp.vertices == null) { Console.WriteLine("Group has no vertices!");  continue; }
                for (int i = 0; i < reader.wmofile.groupNames.Count(); i++)
                {
                    if (reader.wmofile.group[g].mogp.nameOffset == reader.wmofile.groupNames[i].offset)
                    {
                        groups[g].name = reader.wmofile.groupNames[i].name.Replace(" ", "_");
                    }
                }

                if (groups[g].name == "antiportal") { Console.WriteLine("Group is antiportal"); continue; }

                // Position bufferview
                var vPosBuffer = new BufferView()
                {
                    buffer = 0,
                    byteOffset = (uint)writer.BaseStream.Position,
                    target = 34962
                };

                for (int i = 0; i < reader.wmofile.group[g].mogp.vertices.Count(); i++)
                {
                    writer.Write(reader.wmofile.group[g].mogp.vertices[i].vector.X * -1);
                    writer.Write(reader.wmofile.group[g].mogp.vertices[i].vector.Z);
                    writer.Write(reader.wmofile.group[g].mogp.vertices[i].vector.Y);
                }

                vPosBuffer.byteLength = (uint)writer.BaseStream.Position - vPosBuffer.byteOffset;

                accessorInfo.Add(new Accessor()
                {
                    bufferView = bufferViews.Count(),
                    byteOffset = 0,
                    componentType = 5126,
                    count = (uint)reader.wmofile.group[g].mogp.vertices.Count(),
                    type = "VEC3"
                });

                bufferViews.Add(vPosBuffer);

                // TexCoord bufferview
                var texCoordBuffer = new BufferView()
                {
                    buffer = 0,
                    byteOffset = (uint)writer.BaseStream.Position,
                    target = 34962
                };

                for (int i = 0; i < reader.wmofile.group[g].mogp.vertices.Count(); i++)
                {
                    writer.Write(reader.wmofile.group[g].mogp.textureCoords[0][i].X);
                    writer.Write(reader.wmofile.group[g].mogp.textureCoords[0][i].Y);
                }

                texCoordBuffer.byteLength = (uint)writer.BaseStream.Position - texCoordBuffer.byteOffset;

                accessorInfo.Add(new Accessor()
                {
                    bufferView = bufferViews.Count(),
                    byteOffset = 0,
                    componentType = 5126,
                    count = (uint)reader.wmofile.group[g].mogp.vertices.Count(),
                    type = "VEC2"
                });

                bufferViews.Add(texCoordBuffer);

                var indiceBuffer = new BufferView()
                {
                    buffer = 0,
                    byteOffset = (uint)writer.BaseStream.Position,
                    target = 34963
                };

                for (int i = 0; i < reader.wmofile.group[g].mogp.indices.Count(); i++)
                {
                    writer.Write(reader.wmofile.group[g].mogp.indices[i].indice);
                }

                indiceBuffer.byteLength = (uint)writer.BaseStream.Position - indiceBuffer.byteOffset;

                accessorInfo.Add(new Accessor()
                {
                    bufferView = bufferViews.Count(),
                    byteOffset = 0,
                    componentType = 5123,
                    count = (uint)reader.wmofile.group[g].mogp.indices.Count(),
                    type = "SCALAR"
                });

                bufferViews.Add(indiceBuffer);
            }

            glTF.bufferViews = bufferViews.ToArray();
            glTF.accessors = accessorInfo.ToArray();

            glTF.buffers = new Buffer[1];
            glTF.buffers[0].byteLength = (uint)writer.BaseStream.Length;
            glTF.buffers[0].uri = Path.GetFileNameWithoutExtension(file) + ".bin";

            writer.Close();
            writer.Dispose();

            exportworker.ReportProgress(55, "Exporting doodads..");

            exportworker.ReportProgress(65, "Exporting textures..");

            var textureID = 0;

            if (reader.wmofile.materials == null) { Console.WriteLine("Materials empty"); return; }
            var materials = new Structs.Material[reader.wmofile.materials.Count()];
            for (int i = 0; i < reader.wmofile.materials.Count(); i++)
            {
                for (int ti = 0; ti < reader.wmofile.textures.Count(); ti++)
                {
                    if (reader.wmofile.textures[ti].startOffset == reader.wmofile.materials[i].texture1)
                    {
                        materials[i].textureID = textureID + i;
                        materials[i].filename = Path.GetFileNameWithoutExtension(reader.wmofile.textures[ti].filename);

                        if (reader.wmofile.materials[i].blendMode == 0)
                        {
                            materials[i].transparent = false;
                        }
                        else
                        {
                            materials[i].transparent = true;
                        }

                        if (!File.Exists(Path.Combine(outdir, Path.GetDirectoryName(file), materials[i].filename + ".png"))){
                            var blpreader = new BLPReader();

                            blpreader.LoadBLP(reader.wmofile.textures[ti].filename);

                            try
                            {
                                if (destinationOverride == null)
                                {
                                    blpreader.bmp.Save(Path.Combine(outdir, Path.GetDirectoryName(file), materials[i].filename + ".png"));
                                }
                                else
                                {
                                    blpreader.bmp.Save(Path.Combine(outdir, destinationOverride, materials[i].filename.ToLower() + ".png"));
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }

                        textureID++;
                    }
                }
            }

            glTF.images = new Image[materials.Count()];
            for (int ti = 0; ti < reader.wmofile.textures.Count(); ti++)
            {
                glTF.images[ti].uri = Path.GetFileNameWithoutExtension(reader.wmofile.textures[ti].filename) + ".png";
            }

            exportworker.ReportProgress(75, "Exporting model..");

            int numRenderbatches = 0;
            for (int i = 0; i < reader.wmofile.group.Count(); i++)
            {
                if (reader.wmofile.group[i].mogp.renderBatches == null) { continue; }
                numRenderbatches = numRenderbatches + reader.wmofile.group[i].mogp.renderBatches.Count();
            }

            int rb = 0;
            for (int g = 0; g < reader.wmofile.group.Count(); g++)
            {
                groups[g].renderBatches = new Structs.RenderBatch[numRenderbatches];

                var group = reader.wmofile.group[g];
                if (group.mogp.renderBatches == null) { continue; }
                for (int i = 0; i < group.mogp.renderBatches.Count(); i++)
                {
                    var batch = group.mogp.renderBatches[i];

                    groups[g].renderBatches[rb].firstFace = batch.firstFace;
                    groups[g].renderBatches[rb].numFaces = batch.numFaces;

                    if (batch.flags == 2)
                    {
                        groups[g].renderBatches[rb].materialID = (uint)batch.possibleBox2_3;
                    }
                    else
                    {
                        groups[g].renderBatches[rb].materialID = batch.materialID;
                    }
                    groups[g].renderBatches[rb].blendType = reader.wmofile.materials[batch.materialID].blendMode;
                    groups[g].renderBatches[rb].groupID = (uint)g;
                    rb++;
                }
            }

            exportworker.ReportProgress(95, "Writing files..");

            if (destinationOverride == null)
            {
                File.WriteAllText(Path.Combine(outdir, file.Replace(".wmo", ".gltf")), JsonConvert.SerializeObject(glTF, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                }));
            }
            else
            {
                File.WriteAllText(Path.Combine(outdir, destinationOverride, Path.GetFileName(file.ToLower()).Replace(".wmo", ".gltf")), JsonConvert.SerializeObject(glTF, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                }));
            }

            Console.WriteLine("Done loading WMO file!");
        }
    }
}
