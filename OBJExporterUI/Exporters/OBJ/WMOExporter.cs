using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using WoWFormatLib.FileReaders;
using OpenTK;
using System.IO;

namespace OBJExporterUI.Exporters.OBJ
{
    public class WMOExporter
    {
        public static void exportWMO(string file, BackgroundWorker exportworker = null, string destinationOverride = null, ushort doodadSetExportID = ushort.MaxValue)
        {
            exportWMO(WoWFormatLib.Utils.CASC.getFileDataIdByName(file), exportworker, destinationOverride, doodadSetExportID, file);
        }

        public static void exportWMO(uint filedataid, BackgroundWorker exportworker = null, string destinationOverride = null, ushort doodadSetExportID = ushort.MaxValue, string filename = "")
        {
            if (exportworker == null)
            {
                exportworker = new BackgroundWorker();
                exportworker.WorkerReportsProgress = true;
            }

            if (string.IsNullOrEmpty(filename))
            {
                var lookup = WoWFormatLib.Utils.CASC.getHashByFileDataID(filedataid);
                MainWindow.filenameLookup.TryGetValue(lookup, out filename);
            }

            Console.WriteLine("Loading WMO file..");

            exportworker.ReportProgress(5, "Reading WMO..");

            var outdir = ConfigurationManager.AppSettings["outdir"];
            var wmo = new WMOReader().LoadWMO(filedataid);

            var customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            exportworker.ReportProgress(30, "Reading WMO..");

            uint totalVertices = 0;

            var groups = new Structs.WMOGroup[wmo.group.Count()];

            for (var g = 0; g < wmo.group.Count(); g++)
            {
                Console.WriteLine("Loading group #" + g);
                if (wmo.group[g].mogp.vertices == null)
                { Console.WriteLine("Group has no vertices!"); continue; }
                for (var i = 0; i < wmo.groupNames.Count(); i++)
                {
                    if (wmo.group[g].mogp.nameOffset == wmo.groupNames[i].offset)
                    {
                        groups[g].name = wmo.groupNames[i].name.Replace(" ", "_");
                    }
                }

                if (groups[g].name == "antiportal")
                { Console.WriteLine("Group is antiportal"); continue; }

                groups[g].verticeOffset = totalVertices;
                groups[g].vertices = new Structs.Vertex[wmo.group[g].mogp.vertices.Count()];

                for (var i = 0; i < wmo.group[g].mogp.vertices.Count(); i++)
                {
                    groups[g].vertices[i].Position = new Vector3(wmo.group[g].mogp.vertices[i].vector.X * -1, wmo.group[g].mogp.vertices[i].vector.Z, wmo.group[g].mogp.vertices[i].vector.Y);
                    groups[g].vertices[i].Normal = new Vector3(wmo.group[g].mogp.normals[i].normal.X, wmo.group[g].mogp.normals[i].normal.Z, wmo.group[g].mogp.normals[i].normal.Y);
                    groups[g].vertices[i].TexCoord = new Vector2(wmo.group[g].mogp.textureCoords[0][i].X, wmo.group[g].mogp.textureCoords[0][i].Y);
                    totalVertices++;
                }

                var indicelist = new List<uint>();

                for (var i = 0; i < wmo.group[g].mogp.indices.Count(); i++)
                {
                    indicelist.Add(wmo.group[g].mogp.indices[i].indice);
                }

                groups[g].indices = indicelist.ToArray();
            }

            if (destinationOverride == null)
            {
                // Create output directory
                if (!string.IsNullOrEmpty(filename))
                {
                    if (!Directory.Exists(Path.Combine(outdir, Path.GetDirectoryName(filename))))
                    {
                        Directory.CreateDirectory(Path.Combine(outdir, Path.GetDirectoryName(filename)));
                    }
                }
                else
                {
                    if (!Directory.Exists(outdir))
                    {
                        Directory.CreateDirectory(outdir);
                    }
                }
            }

            StreamWriter doodadSW;

            if (destinationOverride == null)
            {
                if (!string.IsNullOrEmpty(filename))
                {
                    doodadSW = new StreamWriter(Path.Combine(outdir, Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename.Replace(" ", "")) + "_ModelPlacementInformation.csv"));
                }
                else
                {
                    doodadSW = new StreamWriter(Path.Combine(outdir, Path.GetDirectoryName(filename), filedataid + "_ModelPlacementInformation.csv"));
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(filename))
                {
                    doodadSW = new StreamWriter(Path.Combine(outdir, destinationOverride, Path.GetFileNameWithoutExtension(filename).Replace(" ", "") + "_ModelPlacementInformation.csv"));
                }
                else
                {
                    doodadSW = new StreamWriter(Path.Combine(outdir, destinationOverride, filedataid + "_ModelPlacementInformation.csv"));
                }
            }

