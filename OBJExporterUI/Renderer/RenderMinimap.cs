using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OBJExporterUI.Loaders;
using System.Drawing;
using System.Configuration;
using System.IO;

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

            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(bakeShaderProgram);

            if (!cache.terrain.ContainsKey(filename))
            {
                ADTLoader.LoadADT(filename, cache, bakeShaderProgram, loadModels);
            }

            var firstPos = cache.terrain[filename].startPos.Position;
            var projectionMatrixLocation = GL.GetUniformLocation(bakeShaderProgram, "projection_matrix");
            var modelviewMatrixLocation = GL.GetUniformLocation(bakeShaderProgram, "modelview_matrix");
            var firstPosLocation = GL.GetUniformLocation(bakeShaderProgram, "firstPos");
            var doodadOffsLocation = GL.GetUniformLocation(bakeShaderProgram, "doodadOffs");
            var heightScaleLoc = GL.GetUniformLocation(bakeShaderProgram, "pc_heightScale");
            var heightOffsetLoc = GL.GetUniformLocation(bakeShaderProgram, "pc_heightOffset");

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
                        var fbError = GL.GetError().ToString();
                        Console.WriteLine(fbError);
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
                        var textureLoc = GL.GetUniformLocation(bakeShaderProgram, "pt_layer" + j);
                        GL.Uniform1(textureLoc, j);

                        var scaleLoc = GL.GetUniformLocation(bakeShaderProgram, "layer" + j + "scale");
                        GL.Uniform1(scaleLoc, cache.terrain[filename].renderBatches[i].scales[j]);

                        GL.ActiveTexture(TextureUnit.Texture0 + j);
                        GL.BindTexture(TextureTarget.Texture2D, (int)cache.terrain[filename].renderBatches[i].materialID[j]);
                    }

                    for (var j = 1; j < cache.terrain[filename].renderBatches[i].alphaMaterialID.Length; j++)
                    {
                        var textureLoc = GL.GetUniformLocation(bakeShaderProgram, "pt_blend" + j);
                        GL.Uniform1(textureLoc, 3 + j);

                        GL.ActiveTexture(TextureUnit.Texture3 + j);
                        GL.BindTexture(TextureTarget.Texture2D, cache.terrain[filename].renderBatches[i].alphaMaterialID[j]);
                    }

                    for (var j = 0; j < cache.terrain[filename].renderBatches[i].heightMaterialIDs.Length; j++)
                    {
                        var textureLoc = GL.GetUniformLocation(bakeShaderProgram, "pt_height" + j);
                        GL.Uniform1(textureLoc, 7 + j);

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
                        Console.WriteLine(error);
                    }

                    var bmp = new Bitmap(bakeSize, bakeSize);
                    var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    GL.ReadPixels(0, 0, bakeSize, bakeSize, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
                    bmp.UnlockBits(data);

                    bmp.RotateFlip(RotateFlipType.Rotate270FlipX);
                    bmp.Save(outName.Replace(".png", "_" + i + ".png"), System.Drawing.Imaging.ImageFormat.Png);

                    bmp.Dispose();

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
                    var error = GL.GetError().ToString();
                    Console.WriteLine(error);
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
                        var textureLoc = GL.GetUniformLocation(bakeShaderProgram, "pt_layer" + j);
                        GL.Uniform1(textureLoc, j);

                        var scaleLoc = GL.GetUniformLocation(bakeShaderProgram, "layer" + j + "scale");
                        GL.Uniform1(scaleLoc, cache.terrain[filename].renderBatches[i].scales[j]);

                        GL.ActiveTexture(TextureUnit.Texture0 + j);
                        GL.BindTexture(TextureTarget.Texture2D, (int)cache.terrain[filename].renderBatches[i].materialID[j]);
                    }

                    for (var j = 1; j < cache.terrain[filename].renderBatches[i].alphaMaterialID.Length; j++)
                    {
                        var textureLoc = GL.GetUniformLocation(bakeShaderProgram, "pt_blend" + j);
                        GL.Uniform1(textureLoc, 3 + j);

                        GL.ActiveTexture(TextureUnit.Texture3 + j);
                        GL.BindTexture(TextureTarget.Texture2D, cache.terrain[filename].renderBatches[i].alphaMaterialID[j]);
                    }

                    for (var j = 0; j < cache.terrain[filename].renderBatches[i].heightMaterialIDs.Length; j++)
                    {
                        var textureLoc = GL.GetUniformLocation(bakeShaderProgram, "pt_height" + j);
                        GL.Uniform1(textureLoc, 7 + j);

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
                        Console.WriteLine(error);
                    }
                }

                foreach (var batch in cache.terrain[filename].worldModelBatches)
                {
                }

                var bmp = new Bitmap(bakeSize, bakeSize);
                var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                GL.ReadPixels(0, 0, bakeSize, bakeSize, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
                bmp.UnlockBits(data);

                bmp.RotateFlip(RotateFlipType.Rotate270FlipX);
                bmp.Save(outName, System.Drawing.Imaging.ImageFormat.Png);

                bmp.Dispose();

                GL.DeleteFramebuffer(frameBuffer);
                GL.UseProgram(0);
            }
        }
    }
}
