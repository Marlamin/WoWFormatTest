using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OBJExporterUI.Loaders;
using System.Drawing;

namespace OBJExporterUI.Renderer
{
    class RenderMinimap
    {
        public void Generate(string filename, string outName, CacheStorage cache, int bakeShaderProgram)
        {
            float TileSize = 1600.0f / 3.0f; //533.333
            float ChunkSize = TileSize / 16.0f; //33.333
            float UnitSize = ChunkSize / 8.0f; //4.166666
            float MapMidPoint = 32.0f / ChunkSize;

            var bakeSize = 8192;

            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(bakeShaderProgram);

            if (!cache.terrain.ContainsKey(filename))
            {
                ADTLoader.LoadADT(filename, cache, bakeShaderProgram);
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

            var firstPos = cache.terrain[filename].startPos.Position;
            var projectionMatrix = Matrix4.CreateOrthographic(TileSize, TileSize, -1500f, 1500f);
            var projectionMatrixLocation = GL.GetUniformLocation(bakeShaderProgram, "projection_matrix");
            GL.UniformMatrix4(projectionMatrixLocation, false, ref projectionMatrix);

            var modelviewMatrixLocation = GL.GetUniformLocation(bakeShaderProgram, "modelview_matrix");
            var eye = new Vector3(-TileSize / 2 , -TileSize/2, 400f);
            var target = new Vector3(-TileSize/2, -TileSize/2, 399.9999f);
            Matrix4 modelViewMatrix = Matrix4.LookAt(eye, target, new Vector3(0f, 1f, 0f));
            GL.UniformMatrix4(modelviewMatrixLocation, false, ref modelViewMatrix);

            var firstPosLocation = GL.GetUniformLocation(bakeShaderProgram, "firstPos");
            GL.Uniform3(firstPosLocation, ref firstPos);

            GL.Viewport(0, 0, bakeSize, bakeSize);

            GL.BindVertexArray(cache.terrain[filename].vao);

            for (int i = 0; i < cache.terrain[filename].renderBatches.Length; i++)
            {
                var heightScaleLoc = GL.GetUniformLocation(bakeShaderProgram, "pc_heightScale");
                GL.Uniform4(heightScaleLoc, cache.terrain[filename].renderBatches[i].heightScales);

                var heightOffsetLoc = GL.GetUniformLocation(bakeShaderProgram, "pc_heightOffset");
                GL.Uniform4(heightOffsetLoc, cache.terrain[filename].renderBatches[i].heightOffsets);

                for (int j = 0; j < cache.terrain[filename].renderBatches[i].materialID.Length; j++)
                {
                    var textureLoc = GL.GetUniformLocation(bakeShaderProgram, "pt_layer" + j);
                    GL.Uniform1(textureLoc, j);

                    var scaleLoc = GL.GetUniformLocation(bakeShaderProgram, "layer" + j + "scale");
                    GL.Uniform1(scaleLoc, cache.terrain[filename].renderBatches[i].scales[j]);

                    GL.ActiveTexture(TextureUnit.Texture0 + j);
                    GL.BindTexture(TextureTarget.Texture2D, (int)cache.terrain[filename].renderBatches[i].materialID[j]);
                }

                for (int j = 1; j < cache.terrain[filename].renderBatches[i].alphaMaterialID.Length; j++)
                {
                    var textureLoc = GL.GetUniformLocation(bakeShaderProgram, "pt_blend" + j);
                    GL.Uniform1(textureLoc, 3 + j);

                    GL.ActiveTexture(TextureUnit.Texture3 + j);
                    GL.BindTexture(TextureTarget.Texture2D, cache.terrain[filename].renderBatches[i].alphaMaterialID[j]);
                }

                for (int j = 0; j < cache.terrain[filename].renderBatches[i].heightMaterialIDs.Length; j++)
                {
                    var textureLoc = GL.GetUniformLocation(bakeShaderProgram, "pt_height" + j);
                    GL.Uniform1(textureLoc, 7 + j);

                    GL.ActiveTexture(TextureUnit.Texture7 + j);
                    GL.BindTexture(TextureTarget.Texture2D, cache.terrain[filename].renderBatches[i].heightMaterialIDs[j]);
                }

                GL.DrawElements(PrimitiveType.Triangles, (int)cache.terrain[filename].renderBatches[i].numFaces, DrawElementsType.UnsignedInt, (int)cache.terrain[filename].renderBatches[i].firstFace * 4);

                for (int j = 0; j < 11; j++)
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

            Bitmap bmp = new Bitmap(bakeSize, bakeSize);
            System.Drawing.Imaging.BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            GL.ReadPixels(0, 0, bakeSize, bakeSize, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);

            bmp.RotateFlip(RotateFlipType.Rotate270FlipX);
            bmp.Save(outName, System.Drawing.Imaging.ImageFormat.Png);

            bmp.Dispose();

            GL.DeleteFramebuffer(frameBuffer);
            GL.UseProgram(0);
            GL.DeleteProgram(bakeShaderProgram);
        }
    }
}