            exportworker.ReportProgress(55, "Exporting doodads..");

            doodadSW.WriteLine("ModelFile;PositionX;PositionY;PositionZ;RotationW;RotationX;RotationY;RotationZ;ScaleFactor;DoodadSet");

            for (var i = 0; i < wmo.doodadSets.Count(); i++)
            {
                var doodadSet = wmo.doodadSets[i];

                var currentDoodadSetName = doodadSet.setName.Replace("Set_", "").Replace("SET_", "").Replace("$DefaultGlobal", "Default");

                if (doodadSetExportID != ushort.MaxValue)
                {
                    //if (i != 0 && i != doodadSetExportID) // Is 0 always exported? Double check?
                    if (i != doodadSetExportID)
                    {
                        Console.WriteLine("Skipping doodadset with ID " + i + " (" + currentDoodadSetName + ") because export filter is set to " + doodadSetExportID);
                        continue;
                    }
                }

                Console.WriteLine("At doodadset " + i + " (" + currentDoodadSetName + ")");

                for (var j = doodadSet.firstInstanceIndex; j < (doodadSet.firstInstanceIndex + doodadSet.numDoodads); j++)
                {
                    var doodadDefinition = wmo.doodadDefinitions[j];

                    var doodadFilename = "";
                    uint doodadFileDataID = 0;

                    if (wmo.doodadIds != null)
                    {
                        doodadFileDataID = wmo.doodadIds[doodadDefinition.offset];
                        var lookup = WoWFormatLib.Utils.CASC.getHashByFileDataID(doodadFileDataID);
                        MainWindow.filenameLookup.TryGetValue(lookup, out doodadFilename);
                    }
                    else
                    {
                        CASCLib.Logger.WriteLine("Warning!! File " + filename + " ID: " + filedataid + " still has filenames!");
                        foreach (var doodadNameEntry in wmo.doodadNames)
                        {
                            if (doodadNameEntry.startOffset == doodadDefinition.offset)
                            {
                                doodadFilename = doodadNameEntry.filename.Replace(".MDX", ".M2").Replace(".MDL", ".M2").ToLower();
                                doodadFileDataID = WoWFormatLib.Utils.CASC.getFileDataIdByName(doodadFilename);
                            }
                        }
                    }

                    if (destinationOverride == null)
                    {
                        if (!string.IsNullOrEmpty(doodadFilename))
                        {
                            if (!File.Exists(Path.Combine(outdir, Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(doodadFilename) + ".obj")))
                            {
                                M2Exporter.ExportM2(doodadFileDataID, null, Path.Combine(outdir, Path.GetDirectoryName(filename)), doodadFilename);
                            }

                            if (File.Exists(Path.Combine(outdir, Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(doodadFilename) + ".obj")))
                            {
                                doodadSW.WriteLine(Path.GetFileNameWithoutExtension(doodadFilename) + ".obj;" + doodadDefinition.position.X.ToString("F09") + ";" + doodadDefinition.position.Y.ToString("F09") + ";" + doodadDefinition.position.Z.ToString("F09") + ";" + doodadDefinition.rotation.W.ToString("F15") + ";" + doodadDefinition.rotation.X.ToString("F15") + ";" + doodadDefinition.rotation.Y.ToString("F15") + ";" + doodadDefinition.rotation.Z.ToString("F15") + ";" + doodadDefinition.scale + ";" + currentDoodadSetName);
                            }
                        }
                        else
                        {
                            if (!File.Exists(Path.Combine(outdir, doodadFileDataID + ".obj")))
                            {
                                M2Exporter.ExportM2(doodadFileDataID, null, outdir, doodadFilename);
                            }

                            if (File.Exists(Path.Combine(outdir, doodadFileDataID + ".obj")))
                            {
                                doodadSW.WriteLine(doodadFileDataID + ".obj;" + doodadDefinition.position.X.ToString("F09") + ";" + doodadDefinition.position.Y.ToString("F09") + ";" + doodadDefinition.position.Z.ToString("F09") + ";" + doodadDefinition.rotation.W.ToString("F15") + ";" + doodadDefinition.rotation.X.ToString("F15") + ";" + doodadDefinition.rotation.Y.ToString("F15") + ";" + doodadDefinition.rotation.Z.ToString("F15") + ";" + doodadDefinition.scale + ";" + currentDoodadSetName);
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(doodadFilename))
                        {
                            if (!File.Exists(Path.Combine(destinationOverride, Path.GetFileNameWithoutExtension(doodadFilename) + ".obj")))
                            {
                                M2Exporter.ExportM2(doodadFileDataID, null, destinationOverride, doodadFilename);
                            }

                            if (File.Exists(Path.Combine(destinationOverride, Path.GetFileNameWithoutExtension(doodadFilename) + ".obj")))
                            {
                                doodadSW.WriteLine(Path.GetFileNameWithoutExtension(doodadFilename) + ".obj;" + doodadDefinition.position.X.ToString("F09") + ";" + doodadDefinition.position.Y.ToString("F09") + ";" + doodadDefinition.position.Z.ToString("F09") + ";" + doodadDefinition.rotation.W.ToString("F15") + ";" + doodadDefinition.rotation.X.ToString("F15") + ";" + doodadDefinition.rotation.Y.ToString("F15") + ";" + doodadDefinition.rotation.Z.ToString("F15") + ";" + doodadDefinition.scale + ";" + currentDoodadSetName);
                            }
                        }
                        else
                        {
                            if (!File.Exists(Path.Combine(destinationOverride, doodadFileDataID + ".obj")))
                            {
                                M2Exporter.ExportM2(doodadFileDataID, null, destinationOverride, doodadFilename);
                            }

                            if (File.Exists(Path.Combine(destinationOverride, doodadFileDataID + ".obj")))
                            {
                                doodadSW.WriteLine(doodadFileDataID + ".obj;" + doodadDefinition.position.X.ToString("F09") + ";" + doodadDefinition.position.Y.ToString("F09") + ";" + doodadDefinition.position.Z.ToString("F09") + ";" + doodadDefinition.rotation.W.ToString("F15") + ";" + doodadDefinition.rotation.X.ToString("F15") + ";" + doodadDefinition.rotation.Y.ToString("F15") + ";" + doodadDefinition.rotation.Z.ToString("F15") + ";" + doodadDefinition.scale + ";" + currentDoodadSetName);
                            }
                        }
                    }
                }
            }

            doodadSW.Close();

            exportworker.ReportProgress(65, "Exporting textures..");

            var mtlsb = new StringBuilder();
            var textureID = 0;

            if (wmo.materials == null)
            {
                CASCLib.Logger.WriteLine("Unable to find materials for WMO " + filedataid + ", not exporting!");
                return;
            }

            var materials = new Structs.Material[wmo.materials.Count()];

            for (var i = 0; i < wmo.materials.Count(); i++)
            {
                var blpreader = new BLPReader();

                if (wmo.textures == null)
                {
                    var lookup = WoWFormatLib.Utils.CASC.getHashByFileDataID(wmo.materials[i].texture1);
                    if(MainWindow.filenameLookup.TryGetValue(lookup, out var textureFilename))
                    {
                        materials[i].filename = Path.GetFileNameWithoutExtension(textureFilename);
                    }
                    else
                    {
                        materials[i].filename = wmo.materials[i].texture1.ToString();
                    }

                    blpreader.LoadBLP(wmo.materials[i].texture1);
                }
                else
                {
                    for (var ti = 0; ti < wmo.textures.Count(); ti++)
                    {
                        if (wmo.textures[ti].startOffset == wmo.materials[i].texture1)
                        {
                            materials[i].filename = Path.GetFileNameWithoutExtension(wmo.textures[ti].filename);
                            blpreader.LoadBLP(wmo.textures[ti].filename);
                        }
                    }
                }

                materials[i].textureID = textureID + i;

                if (wmo.materials[i].blendMode == 0)
                {
                    materials[i].transparent = false;
                }
                else
                {
                    materials[i].transparent = true;
                }

                materials[i].blendMode = wmo.materials[i].blendMode;
                materials[i].shaderID = wmo.materials[i].shader;
                materials[i].terrainType = wmo.materials[i].groundType;

                var saveLocation = "";

                if (destinationOverride == null)
                {
                    if (!string.IsNullOrEmpty(filename))
                    {
                        saveLocation = Path.Combine(outdir, Path.GetDirectoryName(filename), materials[i].filename + ".png");
                    }
                    else
                    {
                        saveLocation = Path.Combine(outdir, materials[i].filename + ".png");
                    }
                }
                else
                {
                    saveLocation = Path.Combine(outdir, destinationOverride, materials[i].filename + ".png");
                }

                if (!File.Exists(saveLocation))
                {
                    try
                    {
                        blpreader.bmp.Save(saveLocation);
                    }
                    catch (Exception e)
                    {
                        CASCLib.Logger.WriteLine("Exception while saving BLP " + materials[i].filename + ": " + e.Message);
                    }
                }

                textureID++;
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
                mtlsb.Append("blend " + material.blendMode + "\n");
                mtlsb.Append("shader " + material.shaderID + "\n");
                mtlsb.Append("terrain " + material.terrainType + "\n");
            }

            if (!string.IsNullOrEmpty(filename))
            {
                if (destinationOverride == null)
                {
                    File.WriteAllText(Path.Combine(outdir, filename.Replace(".wmo", ".mtl")), mtlsb.ToString());
                }
                else
                {
                    File.WriteAllText(Path.Combine(outdir, destinationOverride, Path.GetFileName(filename.ToLower()).Replace(".wmo", ".mtl")), mtlsb.ToString());
                }
            }
            else
            {
                if (destinationOverride == null)
                {
                    File.WriteAllText(Path.Combine(outdir, filedataid + ".mtl"), mtlsb.ToString());
                }
                else
                {
                    File.WriteAllText(Path.Combine(outdir, destinationOverride, filedataid + ".mtl"), mtlsb.ToString());
                }
            }

            exportworker.ReportProgress(75, "Exporting model..");

            var numRenderbatches = 0;
            //Get total amount of render batches
            for (var i = 0; i < wmo.group.Count(); i++)
            {
                if (wmo.group[i].mogp.renderBatches == null)
                {
                    continue;
                }
                numRenderbatches = numRenderbatches + wmo.group[i].mogp.renderBatches.Count();
            }


            var rb = 0;
            for (var g = 0; g < wmo.group.Count(); g++)
            {
                groups[g].renderBatches = new Structs.RenderBatch[numRenderbatches];

                var group = wmo.group[g];
                if (group.mogp.renderBatches == null)
                {
                    continue;
                }

                for (var i = 0; i < group.mogp.renderBatches.Count(); i++)
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
                    groups[g].renderBatches[rb].blendType = wmo.materials[batch.materialID].blendMode;
                    groups[g].renderBatches[rb].groupID = (uint)g;
                    rb++;
                }
            }

