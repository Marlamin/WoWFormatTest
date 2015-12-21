using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWFormatLib.FileReaders;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace WoWOpenGL.Loaders
{
    class WMOLoader
    {
        public static TerrainWindow.WorldModel LoadWMO(string filename, CacheStorage cache)
        {
            WoWFormatLib.Structs.WMO.WMO wmo = new WoWFormatLib.Structs.WMO.WMO();

            if (cache.worldModels.ContainsKey(filename))
            {
                wmo = cache.worldModels[filename];
            }
            else
            {
                //Load WMO from file
                if (WoWFormatLib.Utils.CASC.FileExists(filename))
                {
                    var wmoreader = new WMOReader();
                    wmoreader.LoadWMO(filename);
                    cache.worldModels.Add(filename, wmoreader.wmofile);
                    wmo = wmoreader.wmofile;
                }
                else
                {
                    throw new Exception("WMO " + filename + " does not exist!");
                }
            }

            var wmobatch = new TerrainWindow.WorldModel();

            wmobatch.groupBatches = new TerrainWindow.WorldModelGroupBatches[wmo.group.Count()];

            for (int g = 0; g < wmo.group.Count(); g++)
            {
                if (wmo.group[g].mogp.vertices == null) { continue; }

                wmobatch.groupBatches[g].vertexBuffer = GL.GenBuffer();
                wmobatch.groupBatches[g].indiceBuffer = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, wmobatch.groupBatches[g].vertexBuffer);

                TerrainWindow.M2Vertex[] wmovertices = new TerrainWindow.M2Vertex[wmo.group[g].mogp.vertices.Count()];

                for (int i = 0; i < wmo.group[g].mogp.vertices.Count(); i++)
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

                //Switch to Index buffer
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, wmobatch.groupBatches[g].indiceBuffer);

                List<uint> wmoindicelist = new List<uint>();
                for (int i = 0; i < wmo.group[g].mogp.indices.Count(); i++)
                {
                    wmoindicelist.Add(wmo.group[g].mogp.indices[i].indice);
                }

                wmobatch.groupBatches[g].indices = wmoindicelist.ToArray();

                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(wmobatch.groupBatches[g].indices.Length * sizeof(uint)), wmobatch.groupBatches[g].indices, BufferUsageHint.StaticDraw);
            }

            GL.Enable(EnableCap.Texture2D);

            wmobatch.mats = new TerrainWindow.Material[wmo.materials.Count()];
            for (int i = 0; i < wmo.materials.Count(); i++)
            {
                for (int ti = 0; ti < wmo.textures.Count(); ti++)
                {

                    if (wmo.textures[ti].startOffset == wmo.materials[i].texture1)
                    {
                        wmobatch.mats[i].texture1 = wmo.materials[i].texture1;
                        wmobatch.mats[i].textureID = BLPLoader.LoadTexture(wmo.textures[ti].filename, cache);
                        wmobatch.mats[i].filename = wmo.textures[ti].filename;
                    }
                }
            }

            

            int numRenderbatches = 0;
            //Get total amount of render batches
            for (int i = 0; i < wmo.group.Count(); i++)
            {
                if (wmo.group[i].mogp.renderBatches == null) { continue; }
                numRenderbatches = numRenderbatches + wmo.group[i].mogp.renderBatches.Count();
            }

            wmobatch.wmoRenderBatch = new TerrainWindow.RenderBatch[numRenderbatches];

            int rb = 0;
            for (int g = 0; g < wmo.group.Count(); g++)
            {
                var group = wmo.group[g];
                if (group.mogp.renderBatches == null) { continue; }
                for (int i = 0; i < group.mogp.renderBatches.Count(); i++)
                {
                    wmobatch.wmoRenderBatch[rb].firstFace = group.mogp.renderBatches[i].firstFace;
                    wmobatch.wmoRenderBatch[rb].numFaces = group.mogp.renderBatches[i].numFaces;
                    for (int ti = 0; ti < wmobatch.mats.Count(); ti++)
                    {
                        if (wmo.materials[group.mogp.renderBatches[i].materialID].texture1 == wmobatch.mats[ti].texture1)
                        {
                            wmobatch.wmoRenderBatch[rb].materialID = new uint[] { (uint)wmobatch.mats[ti].textureID };
                        }
                    }

                    wmobatch.wmoRenderBatch[rb].blendType = wmo.materials[group.mogp.renderBatches[i].materialID].blendMode;
                    wmobatch.wmoRenderBatch[rb].groupID = (uint)g;
                    rb++;
                }
            }
            return wmobatch;
        }
    }
}
