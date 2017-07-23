using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OBJExporterUI.Loaders;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OBJExporterUI.Renderer
{
    class RenderMinimap
    {
        private static CacheStorage cache = new CacheStorage();
        private static int bakeShaderProgram;

        public static void Generate(string filename)
        {
            // Generate baked terrain texture
            bakeShaderProgram = Shader.CompileShader("baketexture");

            GL.UseProgram(bakeShaderProgram);

            if (!cache.terrain.ContainsKey(filename))
            {
                ADTLoader.LoadADT(filename, cache, bakeShaderProgram);
            }

            var frameBuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);

            var bakedTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, bakedTexture);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, 2048, 2048, 0, PixelFormat.Rgb, PixelType.UnsignedByte, new IntPtr(0));
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
            var projectionMatrix = Matrix4.CreateOrthographic(1024, 1024, -500f, 500f);
            var projectionMatrixLocation = GL.GetUniformLocation(bakeShaderProgram, "projection_matrix");
            GL.UniformMatrix4(projectionMatrixLocation, false, ref projectionMatrix);

            var modelviewMatrixLocation = GL.GetUniformLocation(bakeShaderProgram, "modelview_matrix");
            var eye = new Vector3((float)533.3333 / 2, (float)533.3333 / 2, 300f);
            var target = new Vector3((float)533.3333 / 2, (float)533.3333 / 2, 300f - 0.1f);
            //Matrix4 modelViewMatrix = Matrix4.LookAt(eye, target, new Vector3(0f, 0f, 1f));
            Matrix4 modelViewMatrix = Matrix4.LookAt(new Vector3(-1f, -1f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 1f));
            GL.UniformMatrix4(modelviewMatrixLocation, false, ref modelViewMatrix);

            var rotationMatrixLocation = GL.GetUniformLocation(bakeShaderProgram, "rotation_matrix");
            Matrix4 rotationMatrix = Matrix4.CreateRotationX(90f);
            GL.UniformMatrix4(rotationMatrixLocation, false, ref rotationMatrix);

            var firstPosLocation = GL.GetUniformLocation(bakeShaderProgram, "firstPos");
            GL.Uniform3(firstPosLocation, ref firstPos);
            GL.Viewport(0, 0, 2048, 2048);

            GL.BindVertexArray(cache.terrain[filename].vao);

            for (int i = 0; i < cache.terrain[filename].renderBatches.Length; i++)
            {
                for (int j = 0; j < cache.terrain[filename].renderBatches[i].materialID.Length; j++)
                {
                    var textureLoc = GL.GetUniformLocation(bakeShaderProgram, "layer" + j);
                    GL.Uniform1(textureLoc, j);

                    GL.ActiveTexture(TextureUnit.Texture0 + j);
                    GL.BindTexture(TextureTarget.Texture2D, (int)cache.terrain[filename].renderBatches[i].materialID[j]);
                }

                for (int j = 1; j < cache.terrain[filename].renderBatches[i].alphaMaterialID.Length; j++)
                {
                    var textureLoc = GL.GetUniformLocation(bakeShaderProgram, "alphaLayer" + j);
                    GL.Uniform1(textureLoc, 3 + j);

                    GL.ActiveTexture(TextureUnit.Texture3 + j);
                    GL.BindTexture(TextureTarget.Texture2D, cache.terrain[filename].renderBatches[i].alphaMaterialID[j]);
                }

                GL.DrawRangeElements(PrimitiveType.Triangles, (int)cache.terrain[filename].renderBatches[i].firstFace, (int)cache.terrain[filename].renderBatches[i].firstFace + (int)cache.terrain[filename].renderBatches[i].numFaces, (int)cache.terrain[filename].renderBatches[i].numFaces, DrawElementsType.UnsignedInt, new IntPtr(cache.terrain[filename].renderBatches[i].firstFace * 4));
            }

            Bitmap bmp = new Bitmap(2048, 2048);
            System.Drawing.Imaging.BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            GL.ReadPixels(0, 0, 2048, 2048, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);

            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            bmp.Save(Path.GetFileNameWithoutExtension(filename) + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}
