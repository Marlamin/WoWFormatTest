using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWFormatLib.FileReaders;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OBJExporterUI.Loaders
{
    class WMOLoader
    {
        public static Renderer.Structs.WorldModel LoadWMO(string filename, CacheStorage cache, int shaderProgram)
        {
            if (cache.worldModelBatches.ContainsKey(filename))
            {
                return cache.worldModelBatches[filename];
            }

            var wmo = new WoWFormatLib.Structs.WMO.WMO();

            if (cache.worldModels.ContainsKey(filename))
            {
                wmo = cache.worldModels[filename];
            }
            else
            {
                //Load WMO from file
                if (WoWFormatLib.Utils.CASC.FileExists(filename))
                {
                    var wmofile = new WMOReader().LoadWMO(filename);
                    cache.worldModels.Add(filename, wmofile);
                    wmo = cache.worldModels[filename];
                }
                else
                {
                    throw new Exception("WMO " + filename + " does not exist!");
                }
            }

            if(wmo.group.Count() == 0)
            {
                CASCLib.Logger.WriteLine("WMO has no groups: ", filename);
                throw new Exception("Broken WMO! Report to developer (mail marlamin@marlamin.com) with this filename: " + filename);
            }

            var wmobatch = new Renderer.Structs.WorldModel()
            {
                groupBatches = new Renderer.Structs.WorldModelGroupBatches[wmo.group.Count()]
            };

            var groupNames = new string[wmo.group.Count()];

            for (var g = 0; g < wmo.group.Count(); g++)
            {
                if (wmo.group[g].mogp.vertices == null) { continue; }

                wmobatch.groupBatches[g].vao = GL.GenVertexArray();
                wmobatch.groupBatches[g].vertexBuffer = GL.GenBuffer();
                wmobatch.groupBatches[g].indiceBuffer = GL.GenBuffer();

                GL.BindVertexArray(wmobatch.groupBatches[g].vao);

                GL.BindBuffer(BufferTarget.ArrayBuffer, wmobatch.groupBatches[g].vertexBuffer);

                var wmovertices = new Renderer.Structs.M2Vertex[wmo.group[g].mogp.vertices.Count()];

                for (var i = 0; i < wmo.groupNames.Count(); i++)
                {
                    if (wmo.group[g].mogp.nameOffset == wmo.groupNames[i].offset)
                    {
                        groupNames[g] = wmo.groupNames[i].name.Replace(" ", "_");
                    }
                }

                if (groupNames[g] == "antiportal") { continue; }

                for (var i = 0; i < wmo.group[g].mogp.vertices.Count(); i++)
                {
                    wmovertices[i].Position = new Vector3(wmo.group[g].mogp.vertices[i].vector.X, wmo.group[g].mogp.vertices[i].vector.Y, wmo.group[g].mogp.vertices[i].vector.Z);
                    wmovertices[i].Normal = new Vector3(wmo.group[g].mogp.normals[i].normal.X, wmo.group[g].mogp.normals[i].normal.Y, wmo.group[g].mogp.normals[i].normal.Z);
                    if (wmo.group[g].mogp.textureCoords[0] == null)
                    {
                        wmovertices[i].TexCoord = new Vector2(0.0f, 0.0f);
                    }
                    else
                    {
                        wmovertices[i].TexCoord = new Vector2(wmo.group[g].mogp.textureCoords[0][i].X, wmo.group[g].mogp.textureCoords[0][i].Y);
                    }
                }

                //Push to buffer
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(wmovertices.Length * 8 * sizeof(float)), wmovertices, BufferUsageHint.StaticDraw);

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

                //Switch to Index buffer
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, wmobatch.groupBatches[g].indiceBuffer);

                var wmoindicelist = new List<uint>();
                for (var i = 0; i < wmo.group[g].mogp.indices.Count(); i++)
                {
                    wmoindicelist.Add(wmo.group[g].mogp.indices[i].indice);
                }

                wmobatch.groupBatches[g].indices = wmoindicelist.ToArray();

                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(wmobatch.groupBatches[g].indices.Length * sizeof(uint)), wmobatch.groupBatches[g].indices, BufferUsageHint.StaticDraw);
            }

            GL.Enable(EnableCap.Texture2D);

            wmobatch.mats = new Renderer.Structs.Material[wmo.materials.Count()];
            for (var i = 0; i < wmo.materials.Count(); i++)
            {
                wmobatch.mats[i].texture1 = wmo.materials[i].texture1;
                wmobatch.mats[i].texture2 = wmo.materials[i].texture2;
                wmobatch.mats[i].texture3 = wmo.materials[i].texture3;

                if (wmo.textures == null)
                {
                    if (WoWFormatLib.Utils.CASC.FileExists(wmo.materials[i].texture1))
                    {
                        wmobatch.mats[i].textureID1 = BLPLoader.LoadTexture(wmo.materials[i].texture1, cache);
                    }

                    if (WoWFormatLib.Utils.CASC.FileExists(wmo.materials[i].texture2))
                    {
                        wmobatch.mats[i].textureID2 = BLPLoader.LoadTexture(wmo.materials[i].texture2, cache);
                    }

                    if (WoWFormatLib.Utils.CASC.FileExists(wmo.materials[i].texture3))
                    {
                        wmobatch.mats[i].textureID3 = BLPLoader.LoadTexture(wmo.materials[i].texture3, cache);
                    }
                }
                else
                {
                    for (var ti = 0; ti < wmo.textures.Count(); ti++)
                    {
                        if (wmo.textures[ti].startOffset == wmo.materials[i].texture1)
                        {
                            wmobatch.mats[i].textureID1 = BLPLoader.LoadTexture(wmo.textures[ti].filename, cache);
                        }

                        if (wmo.textures[ti].startOffset == wmo.materials[i].texture2)
                        {
                            wmobatch.mats[i].textureID2 = BLPLoader.LoadTexture(wmo.textures[ti].filename, cache);
                        }

                        if (wmo.textures[ti].startOffset == wmo.materials[i].texture3)
                        {
                            wmobatch.mats[i].textureID3 = BLPLoader.LoadTexture(wmo.textures[ti].filename, cache);
                        }
                    }
                }
            }

            wmobatch.doodads = new Renderer.Structs.WMODoodad[wmo.doodadDefinitions.Count()];

            for(var i = 0; i < wmo.doodadDefinitions.Count(); i++)
            {
                if(wmo.doodadNames != null)
                {
                    for (var j = 0; j < wmo.doodadNames.Count(); j++)
                    {
                        if (wmo.doodadDefinitions[i].offset == wmo.doodadNames[j].startOffset)
                        {
                            wmobatch.doodads[i].filename = wmo.doodadNames[j].filename;
                        }
                    }
                }
                else
                {
                    wmobatch.doodads[i].filedataid = wmo.doodadDefinitions[i].offset;
                }

                wmobatch.doodads[i].flags = wmo.doodadDefinitions[i].flags;
                wmobatch.doodads[i].position = new Vector3(wmo.doodadDefinitions[i].position.X, wmo.doodadDefinitions[i].position.Y, wmo.doodadDefinitions[i].position.Z);
                wmobatch.doodads[i].rotation = new Quaternion(wmo.doodadDefinitions[i].rotation.X, wmo.doodadDefinitions[i].rotation.Y, wmo.doodadDefinitions[i].rotation.Z, wmo.doodadDefinitions[i].rotation.W);
                wmobatch.doodads[i].scale = wmo.doodadDefinitions[i].scale;
                wmobatch.doodads[i].color = new Vector4(wmo.doodadDefinitions[i].color[0], wmo.doodadDefinitions[i].color[1], wmo.doodadDefinitions[i].color[2], wmo.doodadDefinitions[i].color[3]);
            }

            var numRenderbatches = 0;
            //Get total amount of render batches
            for (var i = 0; i < wmo.group.Count(); i++)
            {
                if (wmo.group[i].mogp.renderBatches == null) { continue; }
                numRenderbatches = numRenderbatches + wmo.group[i].mogp.renderBatches.Count();
            }

            wmobatch.wmoRenderBatch = new Renderer.Structs.RenderBatch[numRenderbatches];

            var rb = 0;
            for (var g = 0; g < wmo.group.Count(); g++)
            {
                var group = wmo.group[g];
                if (group.mogp.renderBatches == null) { continue; }
                for (var i = 0; i < group.mogp.renderBatches.Count(); i++)
                {
                    wmobatch.wmoRenderBatch[rb].firstFace = group.mogp.renderBatches[i].firstFace;
                    wmobatch.wmoRenderBatch[rb].numFaces = group.mogp.renderBatches[i].numFaces;
                    uint matID = 0;

                    if (group.mogp.renderBatches[i].flags == 2)
                    {
                        matID = (uint) group.mogp.renderBatches[i].possibleBox2_3;
                    }
                    else
                    {
                        matID = group.mogp.renderBatches[i].materialID;
                    }

                    wmobatch.wmoRenderBatch[rb].materialID = new uint[3];
                    for (var ti = 0; ti < wmobatch.mats.Count(); ti++)
                    {
                        if (wmo.materials[matID].texture1 == wmobatch.mats[ti].texture1)
                        {
                            wmobatch.wmoRenderBatch[rb].materialID[0] = (uint)wmobatch.mats[ti].textureID1;
                        }

                        if (wmo.materials[matID].texture2 == wmobatch.mats[ti].texture2)
                        {
                            wmobatch.wmoRenderBatch[rb].materialID[1] = (uint)wmobatch.mats[ti].textureID2;
                        }

                        if (wmo.materials[matID].texture3 == wmobatch.mats[ti].texture3)
                        {
                            wmobatch.wmoRenderBatch[rb].materialID[2] = (uint)wmobatch.mats[ti].textureID3;
                        }
                    }

                    wmobatch.wmoRenderBatch[rb].blendType = wmo.materials[matID].blendMode;
                    wmobatch.wmoRenderBatch[rb].groupID = (uint)g;
                    rb++;
                }
            }
            cache.worldModelBatches.Add(filename, wmobatch);

            return wmobatch;
        }
    }
}
