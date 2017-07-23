using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OBJExporterUI.Loaders;
using System.Drawing;
using OpenTK.Input;
using System.IO;

namespace OBJExporterUI
{
    public class PreviewControl
    {
        private GLControl renderCanvas;

        private bool ready = false;
        private string modelType;

        private CacheStorage cache = new CacheStorage();

        private NewCamera ActiveCamera;

        private string filename;

        private int adtShaderProgram;
        private int wmoShaderProgram;
        private int m2ShaderProgram;
        private int bakeShaderProgram;

        public PreviewControl(GLControl renderCanvas)
        {
            this.renderCanvas = renderCanvas;
            this.renderCanvas.Paint += RenderCanvas_Paint;
            this.renderCanvas.Load += RenderCanvas_Load;
            this.renderCanvas.Resize += RenderCanvas_Resize;

            ActiveCamera = new NewCamera(renderCanvas.Width, renderCanvas.Height, new Vector3(0, 0, -1), new Vector3(-11, 0, 0));
        }

        private void RenderCanvas_Resize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, renderCanvas.Width, renderCanvas.Height);
            if(renderCanvas.Width > 0 && renderCanvas.Height > 0)
            {
                ActiveCamera.viewportSize(renderCanvas.Width, renderCanvas.Height);
            }
        }

        public void LoadModel(string filename)
        {
            ready = false;

            GL.ActiveTexture(TextureUnit.Texture0);

            this.filename = filename;

            if (filename.EndsWith(".m2"))
            {
                if (!cache.doodadBatches.ContainsKey(filename))
                {
                    M2Loader.LoadM2(filename, cache, m2ShaderProgram);
                }
                modelType = "m2";
                ActiveCamera.switchMode("perspective");
                ActiveCamera.Pos = new Vector3((cache.doodadBatches[filename].boundingBox.max.Z) + 11.0f, 0.0f, 4.0f);
            }
            else if (filename.EndsWith(".wmo"))
            {
                if (!cache.worldModels.ContainsKey(filename))
                {
                    WMOLoader.LoadWMO(filename, cache, wmoShaderProgram);
                }
                ActiveCamera.switchMode("perspective");
                modelType = "wmo";
            }
            else if (filename.EndsWith(".adt"))
            {
#if DEBUG
                // Generate baked terrain texture

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

                if(GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                {
                    var error = GL.GetError().ToString();
                    Console.WriteLine(error);
                }

                var firstPos = cache.terrain[filename].startPos.Position;
                var projectionMatrix = Matrix4.CreateOrthographic(1024, 1024, -500f, 500f);
                var projectionMatrixLocation = GL.GetUniformLocation(bakeShaderProgram, "projection_matrix");
                GL.UniformMatrix4(projectionMatrixLocation, false, ref projectionMatrix);

                var modelviewMatrixLocation = GL.GetUniformLocation(bakeShaderProgram, "modelview_matrix");
                Matrix4 modelViewMatrix = Matrix4.LookAt(new Vector3(-1f, -1f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f ,1f));
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
                bmp.Save("test.bmp", System.Drawing.Imaging.ImageFormat.Bmp);

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.BindTexture(TextureTarget.Texture2D, 0);

                cache.terrain.Remove(filename);
#endif
                if (!cache.terrain.ContainsKey(filename))
                {
                    ADTLoader.LoadADT(filename, cache, adtShaderProgram);
                }
                //ActiveCamera.switchMode("ortho");
                ActiveCamera.Pos = new Vector3(cache.terrain[filename].startPos.Position.X, cache.terrain[filename].startPos.Position.Y, cache.terrain[filename].startPos.Position.Z);
                modelType = "adt";
            }

            ready = true;
        }

        public void WindowsFormsHost_Initialized(object sender, EventArgs e)
        {
            renderCanvas.MakeCurrent();
        }

        private void Update()
        {
            if (!renderCanvas.Focused) return;

            MouseState mouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();

            ActiveCamera.processKeyboardInput(keyboardState);

            return;
        }

        private void RenderCanvas_Load(object sender, EventArgs e)
        {
            GL.Enable(EnableCap.DepthTest);

            adtShaderProgram = Shader.CompileShader("adt");
            wmoShaderProgram = Shader.CompileShader("wmo");
            m2ShaderProgram = Shader.CompileShader("m2");
            bakeShaderProgram = Shader.CompileShader("baketexture");

            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        private void RenderCanvas_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            if (!ready) return;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Viewport(0, 0, renderCanvas.Width, renderCanvas.Height);
            GL.Enable(EnableCap.Texture2D);

            if (modelType == "m2")
            {
                GL.UseProgram(m2ShaderProgram);

                ActiveCamera.setupGLRenderMatrix(m2ShaderProgram);
                ActiveCamera.flyMode = false;

                GL.BindVertexArray(cache.doodadBatches[filename].vao);

                for (int i = 0; i < cache.doodadBatches[filename].submeshes.Length; i++)
                {
                    GL.BindTexture(TextureTarget.Texture2D, cache.doodadBatches[filename].submeshes[i].material);
                    GL.DrawRangeElements(PrimitiveType.Triangles, cache.doodadBatches[filename].submeshes[i].firstFace, (cache.doodadBatches[filename].submeshes[i].firstFace + cache.doodadBatches[filename].submeshes[i].numFaces), (int)cache.doodadBatches[filename].submeshes[i].numFaces, DrawElementsType.UnsignedInt, new IntPtr(cache.doodadBatches[filename].submeshes[i].firstFace * 4));
                }
            }
            else if (modelType == "wmo")
            {
                GL.UseProgram(wmoShaderProgram);

                ActiveCamera.setupGLRenderMatrix(wmoShaderProgram);
                ActiveCamera.flyMode = false;

                var alphaRefLoc = GL.GetUniformLocation(wmoShaderProgram, "alphaRef");

                for (int j = 0; j < cache.worldModelBatches[filename].wmoRenderBatch.Length; j++)
                {
                    GL.BindVertexArray(cache.worldModelBatches[filename].groupBatches[cache.worldModelBatches[filename].wmoRenderBatch[j].groupID].vao);

                    switch(cache.worldModelBatches[filename].wmoRenderBatch[j].blendType)
                    {
                        case 0:
                            GL.Disable(EnableCap.Blend);
                            GL.Uniform1(alphaRefLoc, -1.0f);
                            break;
                        case 1:
                            GL.Disable(EnableCap.Blend);
                            GL.Uniform1(alphaRefLoc, 0.90393700787f);
                            break;
                        case 2:
                            GL.Enable(EnableCap.Blend);
                            GL.Uniform1(alphaRefLoc, -1.0f);
                            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                            break;
                        default:
                            GL.Disable(EnableCap.Blend);
                            GL.Uniform1(alphaRefLoc, -1.0f);
                            break;
                    }

                    GL.BindTexture(TextureTarget.Texture2D, cache.worldModelBatches[filename].wmoRenderBatch[j].materialID[0]);
                    GL.DrawElements(PrimitiveType.Triangles, (int)cache.worldModelBatches[filename].wmoRenderBatch[j].numFaces, DrawElementsType.UnsignedInt, (int)cache.worldModelBatches[filename].wmoRenderBatch[j].firstFace * 4);
                }
            }else if(modelType == "adt")
            {
                GL.UseProgram(adtShaderProgram);

                ActiveCamera.setupGLRenderMatrix(adtShaderProgram);
                ActiveCamera.flyMode = true;

                GL.BindVertexArray(cache.terrain[filename].vao);

                for (int i = 0; i < cache.terrain[filename].renderBatches.Length; i++)
                {
                    for (int j = 0; j < cache.terrain[filename].renderBatches[i].materialID.Length; j++)
                    {
                        var textureLoc = GL.GetUniformLocation(adtShaderProgram, "layer" + j);
                        GL.Uniform1(textureLoc, j);

                        GL.ActiveTexture(TextureUnit.Texture0 + j);
                        GL.BindTexture(TextureTarget.Texture2D, (int)cache.terrain[filename].renderBatches[i].materialID[j]);
                    }

                    for (int j = 1; j < cache.terrain[filename].renderBatches[i].alphaMaterialID.Length; j++)
                    {
                        var textureLoc = GL.GetUniformLocation(adtShaderProgram, "alphaLayer" + j);
                        GL.Uniform1(textureLoc, 3 + j);

                        GL.ActiveTexture(TextureUnit.Texture3 + j);
                        GL.BindTexture(TextureTarget.Texture2D, cache.terrain[filename].renderBatches[i].alphaMaterialID[j]);
                    }

                    GL.DrawRangeElements(PrimitiveType.Triangles, (int)cache.terrain[filename].renderBatches[i].firstFace, (int)cache.terrain[filename].renderBatches[i].firstFace + (int)cache.terrain[filename].renderBatches[i].numFaces, (int)cache.terrain[filename].renderBatches[i].numFaces, DrawElementsType.UnsignedInt, new IntPtr(cache.terrain[filename].renderBatches[i].firstFace * 4));
                }
            }

            var error = GL.GetError().ToString();

            if (error != "NoError")
            {
                throw new Exception(error);
            }

            GL.BindVertexArray(0);
            renderCanvas.SwapBuffers();
        }

        public void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            Update();
            renderCanvas.Invalidate();
        }
    }
}
