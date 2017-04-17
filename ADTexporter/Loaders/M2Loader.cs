using System;
using System.Collections.Generic;
using System.Linq;
using WoWFormatLib.FileReaders;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace ADTexporter.Loaders
{
    class M2Loader
    {
        public static void LoadM2(string filename, CacheStorage cache, int modelShader)
        {
            filename = filename.ToLower().Replace(".mdx", ".m2");
            filename = filename.ToLower().Replace(".mdl", ".m2");

            if (cache.doodadBatches.ContainsKey(filename))
            {
                return;
            }

            WoWFormatLib.Structs.M2.M2Model model = new WoWFormatLib.Structs.M2.M2Model();

            if (cache.models.ContainsKey(filename))
            {
                model = cache.models[filename];
            }
            else
            {
                //Load model from file
                if (WoWFormatLib.Utils.CASC.cascHandler.FileExists(filename))
                {
                    var modelreader = new M2Reader();
                    modelreader.LoadM2(filename);
                    cache.models.Add(filename, modelreader.model);
                    model = modelreader.model;
                }
                else
                {
                    throw new Exception("Model " + filename + " does not exist!");
                }
            }

            var ddBatch = new DoodadBatch();

            // Textures
            ddBatch.mats = new Material[model.textures.Count()];

            for (int i = 0; i < model.textures.Count(); i++)
            {
                string texturefilename = model.textures[i].filename;
                ddBatch.mats[i].flags = model.textures[i].flags;

                switch (model.textures[i].type)
                {
                    case 0:
                        // Console.WriteLine("      Texture given in file!");
                        texturefilename = model.textures[i].filename;
                        break;
                    case 1:
                        string[] csfilenames = WoWFormatLib.DBC.DBCHelper.getTexturesByModelFilename(filename, (int)model.textures[i].type, i);
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
                        if (WoWFormatLib.Utils.CASC.cascHandler.FileExists(System.IO.Path.ChangeExtension(filename, ".blp")))
                        {
                            // Console.WriteLine("      BLP exists!");
                            texturefilename = System.IO.Path.ChangeExtension(filename, ".blp");
                        }
                        else
                        {
                            //Console.WriteLine("      Type 2 does not exist!");
                            //needs lookup?
                        }
                        break;
                    case 11:
                        string[] cdifilenames = WoWFormatLib.DBC.DBCHelper.getTexturesByModelFilename(filename, (int)model.textures[i].type);
                        for (int ti = 0; ti < cdifilenames.Count(); ti++)
                        {
                            if (WoWFormatLib.Utils.CASC.cascHandler.FileExists(filename.Replace(model.name + ".M2", cdifilenames[ti] + ".blp")))
                            {
                                texturefilename = filename.Replace(model.name + ".M2", cdifilenames[ti] + ".blp");
                            }
                        }
                        break;
                    default:
                        //Console.WriteLine("      Falling back to placeholder texture");
                        texturefilename = "Dungeons\\Textures\\testing\\COLOR_13.blp";
                        break;
                }
                ddBatch.mats[i].textureID = BLPLoader.LoadTexture(texturefilename, cache);
                ddBatch.mats[i].filename = texturefilename;
            }
            
            // Submeshes
            ddBatch.submeshes = new Submesh[model.skins[0].submeshes.Count()];
            for (int i = 0; i < model.skins[0].submeshes.Count(); i++)
            {
                if (filename.StartsWith("character"))
                {
                    if (model.skins[0].submeshes[i].submeshID != 0)
                    {
                        if (!model.skins[0].submeshes[i].submeshID.ToString().EndsWith("01"))
                        {
                            continue;
                        }
                    }
                }

                ddBatch.submeshes[i].firstFace = model.skins[0].submeshes[i].startTriangle;
                ddBatch.submeshes[i].numFaces = model.skins[0].submeshes[i].nTriangles;
                for (int tu = 0; tu < model.skins[0].textureunit.Count(); tu++)
                {
                    if (model.skins[0].textureunit[tu].submeshIndex == i)
                    {
                        ddBatch.submeshes[i].blendType = model.renderflags[model.skins[0].textureunit[tu].renderFlags].blendingMode;
                        if (!cache.materials.ContainsKey(model.textures[model.texlookup[model.skins[0].textureunit[tu].texture].textureID].filename.ToLower()))
                        {
                            throw new Exception("MaterialCache does not have texture " + model.textures[model.texlookup[model.skins[0].textureunit[tu].texture].textureID].filename.ToLower());
                        }

                        ddBatch.submeshes[i].material = (uint)cache.materials[model.textures[model.texlookup[model.skins[0].textureunit[tu].texture].textureID].filename.ToLower()];
                    }
                }
            }

            ddBatch.vao = GL.GenVertexArray();
            GL.BindVertexArray(ddBatch.vao);

            // Vertices & indices
            ddBatch.vertexBuffer = GL.GenBuffer();
            ddBatch.indiceBuffer = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, ddBatch.vertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ddBatch.indiceBuffer);

            List<uint> modelindicelist = new List<uint>();
            for (int i = 0; i < model.skins[0].triangles.Count(); i++)
            {
                modelindicelist.Add(model.skins[0].triangles[i].pt1);
                modelindicelist.Add(model.skins[0].triangles[i].pt2);
                modelindicelist.Add(model.skins[0].triangles[i].pt3);
            }

            uint[] modelindices = modelindicelist.ToArray();

            //Console.WriteLine(modelindicelist.Count() + " indices!");
            ddBatch.indices = modelindices;

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ddBatch.indiceBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(ddBatch.indices.Length * sizeof(uint)), ddBatch.indices, BufferUsageHint.StaticDraw);

            M2Vertex[] modelvertices = new M2Vertex[model.vertices.Count()];

            for (int i = 0; i < model.vertices.Count(); i++)
            {
                modelvertices[i].Position = new Vector3(model.vertices[i].position.X, model.vertices[i].position.Y, model.vertices[i].position.Z);
                modelvertices[i].Normal = new Vector3(model.vertices[i].normal.X, model.vertices[i].normal.Y, model.vertices[i].normal.Z);
                modelvertices[i].TexCoord = new Vector2(model.vertices[i].textureCoordX, model.vertices[i].textureCoordY);
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, ddBatch.vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(modelvertices.Length * 8 * sizeof(float)), modelvertices, BufferUsageHint.StaticDraw);

            var texCoordLoc = GL.GetAttribLocation(modelShader, "vTexCoord");
            GL.EnableVertexAttribArray(texCoordLoc);
            GL.VertexAttribPointer(texCoordLoc, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));

            var posLoc = GL.GetAttribLocation(modelShader, "vPosition");
            GL.EnableVertexAttribArray(posLoc);
            GL.VertexAttribPointer(posLoc, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 5 * sizeof(float));

            GL.BindVertexArray(0);

            cache.doodadBatches.Add(filename, ddBatch);
        }
    }
}
