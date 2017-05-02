using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OBJExporterUI.Loaders;
using System.Drawing;
using OpenTK.Input;

namespace OBJExporterUI
{
    public class PreviewControl
    {
        private GLControl renderCanvas;

        private bool ready = false;
        private bool isWMO;

        // Cache storage for models... bad idea?
        private CacheStorage cache = new CacheStorage();

        // Camera stuff
        private bool mouseDragging = true;
        private static float dragX;
        private static float dragY;
        private static float dragZ;
        private static float angle;
        private static float MDDepth = 0;
        private static float MDHorizontal = 0;
        private static float MDVertical = 0;
        private static float camSpeed = 0.25f;
        private OldCamera ActiveCamera;
        private Point mouseOldCoords;

        private string filename;

        public PreviewControl(GLControl renderCanvas)
        {
            this.renderCanvas = renderCanvas;
            this.renderCanvas.Paint += RenderCanvas_Paint;
            this.renderCanvas.Load += RenderCanvas_Load;
            this.renderCanvas.KeyDown += RenderCanvas_KeyDown;

            dragX = 0;
            dragY = 0;
            dragZ = 0;
            angle = 0.0f;

            ActiveCamera = new OldCamera(renderCanvas.Width, renderCanvas.Height);
        }

        private void RenderCanvas_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            // Console.WriteLine(e.KeyCode);
        }

        public void LoadModel(string filename)
        {
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

            this.filename = filename;

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

            MDVertical = 0;
            MDDepth = 0;
            MDHorizontal = 0;

            if (keyboardState.IsKeyDown(Key.I))
            {
                Console.WriteLine("Camera position: " + ActiveCamera.Pos);
            }

            if (keyboardState.IsKeyDown(Key.Q))
            {
                MDVertical = 1;
            }

            if (keyboardState.IsKeyDown(Key.E))
            {
                MDVertical = -1;
            }

            if (keyboardState.IsKeyDown(Key.W))
            {
                MDDepth = 1;
            }

            if (keyboardState.IsKeyDown(Key.S))
            {
                MDDepth = -1;
            }

            if (keyboardState.IsKeyDown(Key.A))
            {
                MDHorizontal = -1;
            }

            if (keyboardState.IsKeyDown(Key.D))
            {
                MDHorizontal = 1;
            }

            if (keyboardState.IsKeyDown(Key.O))
            {
                camSpeed = camSpeed + 0.025f;
            }

            if (keyboardState.IsKeyDown(Key.P))
            {
                camSpeed = camSpeed - 0.025f;
            }

            if (keyboardState.IsKeyDown(Key.ShiftLeft))
            {
                if (keyboardState.IsKeyDown(Key.Up))
                {
                    dragY = dragY - camSpeed;
                }

                if (keyboardState.IsKeyDown(Key.Down))
                {
                    dragY = dragY + camSpeed;
                }

                if (keyboardState.IsKeyDown(Key.Left))
                {
                    dragX = dragX - camSpeed;
                }

                if (keyboardState.IsKeyDown(Key.Right))
                {
                    dragX = dragX + camSpeed;
                }
            }

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (!mouseDragging)
                {
                    mouseDragging = true;
                    mouseOldCoords = new Point(mouseState.X, mouseState.Y);
                }

                Point mouseNewCoords = new Point(mouseState.X, mouseState.Y);

                int mouseMovementY = (mouseNewCoords.Y - mouseOldCoords.Y);
                int mouseMovementX = (mouseNewCoords.X - mouseOldCoords.X);

                dragY = dragY + mouseMovementY / 20.0f;
                dragX = dragX + mouseMovementX / 20.0f;

                if (dragY < -89)
                {
                    dragY = -89;
                }
                else if (dragY > 89)
                {
                    dragY = 89;
                }

                mouseOldCoords = mouseNewCoords;
            }

            if (mouseState.LeftButton == ButtonState.Released)
            {
                mouseDragging = false;
            }

            if (keyboardState.IsKeyDown(Key.R))//Reset
            {
                dragX = dragY = dragZ = angle = 0;
            }

