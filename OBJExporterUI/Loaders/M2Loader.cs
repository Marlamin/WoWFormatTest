using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OBJExporterUI.Loaders;
using WoWFormatLib.FileReaders;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace OBJExporterUI.Loaders
{
    class M2Loader
    {
        public static void LoadM2(string filename, CacheStorage cache, int shaderProgram)
        {
            filename = filename.ToLower().Replace(".mdx", ".m2");
            filename = filename.ToLower().Replace(".mdl", ".m2");

            if (cache.doodadBatches.ContainsKey(filename))
            {
                return;
            }

            var model = new WoWFormatLib.Structs.M2.M2Model();

            if (cache.models.ContainsKey(filename))
            {
                model = cache.models[filename];
            }
            else
            {
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

            if (model.boundingbox == null)
            {
                CASCLib.Logger.WriteLine("Error during loading file: {0}, bounding box is not defined", filename);
                return;
            }

            var ddBatch = new Renderer.Structs.DoodadBatch()
            {
                boundingBox = new Renderer.Structs.BoundingBox()
                {
                    min = new Vector3(model.boundingbox[0].X, model.boundingbox[0].Y, model.boundingbox[0].Z),
                    max = new Vector3(model.boundingbox[1].X, model.boundingbox[1].Y, model.boundingbox[1].Z)
                }
            };

            if (model.textures == null)
            {
                CASCLib.Logger.WriteLine("Error during loading file: {0}, model has no textures", filename);
                return;
            }

            if (model.skins == null)
            {
                CASCLib.Logger.WriteLine("Error during loading file: {0}, model has no skins", filename);
                return;
            }

            // Textures
            ddBatch.mats = new Renderer.Structs.Material[model.textures.Count()];

            for (var i = 0; i < model.textures.Count(); i++)
            {
                var textureFileDataID = 372993;
                ddBatch.mats[i].flags = model.textures[i].flags;

                switch (model.textures[i].type)
                {
                    case 0:
                        if(model.textureFileDataIDs != null && model.textureFileDataIDs.Length > 0)
                        {
                            textureFileDataID = model.textureFileDataIDs[i];
                        }
                        else
                        {
                            textureFileDataID = WoWFormatLib.Utils.CASC.getFileDataIdByName(model.textures[i].filename);
                        }
                        break;
                    case 1:
                    case 2:
                    case 11:
                    default:
                        textureFileDataID = 372993;
                        break;
                }

                // Not set in TXID
                if(textureFileDataID == 0)
                {
                    textureFileDataID = 372993;
                }

                ddBatch.mats[i].textureID = BLPLoader.LoadTexture(textureFileDataID, cache);
                ddBatch.mats[i].filename = textureFileDataID.ToString();
            }

            // Submeshes
            ddBatch.submeshes = new Renderer.Structs.Submesh[model.skins[0].submeshes.Count()];
            for (var i = 0; i < model.skins[0].submeshes.Count(); i++)
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
                for (var tu = 0; tu < model.skins[0].textureunit.Count(); tu++)
                {
                    if (model.skins[0].textureunit[tu].submeshIndex == i)
                    {
                        ddBatch.submeshes[i].blendType = model.renderflags[model.skins[0].textureunit[tu].renderFlags].blendingMode;

                        var textureFileDataID = 372993;

                        if (model.textureFileDataIDs != null && model.textureFileDataIDs.Length > 0)
                        {
                            textureFileDataID = model.textureFileDataIDs[model.texlookup[model.skins[0].textureunit[tu].texture].textureID];
                        }
                        else
                        {
                            textureFileDataID = WoWFormatLib.Utils.CASC.getFileDataIdByName(model.textures[model.texlookup[model.skins[0].textureunit[tu].texture].textureID].filename);
                        }

                        if (!cache.materials.ContainsKey(textureFileDataID))
                        {
                            throw new Exception("MaterialCache does not have texture " + textureFileDataID);
                        }

                        ddBatch.submeshes[i].material = (uint)cache.materials[textureFileDataID];
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

            var modelindicelist = new List<uint>();
            for (var i = 0; i < model.skins[0].triangles.Count(); i++)
            {
                modelindicelist.Add(model.skins[0].triangles[i].pt1);
                modelindicelist.Add(model.skins[0].triangles[i].pt2);
                modelindicelist.Add(model.skins[0].triangles[i].pt3);
            }

            var modelindices = modelindicelist.ToArray();

            //Console.WriteLine(modelindicelist.Count() + " indices!");
            ddBatch.indices = modelindices;

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ddBatch.indiceBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(ddBatch.indices.Length * sizeof(uint)), ddBatch.indices, BufferUsageHint.StaticDraw);

            var modelvertices = new Renderer.Structs.M2Vertex[model.vertices.Count()];

            for (var i = 0; i < model.vertices.Count(); i++)
            {
                modelvertices[i].Position = new Vector3(model.vertices[i].position.X, model.vertices[i].position.Y, model.vertices[i].position.Z);
                modelvertices[i].Normal = new Vector3(model.vertices[i].normal.X, model.vertices[i].normal.Y, model.vertices[i].normal.Z);
                modelvertices[i].TexCoord = new Vector2(model.vertices[i].textureCoordX, model.vertices[i].textureCoordY);
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, ddBatch.vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(modelvertices.Length * 8 * sizeof(float)), modelvertices, BufferUsageHint.StaticDraw);

            //Set pointers in buffer
            //var normalAttrib = GL.GetAttribLocation(shaderProgram, "normal");
            //GL.EnableVertexAttribArray(normalAttrib);
            //GL.VertexAttribPointer(normalAttrib, 3, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 0);

            var texCoordAttrib = GL.GetAttribLocation(shaderProgram, "texCoord");
            GL.EnableVertexAttribArray(texCoordAttrib);
            GL.VertexAttribPointer(texCoordAttrib, 2, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 3);

            var posAttrib = GL.GetAttribLocation(shaderProgram, "position");
            GL.EnableVertexAttribArray(posAttrib);
            GL.VertexAttribPointer(posAttrib, 3, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 5);
            cache.doodadBatches.Add(filename, ddBatch);
        }
    }
}
