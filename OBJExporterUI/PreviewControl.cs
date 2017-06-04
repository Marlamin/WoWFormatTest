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
        private bool isWMO;

        // Cache storage for models... bad idea?
        private CacheStorage cache = new CacheStorage();

        private NewCamera ActiveCamera;

        private string filename;

        private int vertexAttribObject;

        private int shaderProgram;

        public PreviewControl(GLControl renderCanvas)
        {
            this.renderCanvas = renderCanvas;
            this.renderCanvas.Paint += RenderCanvas_Paint;
            this.renderCanvas.Load += RenderCanvas_Load;
            this.renderCanvas.Resize += RenderCanvas_Resize;

            ActiveCamera = new NewCamera(renderCanvas.Width, renderCanvas.Height, new Vector3(0, 0, -1), new Vector3(-11, 0, 0), Vector3.UnitZ);
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
            vertexAttribObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexAttribObject);

            Console.WriteLine("Generated and binded to vertex array: " + vertexAttribObject);

            GL.ActiveTexture(TextureUnit.Texture0);

            this.filename = filename;

            if (filename.EndsWith(".m2"))
            {
                M2Loader.LoadM2(filename, cache);
                isWMO = false;
            }
            else if (filename.EndsWith(".wmo"))
            {
                WMOLoader.LoadWMO(filename, cache);
                isWMO = true;
            }

            //var normalAttrib = GL.GetAttribLocation(shaderProgram, "normal");
            //GL.EnableVertexAttribArray(normalAttrib);
            //GL.VertexAttribPointer(normalAttrib, 3, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 0);

            var texCoordAttrib = GL.GetAttribLocation(shaderProgram, "texCoord");
            GL.EnableVertexAttribArray(texCoordAttrib);
            GL.VertexAttribPointer(texCoordAttrib, 2, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 3);

            var posAttrib = GL.GetAttribLocation(shaderProgram, "position");
            GL.EnableVertexAttribArray(posAttrib);
            GL.VertexAttribPointer(posAttrib, 3, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 5);

            if (!isWMO)
            {
                ActiveCamera.Pos = new Vector3((cache.doodadBatches[filename].boundingBox.max.Z) + 11.0f, 0.0f, 4.0f);
            }
            else
            {
                // WMO
                for (int j = 0; j < cache.worldModelBatches[filename].wmoRenderBatch.Length; j++)
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, cache.worldModelBatches[filename].groupBatches[cache.worldModelBatches[filename].wmoRenderBatch[j].groupID].vertexBuffer);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, cache.worldModelBatches[filename].groupBatches[cache.worldModelBatches[filename].wmoRenderBatch[j].groupID].indiceBuffer);
                }
            }

            GL.BindVertexArray(0);

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

            shaderProgram = Shader.CompileShader();

            ActiveCamera.setupGLRenderMatrix(shaderProgram);

            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        private void RenderCanvas_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            if (!ready) return;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Viewport(0, 0, renderCanvas.Width, renderCanvas.Height);
            GL.Enable(EnableCap.Texture2D);

            ActiveCamera.setupGLRenderMatrix(shaderProgram);

            GL.BindVertexArray(vertexAttribObject);

            if (!isWMO)
            {
                // M2
                GL.BindBuffer(BufferTarget.ArrayBuffer, cache.doodadBatches[filename].vertexBuffer);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, cache.doodadBatches[filename].indiceBuffer);

                for (int i = 0; i < cache.doodadBatches[filename].submeshes.Length; i++)
                {
                    GL.BindTexture(TextureTarget.Texture2D, cache.doodadBatches[filename].submeshes[i].material);
                    GL.DrawRangeElements(PrimitiveType.Triangles, cache.doodadBatches[filename].submeshes[i].firstFace, (cache.doodadBatches[filename].submeshes[i].firstFace + cache.doodadBatches[filename].submeshes[i].numFaces), (int)cache.doodadBatches[filename].submeshes[i].numFaces, DrawElementsType.UnsignedInt, new IntPtr(cache.doodadBatches[filename].submeshes[i].firstFace * 4));
                }
            }
            else
            {
                // WMO 
                for (int j = 0; j < cache.worldModelBatches[filename].wmoRenderBatch.Length; j++)
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, cache.worldModelBatches[filename].groupBatches[cache.worldModelBatches[filename].wmoRenderBatch[j].groupID].vertexBuffer);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, cache.worldModelBatches[filename].groupBatches[cache.worldModelBatches[filename].wmoRenderBatch[j].groupID].indiceBuffer);

                    GL.BindTexture(TextureTarget.Texture2D, cache.worldModelBatches[filename].wmoRenderBatch[j].materialID[0]);
                    GL.DrawRangeElements(PrimitiveType.Triangles, cache.worldModelBatches[filename].wmoRenderBatch[j].firstFace, (cache.worldModelBatches[filename].wmoRenderBatch[j].firstFace + cache.worldModelBatches[filename].wmoRenderBatch[j].numFaces), (int)cache.worldModelBatches[filename].wmoRenderBatch[j].numFaces, DrawElementsType.UnsignedInt, new IntPtr(cache.worldModelBatches[filename].wmoRenderBatch[j].firstFace * 4));
                }
            }

            var error = GL.GetError().ToString();

            if (error != "NoError")
            {
                throw new Exception(error);
            }

            renderCanvas.SwapBuffers();
        }

        public void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            Update();
            renderCanvas.Invalidate();
        }
    }
}
