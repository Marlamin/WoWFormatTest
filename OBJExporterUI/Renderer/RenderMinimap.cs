using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OBJExporterUI.Loaders;
using System.Drawing;
using System.Configuration;
using System.IO;
using CASCLib;

namespace OBJExporterUI.Renderer
{
    class RenderMinimap
    {
        public void Generate(string filename, string outName, CacheStorage cache, int bakeShaderProgram, bool loadModels = false)
        {
            var TileSize = 1600.0f / 3.0f; //533.333
            var ChunkSize = TileSize / 16.0f; //33.333
            var UnitSize = ChunkSize / 8.0f; //4.166666
            var MapMidPoint = 32.0f / ChunkSize;

            var bakeSize = 4096;
            var splitFiles = false;

            ConfigurationManager.RefreshSection("appSettings");

            var size = ConfigurationManager.AppSettings["bakeQuality"];

            if(size == "minimap")
            {
                bakeSize = 256;
            }else if (size == "low")
            {
                bakeSize = 4096;
            }else if(size == "medium")
            {
                bakeSize = 8192;
            }else if(size == "high")
            {
                bakeSize = 1024;
                splitFiles = true;
            }

            if (!Directory.Exists(Path.GetDirectoryName(outName)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outName));
            }

            if (!splitFiles && File.Exists(outName))
            {
                return;
            }

            // Force terrain cache to empty after having 1 ADT cached and run GC
            if(cache.terrain.Count > 1)
            {
                // Make sure to delete lingering alpha textures from GPU
                foreach(var adt in cache.terrain)
                {
                    foreach(var batch in adt.Value.renderBatches)
                    {
                        GL.DeleteTextures(batch.alphaMaterialID.Length, batch.alphaMaterialID);
                    }
                }

                cache.terrain = new System.Collections.Generic.Dictionary<string, Structs.Terrain>();
                GC.Collect();
            }

            if (!cache.terrain.ContainsKey(filename))
            {
                ADTLoader.LoadADT(filename, cache, bakeShaderProgram, loadModels);
            }

            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UseProgram(bakeShaderProgram);

            // Look up uniforms beforehand instead of during drawing
            var firstPos = cache.terrain[filename].startPos.Position;
            var projectionMatrixLocation = GL.GetUniformLocation(bakeShaderProgram, "projection_matrix");
            var modelviewMatrixLocation = GL.GetUniformLocation(bakeShaderProgram, "modelview_matrix");
            var firstPosLocation = GL.GetUniformLocation(bakeShaderProgram, "firstPos");
            var doodadOffsLocation = GL.GetUniformLocation(bakeShaderProgram, "doodadOffs");
            var heightScaleLoc = GL.GetUniformLocation(bakeShaderProgram, "pc_heightScale");
            var heightOffsetLoc = GL.GetUniformLocation(bakeShaderProgram, "pc_heightOffset");

            var layerLocs = new int[4];
            var scaleLocs = new int[4];
            var heightLocs = new int[4];
            var blendLocs = new int[4];

            for (var i = 0; i < 4; i++)
            {
                layerLocs[i] = GL.GetUniformLocation(bakeShaderProgram, "pt_layer" + i);
                scaleLocs[i] = GL.GetUniformLocation(bakeShaderProgram, "layer" + i + "scale");
                heightLocs[i] = GL.GetUniformLocation(bakeShaderProgram, "pt_height" + i);

                // There are only 3 blend samplers
                if (i > 0)
                {
                    blendLocs[i] = GL.GetUniformLocation(bakeShaderProgram, "pt_blend" + i);
                }
            }

            if (splitFiles)
            {
                GL.BindVertexArray(cache.terrain[filename].vao);

                for (var i = 0; i < cache.terrain[filename].renderBatches.Length; i++)
                {
                    if(File.Exists(outName.Replace(".png", "_" + i + ".png")))
                    {
                        continue;
                    }

                    var x = i / 16;
                    var y = i % 16;

                    var frameBuffer = GL.GenFramebuffer();
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);

                    var bakedTexture = GL.GenTexture();
                    GL.BindTexture(TextureTarget.Texture2D, bakedTexture);

                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, bakeSize, bakeSize, 0, PixelFormat.Rgb, PixelType.UnsignedByte, new IntPtr(0));
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                    GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, bakedTexture, 0);

