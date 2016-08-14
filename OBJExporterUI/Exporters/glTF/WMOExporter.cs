using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWFormatLib.FileReaders;
using OpenTK;
using System.IO;

namespace OBJExporterUI.Exporters.glTF
{
    public class WMOExporter
    {
        public static void exportWMO(string file, BackgroundWorker exportworker = null)
        {
            if(exportworker == null)
            {
                exportworker = new BackgroundWorker();
            }

            Console.WriteLine("Loading WMO file..");

            exportworker.ReportProgress(5, "Reading WMO..");

            var outdir = ConfigurationManager.AppSettings["outdir"];
            WMOReader reader = new WMOReader();
            reader.LoadWMO(file);

            // TODO: Support doodads!
            /*
            for (int i = 0; i < reader.wmofile.doodadNames.Count(); i++)
            {
                //Console.WriteLine(reader.wmofile.doodadNames[i].filename);
                //reader.wmofile.doodadDefinitions[i].
                //reader.wmofile.doodadDefinitions[i].
            }
            */

            exportworker.ReportProgress(30, "Reading WMO..");

            uint totalVertices = 0;

            var groups = new Structs.WMOGroup[reader.wmofile.group.Count()];

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

                groups[g].verticeOffset = totalVertices;
                groups[g].vertices = new Structs.Vertex[reader.wmofile.group[g].mogp.vertices.Count()];

                for (int i = 0; i < reader.wmofile.group[g].mogp.vertices.Count(); i++)
                {
                    groups[g].vertices[i].Position = new Vector3(reader.wmofile.group[g].mogp.vertices[i].vector.X * -1, reader.wmofile.group[g].mogp.vertices[i].vector.Z, reader.wmofile.group[g].mogp.vertices[i].vector.Y);
                    groups[g].vertices[i].Normal = new Vector3(reader.wmofile.group[g].mogp.normals[i].normal.X, reader.wmofile.group[g].mogp.normals[i].normal.Z, reader.wmofile.group[g].mogp.normals[i].normal.Y);
                    groups[g].vertices[i].TexCoord = new Vector2(reader.wmofile.group[g].mogp.textureCoords[0][i].X, reader.wmofile.group[g].mogp.textureCoords[0][i].Y);
                    totalVertices++;
                }

                var indicelist = new List<uint>();

                for (int i = 0; i < reader.wmofile.group[g].mogp.indices.Count(); i++)
                {
                    indicelist.Add(reader.wmofile.group[g].mogp.indices[i].indice);
                }

                groups[g].indices = indicelist.ToArray();
            }

            exportworker.ReportProgress(55, "Exporting textures..");

            // Create output directory
            if (!Directory.Exists(Path.Combine(outdir, Path.GetDirectoryName(file))))
            {
                Directory.CreateDirectory(Path.Combine(outdir, Path.GetDirectoryName(file)));
            }

            var mtlsb = new StringBuilder();
            var textureID = 0;

            if (reader.wmofile.materials == null) { Console.WriteLine("Materials empty"); return; }
            var materials = new Structs.Material[reader.wmofile.materials.Count()];
            for (int i = 0; i < reader.wmofile.materials.Count(); i++)
            {
                for (int ti = 0; ti < reader.wmofile.textures.Count(); ti++)
                {
                    if (reader.wmofile.textures[ti].startOffset == reader.wmofile.materials[i].texture1)
                    {
                        //materials[i].textureID = BLPLoader.LoadTexture(reader.wmofile.textures[ti].filename, cache);
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
                                blpreader.bmp.Save(Path.Combine(outdir, Path.GetDirectoryName(file), materials[i].filename + ".png"));
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

            //No idea how MTL files really work yet. Needs more investigation.
            foreach (var material in materials)
            {
                mtlsb.Append("newmtl " + material.filename + "\n");
                mtlsb.Append("Ns 96.078431\n");
                mtlsb.Append("Ka 1.000000 1.000000 1.000000\n");
                mtlsb.Append("Kd 0.640000 0.640000 0.640000\n");
                mtlsb.Append("Ks 0.000000 0.000000 0.000000\n");
                mtlsb.Append("Ke 0.000000 0.000000 0.000000\n");
                mtlsb.Append("Ni 1.000000\n");
                mtlsb.Append("d 1.000000\n");
                mtlsb.Append("illum 2\n");
                mtlsb.Append("map_Kd " + material.filename + ".png\n");
                if (material.transparent)
                {
                    mtlsb.Append("map_d " + material.filename + ".png\n");
                }
            }

            File.WriteAllText(Path.Combine(outdir, file.Replace(".wmo", ".mtl")), mtlsb.ToString());

            exportworker.ReportProgress(75, "Exporting model..");

            int numRenderbatches = 0;
            //Get total amount of render batches
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

            var objsw = new StreamWriter(Path.Combine(outdir, file.Replace(".wmo", ".obj")));
            objsw.WriteLine("# Written by Marlamin's WoW OBJExporter. Original file: " + file);
            objsw.WriteLine("mtllib " + Path.GetFileNameWithoutExtension(file) + ".mtl");

            foreach (var group in groups)
            {
                if (group.vertices == null) { continue; }
                Console.WriteLine("Writing " + group.name);
                objsw.WriteLine("g " + group.name);

                foreach (var vertex in group.vertices)
                {
                    objsw.WriteLine("v " + vertex.Position.X + " " + vertex.Position.Y + " " + vertex.Position.Z);
                    objsw.WriteLine("vt " + vertex.TexCoord.X + " " + -vertex.TexCoord.Y);
                    objsw.WriteLine("vn " + vertex.Normal.X + " " + vertex.Normal.Y + " " + vertex.Normal.Z);
                }

                var indices = group.indices;

                foreach (var renderbatch in group.renderBatches)
                {
                    var i = renderbatch.firstFace;
                    if (renderbatch.numFaces > 0)
                    {
                        objsw.WriteLine("usemtl " + materials[renderbatch.materialID].filename);
                        objsw.WriteLine("s 1");
                        while (i < (renderbatch.firstFace + renderbatch.numFaces))
                        {
                            objsw.WriteLine("f " + (indices[i] + group.verticeOffset + 1) + "/" + (indices[i] + group.verticeOffset + 1) + "/" + (indices[i] + group.verticeOffset + 1) + " " + (indices[i + 1] + group.verticeOffset + 1) + "/" + (indices[i + 1] + group.verticeOffset + 1) + "/" + (indices[i + 1] + group.verticeOffset + 1) + " " + (indices[i + 2] + group.verticeOffset + 1) + "/" + (indices[i + 2] + group.verticeOffset + 1) + "/" + (indices[i + 2] + group.verticeOffset + 1));
                            i = i + 3;
                        }
                    }
                }
            }
            objsw.Close();
            Console.WriteLine("Done loading WMO file!");
        }
    }
}
