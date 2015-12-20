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
    class M2Exporter
    {
        public static void exportM2(string file, BackgroundWorker exportworker = null)
        {
            if (exportworker == null)
            {
                exportworker = new BackgroundWorker();
            }

            var outdir = ConfigurationManager.AppSettings["outdir"];
            var reader = new M2Reader();

            exportworker.ReportProgress(15, "Reading M2..");

            if (!CASC.FileExists(file)) { throw new Exception("404 M2 not found!"); }

            reader.LoadM2(file);

            Structs.Vertex[] vertices = new Structs.Vertex[reader.model.vertices.Count()];

            for (int i = 0; i < reader.model.vertices.Count(); i++)
            {
                vertices[i].Position = new OpenTK.Vector3(reader.model.vertices[i].position.X, reader.model.vertices[i].position.Z, reader.model.vertices[i].position.Y * -1);
                vertices[i].Normal = new OpenTK.Vector3(reader.model.vertices[i].normal.X, reader.model.vertices[i].normal.Z, reader.model.vertices[i].normal.Y);
                vertices[i].TexCoord = new Vector2(reader.model.vertices[i].textureCoordX, reader.model.vertices[i].textureCoordY);
            }

            // Create output directory
            if (!Directory.Exists(Path.Combine(outdir, Path.GetDirectoryName(file))))
            {
                Directory.CreateDirectory(Path.Combine(outdir, Path.GetDirectoryName(file)));
            }

            var objsw = new StreamWriter(Path.Combine(outdir, file.Replace(".m2", ".obj")));

            objsw.WriteLine("# Written by Marlamin's WoW OBJExporter. Original file: " + file);
            objsw.WriteLine("mtllib " + Path.GetFileNameWithoutExtension(file) + ".mtl");

            foreach (var vertex in vertices)
            {
                objsw.WriteLine("v " + vertex.Position.X + " " + vertex.Position.Y + " " + vertex.Position.Z);
                objsw.WriteLine("vt " + vertex.TexCoord.X + " " + -vertex.TexCoord.Y);
                objsw.WriteLine("vn " + vertex.Position.X + " " + vertex.Position.Y + " " + vertex.Normal.Z);
            }

            List<uint> indicelist = new List<uint>();
            for (int i = 0; i < reader.model.skins[0].triangles.Count(); i++)
            {
                var t = reader.model.skins[0].triangles[i];
                indicelist.Add(t.pt1);
                indicelist.Add(t.pt2);
                indicelist.Add(t.pt3);
            }

            var indices = indicelist.ToArray();
            exportworker.ReportProgress(35, "Writing files..");

            var renderbatches = new Structs.RenderBatch[reader.model.skins[0].submeshes.Count()];
            for (int i = 0; i < reader.model.skins[0].submeshes.Count(); i++)
            {
                if (file.StartsWith("character", StringComparison.CurrentCultureIgnoreCase))
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
                for (int tu = 0; tu < reader.model.skins[0].textureunit.Count(); tu++)
                {
                    if (reader.model.skins[0].textureunit[tu].submeshIndex == i)
                    {
                        renderbatches[i].blendType = reader.model.renderflags[reader.model.skins[0].textureunit[tu].renderFlags].blendingMode;
                        renderbatches[i].materialID = reader.model.texlookup[reader.model.skins[0].textureunit[tu].texture].textureID;
                    }
                }
            }

            exportworker.ReportProgress(65, "Exporting textures..");

            var mtlsb = new StreamWriter(Path.Combine(outdir, file.Replace(".m2", ".mtl")));
            var textureID = 0;
            var materials = new Structs.Material[reader.model.textures.Count()];

            for (int i = 0; i < reader.model.textures.Count(); i++)
            {
                string texturefilename = "Dungeons\\Textures\\testing\\COLOR_13.blp";
                materials[i].flags = reader.model.textures[i].flags;
                switch (reader.model.textures[i].type)
                {
                    case 0:
                        //Console.WriteLine("      Texture given in file!");
                        texturefilename = reader.model.textures[i].filename;
                        break;
                    case 1:
                        string[] csfilenames = WoWFormatLib.DBC.DBCHelper.getTexturesByModelFilename(file, (int)reader.model.textures[i].type, i);
                        if (csfilenames.Count() > 0)
                        {
                            texturefilename = csfilenames[0];
                        }
                        else
                        {
                            //Console.WriteLine("      No type 1 texture found, falling back to placeholder texture");
                        }
                        break;
                    case 2:
                        if (WoWFormatLib.Utils.CASC.FileExists(Path.ChangeExtension(file, ".blp")))
                        {
                            //Console.WriteLine("      BLP exists!");
                            texturefilename = Path.ChangeExtension(file, ".blp");
                        }
                        else
                        {
                            //Console.WriteLine("      Type 2 does not exist!");
                            //needs lookup?
                        }
                        break;
                    case 11:
                        string[] cdifilenames = WoWFormatLib.DBC.DBCHelper.getTexturesByModelFilename(file, (int)reader.model.textures[i].type);
                        for (int ti = 0; ti < cdifilenames.Count(); ti++)
                        {
                            if (WoWFormatLib.Utils.CASC.FileExists(file.Replace(reader.model.name + ".M2", cdifilenames[ti] + ".blp")))
                            {
                                texturefilename = file.Replace(reader.model.name + ".M2", cdifilenames[ti] + ".blp");
                            }
                        }
                        break;
                    default:
                        // Console.WriteLine("      Falling back to placeholder texture");
                        break;
                }

                //Console.WriteLine("      Eventual filename is " + texturefilename);

                materials[i].textureID = textureID + i;
                materials[i].filename = Path.GetFileNameWithoutExtension(texturefilename);

                var blpreader = new BLPReader();

                blpreader.LoadBLP(texturefilename);

                try
                {
                    blpreader.bmp.Save(Path.Combine(outdir, Path.GetDirectoryName(file), materials[i].filename + ".png"));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            exportworker.ReportProgress(85, "Writing files..");

            foreach (var material in materials)
            {
                mtlsb.WriteLine("newmtl " + material.filename);
                mtlsb.WriteLine("illum 2");
                mtlsb.WriteLine("map_Ka " + material.filename + ".png");
                mtlsb.WriteLine("map_Kd " + material.filename + ".png");
            }

            mtlsb.Close();

            objsw.WriteLine("g " + Path.GetFileNameWithoutExtension(file));

            foreach (var renderbatch in renderbatches)
            {
                var i = renderbatch.firstFace;
                objsw.WriteLine("o " + Path.GetFileNameWithoutExtension(file) + renderbatch.groupID);
                objsw.WriteLine("usemtl " + materials[renderbatch.materialID].filename);
                objsw.WriteLine("s 1");
                while (i < (renderbatch.firstFace + renderbatch.numFaces))
                {
                    objsw.WriteLine("f " + (indices[i] + 1) + "/" + (indices[i] + 1) + "/" + (indices[i] + 1) + " " + (indices[i + 1] + 1) + "/" + (indices[i + 1] + 1) + "/" + (indices[i + 1] + 1) + " " + (indices[i + 2] + 1) + "/" + (indices[i + 2] + 1) + "/" + (indices[i + 2] + 1));
                    i = i + 3;
                }
            }

            objsw.Close();
            // https://en.wikipedia.org/wiki/Wavefront_.obj_file#Basic_materials
            // http://wiki.unity3d.com/index.php?title=ExportOBJ
            // http://web.cse.ohio-state.edu/~hwshen/581/Site/Lab3_files/Labhelp_Obj_parser.htm

            Console.WriteLine("Done loading model!");
        }
    }
}