            exportworker.ReportProgress(95, "Writing files..");

            StreamWriter objsw;
            if (!string.IsNullOrEmpty(filename))
            {
                if (destinationOverride == null)
                {
                    objsw = new StreamWriter(Path.Combine(outdir, filename.Replace(".wmo", ".obj")));
                }
                else
                {
                    objsw = new StreamWriter(Path.Combine(outdir, destinationOverride, Path.GetFileName(filename.ToLower()).Replace(".wmo", ".obj")));
                }

                objsw.WriteLine("# Written by Marlamin's WoW OBJExporter. Original file: " + filename);
                objsw.WriteLine("mtllib " + Path.GetFileNameWithoutExtension(filename) + ".mtl");
            }
            else
            {
                if (destinationOverride == null)
                {
                    objsw = new StreamWriter(Path.Combine(outdir, filedataid + ".obj"));
                }
                else
                {
                    objsw = new StreamWriter(Path.Combine(outdir, destinationOverride, filedataid + ".obj"));
                }

                objsw.WriteLine("# Written by Marlamin's WoW OBJExporter. Original file id: " + filedataid);
                objsw.WriteLine("mtllib " + filedataid + ".mtl");
            }

            foreach (var group in groups)
            {
                if (group.vertices == null)
                {
                    continue;
                }
                Console.WriteLine("Writing " + group.name);
                objsw.WriteLine("g " + group.name);

                foreach (var vertex in group.vertices)
                {
                    objsw.WriteLine("v " + vertex.Position.X + " " + vertex.Position.Y + " " + vertex.Position.Z);
                    objsw.WriteLine("vt " + vertex.TexCoord.X + " " + -vertex.TexCoord.Y);
                    objsw.WriteLine("vn " + vertex.Normal.X.ToString("F12") + " " + vertex.Normal.Y.ToString("F12") + " " + vertex.Normal.Z.ToString("F12"));
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