                    GL.DrawBuffers(1, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0 });

                    if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                    {
                        var fberror = GL.GetError().ToString();
                        Logger.WriteLine("Frame buffer initialization error: " + fberror);
                    }

                    var projectionMatrix = Matrix4.CreateOrthographic(ChunkSize, ChunkSize, -1500f, 1500f);
                    GL.UniformMatrix4(projectionMatrixLocation, false, ref projectionMatrix);

                    var eye = new Vector3(-ChunkSize / 2, (-ChunkSize / 2), 400f);
                    var target = new Vector3(-ChunkSize / 2, (-ChunkSize / 2), 399.9999f);

                    var modelViewMatrix = Matrix4.LookAt(eye, target, new Vector3(0f, 1f, 0f));
                    GL.UniformMatrix4(modelviewMatrixLocation, false, ref modelViewMatrix);

                    var chunkPos = firstPos;
                    chunkPos.X -= ChunkSize * x;
                    chunkPos.Y -= ChunkSize * y;

                    GL.Uniform3(firstPosLocation, ref chunkPos);

                    GL.Viewport(0,0, bakeSize, bakeSize);

                    GL.Uniform4(heightScaleLoc, cache.terrain[filename].renderBatches[i].heightScales);

                    GL.Uniform4(heightOffsetLoc, cache.terrain[filename].renderBatches[i].heightOffsets);

                    for (var j = 0; j < cache.terrain[filename].renderBatches[i].materialID.Length; j++)
                    {
                        GL.Uniform1(layerLocs[j], j);
                        GL.Uniform1(scaleLocs[j], cache.terrain[filename].renderBatches[i].scales[j]);

                        GL.ActiveTexture(TextureUnit.Texture0 + j);
                        GL.BindTexture(TextureTarget.Texture2D, (int)cache.terrain[filename].renderBatches[i].materialID[j]);
                    }

                    for (var j = 1; j < cache.terrain[filename].renderBatches[i].alphaMaterialID.Length; j++)
                    {
                        GL.Uniform1(blendLocs[j], 3 + j);

                        GL.ActiveTexture(TextureUnit.Texture3 + j);
                        GL.BindTexture(TextureTarget.Texture2D, cache.terrain[filename].renderBatches[i].alphaMaterialID[j]);
                    }

                    for (var j = 0; j < cache.terrain[filename].renderBatches[i].heightMaterialIDs.Length; j++)
                    {
                        GL.Uniform1(heightLocs[j], 7 + j);

                        GL.ActiveTexture(TextureUnit.Texture7 + j);
                        GL.BindTexture(TextureTarget.Texture2D, cache.terrain[filename].renderBatches[i].heightMaterialIDs[j]);
                    }

                    GL.DrawElements(PrimitiveType.Triangles, (int)cache.terrain[filename].renderBatches[i].numFaces, DrawElementsType.UnsignedInt, (int)cache.terrain[filename].renderBatches[i].firstFace * 4);

                    for (var j = 0; j < 11; j++)
                    {
                        GL.ActiveTexture(TextureUnit.Texture0 + j);
                        GL.BindTexture(TextureTarget.Texture2D, 0);
                    }

                    var error = GL.GetError().ToString();
                    if (error != "NoError")
                    {
                        Logger.WriteLine("Drawing error: " + error);
                    }

                    var bmp = new Bitmap(bakeSize, bakeSize, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    GL.ReadPixels(0, 0, bakeSize, bakeSize, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
                    bmp.UnlockBits(data);

                    bmp.RotateFlip(RotateFlipType.Rotate270FlipX);
                    bmp.Save(outName.Replace(".png", "_" + i + ".png"), System.Drawing.Imaging.ImageFormat.Png);

                    bmp.Dispose();

                    GL.DeleteTexture(bakedTexture);
                    GL.DeleteFramebuffer(frameBuffer);
                }
                
                GL.UseProgram(0);
            }
            else
            {
                if (File.Exists(outName))
                {
                    return;
                }

                var frameBuffer = GL.GenFramebuffer();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);

                var bakedTexture = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, bakedTexture);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, bakeSize, bakeSize, 0, PixelFormat.Rgb, PixelType.UnsignedByte, new IntPtr(0));
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, bakedTexture, 0);

                GL.DrawBuffers(1, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0 });

                if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                {
                    var fberror = GL.GetError().ToString();
                    Logger.WriteLine("Frame buffer initialization error: " + fberror);
                }

                var projectionMatrix = Matrix4.CreateOrthographic(TileSize, TileSize, -1500f, 1500f);
                GL.UniformMatrix4(projectionMatrixLocation, false, ref projectionMatrix);

                var eye = new Vector3(-TileSize / 2, -TileSize / 2, 400f);
                var target = new Vector3(-TileSize / 2, -TileSize / 2, 399.9999f);
                var modelViewMatrix = Matrix4.LookAt(eye, target, new Vector3(0f, 1f, 0f));
                GL.UniformMatrix4(modelviewMatrixLocation, false, ref modelViewMatrix);

                GL.Uniform3(firstPosLocation, ref firstPos);

                GL.Viewport(0, 0, bakeSize, bakeSize);

                GL.BindVertexArray(cache.terrain[filename].vao);

                for (var i = 0; i < cache.terrain[filename].renderBatches.Length; i++)
                {
                    GL.Uniform4(heightScaleLoc, cache.terrain[filename].renderBatches[i].heightScales);
                    GL.Uniform4(heightOffsetLoc, cache.terrain[filename].renderBatches[i].heightOffsets);

                    for (var j = 0; j < cache.terrain[filename].renderBatches[i].materialID.Length; j++)
                    {
                        GL.Uniform1(layerLocs[j], j);
                        GL.Uniform1(scaleLocs[j], cache.terrain[filename].renderBatches[i].scales[j]);

                        GL.ActiveTexture(TextureUnit.Texture0 + j);
                        GL.BindTexture(TextureTarget.Texture2D, (int)cache.terrain[filename].renderBatches[i].materialID[j]);
                    }

                    for (var j = 1; j < cache.terrain[filename].renderBatches[i].alphaMaterialID.Length; j++)
                    {
                        GL.Uniform1(blendLocs[j], 3 + j);

                        GL.ActiveTexture(TextureUnit.Texture3 + j);
                        GL.BindTexture(TextureTarget.Texture2D, cache.terrain[filename].renderBatches[i].alphaMaterialID[j]);
                    }

                    for (var j = 0; j < cache.terrain[filename].renderBatches[i].heightMaterialIDs.Length; j++)
                    {
                        GL.Uniform1(heightLocs[j], 7 + j);

                        GL.ActiveTexture(TextureUnit.Texture7 + j);
                        GL.BindTexture(TextureTarget.Texture2D, cache.terrain[filename].renderBatches[i].heightMaterialIDs[j]);
                    }

                    GL.DrawElements(PrimitiveType.Triangles, (int)cache.terrain[filename].renderBatches[i].numFaces, DrawElementsType.UnsignedInt, (int)cache.terrain[filename].renderBatches[i].firstFace * 4);

                    for (var j = 0; j < 11; j++)
                    {
                        GL.ActiveTexture(TextureUnit.Texture0 + j);
                        GL.BindTexture(TextureTarget.Texture2D, 0);
                    }
                }

                var error = GL.GetError().ToString();
                if (error != "NoError")
                {
                    Logger.WriteLine("Drawing error: " + error);
                }

                try
                {
                    using (var bmp = new Bitmap(bakeSize, bakeSize))
                    {
                        var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                        GL.ReadPixels(0, 0, bakeSize, bakeSize, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
                        bmp.UnlockBits(data);

                        bmp.RotateFlip(RotateFlipType.Rotate270FlipX);
                        bmp.Save(outName, System.Drawing.Imaging.ImageFormat.Png);

                        bmp.Dispose();
                    }
                }
                catch (Exception e)
                {
                    Logger.WriteLine("An error occured while baking minimap image " + Path.GetFileNameWithoutExtension(outName) + ": " + e.StackTrace);
                }
                finally
                {
                    GL.DeleteTexture(bakedTexture);
                    GL.DeleteFramebuffer(frameBuffer);
                    GL.UseProgram(0);
                }
            }
        }
    }
}