            dragZ = (mouseState.WheelPrecise / 2) - 500; //Startzoom is at -7.5f 
        }

        private void RenderCanvas_Load(object sender, EventArgs e)
        {
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.DepthTest);
            GL.ShadeModel(ShadingModel.Smooth);
            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            ActiveCamera.Pos = new Vector3(dragX, dragY, dragZ);
            ActiveCamera.setupGLRenderMatrix();
        }

        private void DrawAxes()
        {
            GL.Begin(PrimitiveType.Lines);

            GL.Color3(Color.DarkRed);  // x axis
            GL.Vertex3(-10, 0, 0);
            GL.Vertex3(10, 0, 0);

            GL.Color3(Color.ForestGreen);  // y axis
            GL.Vertex3(0, -10, 0);
            GL.Vertex3(0, 10, 0);

            GL.Color3(Color.LightBlue);  // z axis
            GL.Vertex3(0, 0, -10);
            GL.Vertex3(0, 0, 10);

            GL.End();
        }

        private void RenderCanvas_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            if (!ready) return;

            ActiveCamera.tick(0.02f, dragX, dragY, MDHorizontal, MDDepth, MDVertical);
            ActiveCamera.setupGLRenderMatrix();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Viewport(0, 0, renderCanvas.Width, renderCanvas.Height);
            GL.Enable(EnableCap.Texture2D);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);

            if (!isWMO)
            {
                // M2
                GL.BindBuffer(BufferTarget.ArrayBuffer, cache.doodadBatches[filename].vertexBuffer);
                GL.NormalPointer(NormalPointerType.Float, 8 * sizeof(float), (IntPtr)0);
                GL.TexCoordPointer(2, TexCoordPointerType.Float, 8 * sizeof(float), (IntPtr)(3 * sizeof(float)));
                GL.VertexPointer(3, VertexPointerType.Float, 8 * sizeof(float), (IntPtr)(5 * sizeof(float)));
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, cache.doodadBatches[filename].indiceBuffer);

                for (int i = 0; i < cache.doodadBatches[filename].submeshes.Length; i++)
                {
                    if (cache.doodadBatches[filename].submeshes[i].blendType == 0)
                    {
                        GL.Disable(EnableCap.Blend);
                    }
                    else
                    {
                        GL.Enable(EnableCap.Blend);
                        GL.BlendEquation(BlendEquationMode.FuncAdd);
                    }

                    switch (cache.doodadBatches[filename].submeshes[i].blendType)
                    {
                        case 0: //Combiners_Opaque (Blend disabled)
                            break;
                        case 1: //Combiners_Mod (Blend enabled, Src = ONE, Dest = ZERO, SrcAlpha = ONE, DestAlpha = ZERO)
                            GL.Enable(EnableCap.Blend);
                            //GL.BlendFuncSeparate(BlendingFactorSrc.One, BlendingFactorDest.Zero, BlendingFactorSrc.One, BlendingFactorDest.Zero);
                            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.Zero);
                            break;
                        case 2: //Combiners_Decal (Blend enabled, Src = SRC_ALPHA, Dest = INV_SRC_ALPHA, SrcAlpha = SRC_ALPHA, DestAlpha = INV_SRC_ALPHA )
                            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                            break;
                        case 3: //Combiners_Add (Blend enabled, Src = SRC_COLOR, Dest = DEST_COLOR, SrcAlpha = SRC_ALPHA, DestAlpha = DEST_ALPHA )
                            GL.BlendFuncSeparate(BlendingFactorSrc.SrcColor, BlendingFactorDest.DstColor, BlendingFactorSrc.SrcAlpha, BlendingFactorDest.DstAlpha);
                            break;
                        case 4: //Combiners_Mod2x (Blend enabled, Src = SRC_ALPHA, Dest = ONE, SrcAlpha = SRC_ALPHA, DestAlpha = ONE )
                            GL.BlendFuncSeparate(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One, BlendingFactorSrc.Zero, BlendingFactorDest.One);
                            break;
                        case 5: //Combiners_Fade (Blend enabled, Src = SRC_ALPHA, Dest = INV_SRC_ALPHA, SrcAlpha = SRC_ALPHA, DestAlpha = INV_SRC_ALPHA )
                            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                            break;
                        case 6: //Used in the Deeprun Tram subway glass, supposedly (Blend enabled, Src = DEST_COLOR, Dest = SRC_COLOR, SrcAlpha = DEST_ALPHA, DestAlpha = SRC_ALPHA )
                            GL.BlendFuncSeparate(BlendingFactorSrc.DstColor, BlendingFactorDest.SrcColor, BlendingFactorSrc.DstAlpha, BlendingFactorDest.SrcAlpha);
                            break;
                        case 7: //World\Expansion05\Doodads\Shadowmoon\Doodads\6FX_Fire_Grassline_Doodad_blue_LARGE.m2
                            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
                            break;
                        default:
                            throw new Exception("Unknown blend type " + cache.doodadBatches[filename].submeshes[i].blendType);
                    }
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
                    GL.NormalPointer(NormalPointerType.Float, 8 * sizeof(float), (IntPtr)0);
                    GL.TexCoordPointer(2, TexCoordPointerType.Float, 8 * sizeof(float), (IntPtr)(3 * sizeof(float)));
                    GL.VertexPointer(3, VertexPointerType.Float, 8 * sizeof(float), (IntPtr)(5 * sizeof(float)));
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, cache.worldModelBatches[filename].groupBatches[cache.worldModelBatches[filename].wmoRenderBatch[j].groupID].indiceBuffer);

                    if (cache.worldModelBatches[filename].wmoRenderBatch[cache.worldModelBatches[filename].wmoRenderBatch[j].groupID].blendType == 0)
                    {
                        GL.Disable(EnableCap.Blend);
                    }
                    else
                    {
                        GL.Enable(EnableCap.Blend);
                        GL.BlendEquation(BlendEquationMode.FuncAdd);
                    }

                    switch (cache.worldModelBatches[filename].wmoRenderBatch[cache.worldModelBatches[filename].wmoRenderBatch[j].groupID].blendType)
                    {
                        case 0: //Combiners_Opaque (Blend disabled)
                            break;
                        case 1: //Combiners_Mod (Blend enabled, Src = ONE, Dest = ZERO, SrcAlpha = ONE, DestAlpha = ZERO)
                            GL.Enable(EnableCap.Blend);
                            //GL.BlendFuncSeparate(BlendingFactorSrc.One, BlendingFactorDest.Zero, BlendingFactorSrc.One, BlendingFactorDest.Zero);
                            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.Zero);
                            break;
                        case 2: //Combiners_Decal (Blend enabled, Src = SRC_ALPHA, Dest = INV_SRC_ALPHA, SrcAlpha = SRC_ALPHA, DestAlpha = INV_SRC_ALPHA )
                            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                            break;
                        case 3: //Combiners_Add (Blend enabled, Src = SRC_COLOR, Dest = DEST_COLOR, SrcAlpha = SRC_ALPHA, DestAlpha = DEST_ALPHA )
                            GL.BlendFuncSeparate(BlendingFactorSrc.SrcColor, BlendingFactorDest.DstColor, BlendingFactorSrc.SrcAlpha, BlendingFactorDest.DstAlpha);
                            break;
                        case 4: //Combiners_Mod2x (Blend enabled, Src = SRC_ALPHA, Dest = ONE, SrcAlpha = SRC_ALPHA, DestAlpha = ONE )
                            GL.BlendFuncSeparate(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One, BlendingFactorSrc.Zero, BlendingFactorDest.One);
                            break;
                        case 5: //Combiners_Fade (Blend enabled, Src = SRC_ALPHA, Dest = INV_SRC_ALPHA, SrcAlpha = SRC_ALPHA, DestAlpha = INV_SRC_ALPHA )
                            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                            break;
                        case 6: //Used in the Deeprun Tram subway glass, supposedly (Blend enabled, Src = DEST_COLOR, Dest = SRC_COLOR, SrcAlpha = DEST_ALPHA, DestAlpha = SRC_ALPHA )
                            GL.BlendFuncSeparate(BlendingFactorSrc.DstColor, BlendingFactorDest.SrcColor, BlendingFactorSrc.DstAlpha, BlendingFactorDest.SrcAlpha);
                            break;
                        case 7: //World\Expansion05\Doodads\Shadowmoon\Doodads\6FX_Fire_Grassline_Doodad_blue_LARGE.m2
                            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
                            break;
                        default:
                            throw new Exception("Unknown blend type " + cache.worldModelBatches[filename].wmoRenderBatch[cache.worldModelBatches[filename].wmoRenderBatch[j].groupID].blendType);
                    }
                    GL.BindTexture(TextureTarget.Texture2D, cache.worldModelBatches[filename].wmoRenderBatch[j].materialID[0]);
                    GL.DrawRangeElements(PrimitiveType.Triangles, cache.worldModelBatches[filename].wmoRenderBatch[j].firstFace, (cache.worldModelBatches[filename].wmoRenderBatch[j].firstFace + cache.worldModelBatches[filename].wmoRenderBatch[j].numFaces), (int)cache.worldModelBatches[filename].wmoRenderBatch[j].numFaces, DrawElementsType.UnsignedInt, new IntPtr(cache.worldModelBatches[filename].wmoRenderBatch[j].firstFace * 4));
                }

            }

            var error = GL.GetError().ToString();

            if (error != "NoError")
            {
                Console.WriteLine(error);
            }
            GL.Flush();
            renderCanvas.SwapBuffers();
        }

        public void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            Update();
            renderCanvas.Invalidate();
        }
    }
}
