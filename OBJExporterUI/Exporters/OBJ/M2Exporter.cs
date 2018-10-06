using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using WoWFormatLib.FileReaders;
using WoWFormatLib.Utils;

namespace OBJExporterUI.Exporters.OBJ
{
    class M2Exporter
    {
        public static void ExportM2(string file, BackgroundWorker exportworker = null, string destinationOverride = null)
        {
            ExportM2(CASC.getFileDataIdByName(file), exportworker, destinationOverride, file);
        }

        public static void ExportM2(uint fileDataID, BackgroundWorker exportworker = null, string destinationOverride = null, string filename = "")
        {
            if (exportworker == null)
            {
                exportworker = new BackgroundWorker
                {
                    WorkerReportsProgress = true
                };
            }

            var customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            var outdir = ConfigurationManager.AppSettings["outdir"];
            var reader = new M2Reader();

            exportworker.ReportProgress(15, "Reading M2..");

            if (!CASC.FileExists(fileDataID)) { throw new Exception("404 M2 not found!"); }

            reader.LoadM2(fileDataID);

            // Don't export models without vertices
            if (reader.model.vertices.Count() == 0)
            {
                return;
            }

            var vertices = new Structs.Vertex[reader.model.vertices.Count()];

            for (var i = 0; i < reader.model.vertices.Count(); i++)
            {
                vertices[i].Position = new OpenTK.Vector3(reader.model.vertices[i].position.X, reader.model.vertices[i].position.Z, reader.model.vertices[i].position.Y * -1);
                vertices[i].Normal = new OpenTK.Vector3(reader.model.vertices[i].normal.X, reader.model.vertices[i].normal.Z, reader.model.vertices[i].normal.Y);
                vertices[i].TexCoord = new Vector2(reader.model.vertices[i].textureCoordX, reader.model.vertices[i].textureCoordY);
            }

            StreamWriter objsw;

            if(destinationOverride == null)
            {
                if (!string.IsNullOrEmpty(filename))
                {
                    if (!Directory.Exists(Path.Combine(outdir, Path.GetDirectoryName(filename))))
                    {
                        Directory.CreateDirectory(Path.Combine(outdir, Path.GetDirectoryName(filename)));
                    }

                    objsw = new StreamWriter(Path.Combine(outdir, filename.Replace(".m2", ".obj")));
                }
                else
                {
                    if (!Directory.Exists(outdir))
                    {
                        Directory.CreateDirectory(outdir);
                    }

                    objsw = new StreamWriter(Path.Combine(outdir, fileDataID + ".obj"));
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(filename))
                {
                    objsw = new StreamWriter(Path.Combine(outdir, destinationOverride, Path.GetFileName(filename.ToLower()).Replace(".m2", ".obj")));
                }
                else
                {
                    objsw = new StreamWriter(Path.Combine(outdir, destinationOverride, fileDataID + ".obj"));
                }
            }

            if (!string.IsNullOrEmpty(filename))
            {
                objsw.WriteLine("# Written by Marlamin's WoW Exporter. Original file: " + filename);
                objsw.WriteLine("mtllib " + Path.GetFileNameWithoutExtension(filename) + ".mtl");
            }
            else
            {
                objsw.WriteLine("# Written by Marlamin's WoW Exporter. Original fileDataID: " + fileDataID);
                objsw.WriteLine("mtllib " + fileDataID + ".mtl");
            }

            foreach (var vertex in vertices)
            {
                objsw.WriteLine("v " + vertex.Position.X + " " + vertex.Position.Y + " " + vertex.Position.Z);
                objsw.WriteLine("vt " + vertex.TexCoord.X + " " + -vertex.TexCoord.Y);
                objsw.WriteLine("vn " + vertex.Normal.X.ToString("F12") + " " + vertex.Normal.Y.ToString("F12") + " " + vertex.Normal.Z.ToString("F12"));
            }

            var indicelist = new List<uint>();
            for (var i = 0; i < reader.model.skins[0].triangles.Count(); i++)
            {
                var t = reader.model.skins[0].triangles[i];
                indicelist.Add(t.pt1);
                indicelist.Add(t.pt2);
                indicelist.Add(t.pt3);
            }

            var indices = indicelist.ToArray();
            exportworker.ReportProgress(35, "Writing files..");

            var renderbatches = new Structs.RenderBatch[reader.model.skins[0].submeshes.Count()];
            for (var i = 0; i < reader.model.skins[0].submeshes.Count(); i++)
            {
                if (!string.IsNullOrEmpty(filename) && filename.StartsWith("character", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (reader.model.skins[0].submeshes[i].submeshID != 0)
                    {
                        if (!reader.model.skins[0].submeshes[i].submeshID.ToString().EndsWith("01"))
                        {
                            continue;
                        }
                    }
                }

                renderbatches[i].firstFace = reader.model.skins[0].submeshes[i].startTriangle;
                renderbatches[i].numFaces = reader.model.skins[0].submeshes[i].nTriangles;
                renderbatches[i].groupID = (uint)i;
                for (var tu = 0; tu < reader.model.skins[0].textureunit.Count(); tu++)
                {
                    if (reader.model.skins[0].textureunit[tu].submeshIndex == i)
                    {
                        renderbatches[i].blendType = reader.model.renderflags[reader.model.skins[0].textureunit[tu].renderFlags].blendingMode;
                        renderbatches[i].materialID = reader.model.texlookup[reader.model.skins[0].textureunit[tu].texture].textureID;
                    }
                }
            }

            exportworker.ReportProgress(65, "Exporting textures..");

            StreamWriter mtlsb;

            if (destinationOverride == null)
            {
                if (!string.IsNullOrEmpty(filename))
                {
                    mtlsb = new StreamWriter(Path.Combine(outdir, filename.Replace(".m2", ".mtl")));
                }
                else
                {
                    mtlsb = new StreamWriter(Path.Combine(outdir, fileDataID + ".mtl"));
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(filename))
                {
                    mtlsb = new StreamWriter(Path.Combine(outdir, destinationOverride, Path.GetFileName(filename.ToLower()).Replace(".m2", ".mtl")));
                }
                else
                {
                    mtlsb = new StreamWriter(Path.Combine(outdir, destinationOverride, fileDataID + ".mtl"));
                }
            }

            var textureID = 0;
            var materials = new Structs.Material[reader.model.textures.Count()];

            for (var i = 0; i < reader.model.textures.Count(); i++)
            {
                uint textureFileDataID = 840426;
                materials[i].flags = reader.model.textures[i].flags;
                switch (reader.model.textures[i].type)
                {
                    case 0:
                        //Console.WriteLine("      Texture given in file!");
                        textureFileDataID = CASC.getFileDataIdByName(reader.model.textures[i].filename);
                        if(textureFileDataID == 372993)
                        {
                            textureFileDataID = reader.model.textureFileDataIDs[i];
                        }
                        break;
                    case 1:
                    case 2:
                    case 11:
                        var cdifilenames = WoWFormatLib.DBC.DBCHelper.getTexturesByModelFilename(fileDataID, (int)reader.model.textures[i].type);
                        for (var ti = 0; ti < cdifilenames.Count(); ti++)
                        {
                            textureFileDataID = cdifilenames[0];
                        }
                        break;
                    default:
                        Console.WriteLine("      Falling back to placeholder texture");
                        break;
                }

                materials[i].textureID = textureID + i;
                materials[i].filename = textureFileDataID.ToString();

                var blpreader = new BLPReader();

                blpreader.LoadBLP(textureFileDataID);

                try
                {
                    if (destinationOverride == null)
                    {
                        if (!string.IsNullOrEmpty(filename))
                        {
                            blpreader.bmp.Save(Path.Combine(outdir, Path.GetDirectoryName(filename), "tex_" + materials[i].filename + ".png"));
                        }
                        else
                        {
                            blpreader.bmp.Save(Path.Combine(outdir, "tex_" + materials[i].filename + ".png"));
                        }
                    }
                    else
                    {
                        blpreader.bmp.Save(Path.Combine(outdir, destinationOverride, "tex_" + materials[i].filename.ToLower() + ".png"));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            exportworker.ReportProgress(85, "Writing files..");

            foreach (var material in materials)
            {
                mtlsb.WriteLine("newmtl " + "tex_" + material.filename);
                mtlsb.WriteLine("illum 2");
                mtlsb.WriteLine("map_Ka " + "tex_" + material.filename + ".png");
                mtlsb.WriteLine("map_Kd " + "tex_" + material.filename + ".png");
            }

            mtlsb.Close();

            if (!string.IsNullOrEmpty(filename))
            {
                objsw.WriteLine("g " + Path.GetFileNameWithoutExtension(filename));
            }
            else
            {
                objsw.WriteLine("g " + fileDataID);
            }

            foreach (var renderbatch in renderbatches)
            {
                var i = renderbatch.firstFace;
                if (!string.IsNullOrEmpty(filename))
                {
                    objsw.WriteLine("o " + Path.GetFileNameWithoutExtension(filename) + renderbatch.groupID);
                }
                else
                {
                    objsw.WriteLine("g " + fileDataID.ToString() + renderbatch.groupID.ToString());
                }

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

                if (!string.IsNullOrEmpty(filename))
                {
                    objsw = new StreamWriter(Path.Combine(outdir, Path.GetFileName(filename.ToLower()).Replace(".m2", ".phys.obj")));
                }
                else
                {
                    objsw = new StreamWriter(Path.Combine(outdir, fileDataID + ".phys.obj"));
                }

                objsw.WriteLine("# Written by Marlamin's WoW Exporter. Original file id: " + fileDataID);

                for (var i = 0; i < reader.model.boundingvertices.Count(); i++)
                {
                    objsw.WriteLine("v " +
                         reader.model.boundingvertices[i].vertex.X + " " +
                         reader.model.boundingvertices[i].vertex.Z + " " +
                        -reader.model.boundingvertices[i].vertex.Y);
                }

                for (var i = 0; i < reader.model.boundingtriangles.Count(); i++)
                {
                    var t = reader.model.boundingtriangles[i];
                    objsw.WriteLine("f " + (t.index_0 + 1) + " " + (t.index_1 + 1) + " " + (t.index_2 + 1));
                }

                objsw.Close();
            }

            // https://en.wikipedia.org/wiki/Wavefront_.obj_file#Basic_materials
            // http://wiki.unity3d.com/index.php?title=ExportOBJ
            // http://web.cse.ohio-state.edu/~hwshen/581/Site/Lab3_files/Labhelp_Obj_parser.htm
        }
    }
}
