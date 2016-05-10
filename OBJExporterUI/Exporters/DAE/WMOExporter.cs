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
using Collada141;
using System.Xml;

namespace OBJExporterUI.Exporters.DAE
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
            for (int i = 0; i < reader.wmofile.doodadNames.Count(); i++)
            {
                //Console.WriteLine(reader.wmofile.doodadNames[i].filename);
                //reader.wmofile.doodadDefinitions[i].
                //reader.wmofile.doodadDefinitions[i].
            }

            exportworker.ReportProgress(30, "Reading WMO..");

            uint totalVertices = 0;

            var groups = new Structs.WMOGroup[reader.wmofile.group.Count()];

            for (int g = 0; g < reader.wmofile.group.Count(); g++)
            {
                if (reader.wmofile.group[g].mogp.vertices == null) { continue; }
                for (int i = 0; i < reader.wmofile.groupNames.Count(); i++)
                {
                    if (reader.wmofile.group[g].mogp.nameOffset == reader.wmofile.groupNames[i].offset)
                    {
                        groups[g].name = reader.wmofile.groupNames[i].name.Replace(" ", "_");
                    }
                }

                if (groups[g].name == "antiportal") { continue; }

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

                        textureID++;
                    }
                }
            }

            //No idea how MTL files really work yet. Needs more investigation.
            foreach (var material in materials)
            {
                /*mtlsb.Append("newmtl " + material.filename + "\n");
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
                }*/
            }

            //File.WriteAllText(Path.Combine(outdir, file.Replace(".wmo", ".mtl")), mtlsb.ToString());

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

            COLLADA model = new COLLADA();

            /* Set up asset information */
            model.asset = new asset();
            model.asset.contributor = new assetContributor[1];
            model.asset.contributor[0] = new assetContributor();
            model.asset.contributor[0].authoring_tool = "Marlamin's WoW Format Exporter";
            model.asset.contributor[0].copyright = "Blizzard Entertainment";
            model.asset.contributor[0].comments = "Exported from World of Warcraft";
            model.asset.contributor[0].source_data = file;
            model.asset.created = DateTime.Now;
            model.asset.modified = DateTime.Now;
            model.asset.up_axis = UpAxisType.Y_UP;
            model.asset.unit = new assetUnit();
            model.asset.unit.meter = 1;
            model.asset.unit.name = "meter";

            var geometries = new List<geometry>();

            var groupc = 0;
            foreach (var group in groups)
            {
                groupc++;
                if (group.vertices == null) { continue; }

                var geometree = new geometry();

                geometree.id = "group_" + groupc.ToString();
                geometree.name = group.name;

                var mesh = new mesh();

                mesh.vertices = new vertices();
                mesh.vertices.id = geometree.id + "_vertices";
                mesh.vertices.input = new InputLocal[1];
                mesh.vertices.input[0] = new InputLocal() { semantic = "POSITION", source = "#" + mesh.vertices.id + "_positions_array" };

                var positions = new source();
                positions.id = mesh.vertices.id + "_positions";
                positions.name = "position";

                var floatArray = new float_array();
                floatArray.id = positions.id + "_array";
                floatArray.count = (ulong) group.vertices.Count() * 3;
                floatArray.Values = new double[group.vertices.Count() * 3];

                var i = 0;
                foreach (var vertex in group.vertices)
                {
                    floatArray.Values[i] = vertex.Position.X;
                    floatArray.Values[i + 1] = vertex.Position.Y;
                    floatArray.Values[i + 2] = vertex.Position.Z;
                    i = i + 3;
                }

                positions.Item = floatArray;

                positions.technique_common = new sourceTechnique_common();
                positions.technique_common.accessor = new accessor();
                positions.technique_common.accessor.source = "#" + positions.id + "_array";
                positions.technique_common.accessor.offset = 0;
                positions.technique_common.accessor.stride = 3;
                positions.technique_common.accessor.count = (ulong) group.vertices.Count();
                positions.technique_common.accessor.param = new param[3];
                positions.technique_common.accessor.param[0] = new param();
                positions.technique_common.accessor.param[0].name = "X";
                positions.technique_common.accessor.param[0].type = "float";
                positions.technique_common.accessor.param[1] = new param();
                positions.technique_common.accessor.param[1].name = "Y";
                positions.technique_common.accessor.param[1].type = "float";
                positions.technique_common.accessor.param[2] = new param();
                positions.technique_common.accessor.param[2].name = "Z";
                positions.technique_common.accessor.param[2].type = "float";

                mesh.source = new source[] {
                    positions
                };

                var polylist = new polylist();
                polylist.input = new InputLocalOffset[] {
                    new InputLocalOffset() { semantic = "VERTEX", source = "#" + geometree.id + "_vertices_positions", offset=0 }
                };

                var totalCount = 0;

                foreach (var renderbatch in group.renderBatches)
                {
                    polylist.vcount += string.Join("3 ", new string[renderbatch.numFaces + 1]).Trim();

                    var j = renderbatch.firstFace;

                    if (renderbatch.numFaces > 0)
                    {
                        while (j < (renderbatch.firstFace + renderbatch.numFaces))
                        {
                            polylist.p += String.Format("{0} {1} {2} ", group.indices[j], group.indices[j + 1], group.indices[j + 2]);
                            j = j + 3;
                        }
                    }

                    totalCount = totalCount + (int) renderbatch.numFaces;
                }

                polylist.count = (ulong) totalCount;

                mesh.Items = new object[]{
                    polylist
                };

                geometree.Item = mesh;

                geometries.Add(geometree);
            }

            model.Items = new object[]
            {
                new library_geometries()
                {
                    geometry = geometries.ToArray()
                }
            };

            model.Save(@"D:\MODELS\test.dae");

           // var contents = File.ReadAllText(@"D:\MODELS\test.dae");
           // contents = contents.Replace("<accessor", "<accessor offset=\"0\"");
           // File.WriteAllText(@"D:\MODELS\test.dae", contents);
            Console.WriteLine("Done loading WMO file!");
        }
    }
}
