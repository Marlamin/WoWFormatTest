using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using WoWFormatLib.FileReaders;
using OpenTK.Input;
using System.Timers;
using WoWOpenGL.Loaders;
using System.ComponentModel;
using System.Diagnostics;

namespace WoWOpenGL
{
    public class Render : GameWindow
    {
        private static OldCamera ActiveCamera;

        private GLControl glControl;
        private bool gLoaded = false;

        private static bool isWMO = false;

        private static float dragX;
        private static float dragY;
        private static float dragZ;

        private static bool mouseDragging = true;
        private static Point mouseOldCoords;
        private static bool mouseOverRenderArea = false;
        private static float MDDepth = 0;
        private static float MDHorizontal = 0;
        private static float MDVertical = 0;

        private BackgroundWorker worker;

        private CacheStorage cache = new CacheStorage();

        private Stopwatch sw = new Stopwatch();

        private double spentTime;

        private string filename;

        public Render(string ModelPath, BackgroundWorker worker = null)
        {
            dragX = 0.0f;
            dragY = 0.0f;
            dragZ = 0.0f;

            if (worker == null)
            {
                this.worker = new BackgroundWorker();
            }
            else
            {
                this.worker = worker;
            }

            filename = ModelPath;

            System.Windows.Forms.Integration.WindowsFormsHost wfc = MainWindow.winFormControl;

            ActiveCamera = new OldCamera((int)wfc.ActualWidth, (int)wfc.ActualHeight);
            ActiveCamera.Pos = new Vector3(-15.0f, 0.0f, 4.0f);

            if (filename.EndsWith(".m2"))
            {
                M2Loader.LoadM2(filename, cache);

                ActiveCamera.Pos = new Vector3(-15.0f, 0.0f, 4.0f);

                gLoaded = true;
            }
            else if (filename.EndsWith(".wmo"))
            {
                WMOLoader.LoadWMO(filename, cache);
                
                gLoaded = true;
                isWMO = true;
            }

            glControl = new GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8), 3, 0, OpenTK.Graphics.GraphicsContextFlags.Default);
            glControl.Width = (int)wfc.ActualWidth;
            glControl.Height = (int)wfc.ActualHeight;
            glControl.Left = 0;
            glControl.Top = 0;
            glControl.Load += glControl_Load;
            glControl.Paint += RenderFrame;
            glControl.MouseEnter += glControl_MouseEnter;
            glControl.MouseLeave += glControl_MouseLeave;
            glControl.Resize += glControl_Resize;
            glControl.MakeCurrent();
            glControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;

            sw.Start();

            spentTime = 0.00;

            wfc.Child = glControl;
        }

        private void glControl_Resize(object sender, EventArgs e)
        {
            Console.WriteLine("GLControl resized. Please load a different model to have the viewport resize as well.");
        }

        private void glControl_MouseEnter(object sender, EventArgs e)
        {
            mouseOverRenderArea = true;
            MainWindow.filterBox.IsEnabled = false;
            MainWindow.modelListBox.IsEnabled = false;
            MainWindow.mapsTab.IsEnabled = false;
        }

        private void glControl_MouseLeave(object sender, EventArgs e)
        {
            mouseOverRenderArea = false;
            MainWindow.filterBox.IsEnabled = true;
            MainWindow.modelListBox.IsEnabled = true;
            MainWindow.mapsTab.IsEnabled = true;
        }

        private void glControl_Load(object sender, EventArgs e)
        {

            Console.WriteLine("Loading GLcontrol..");
            glControl.MakeCurrent();
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.DepthTest);

            GL.ClearColor(OpenTK.Graphics.Color4.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            InitializeInputTick();
            ActiveCamera.setupGLRenderMatrix();
            Console.WriteLine("GLcontrol is done loading!");

        }

        private static void InitializeInputTick()
        {
            var timer = new Timer();
            timer.Enabled = true;
            timer.Interval = 1000 / 60;
            timer.Elapsed += new ElapsedEventHandler(InputTick);
            timer.Start();
        }

        private static void InputTick(object sender, EventArgs e)
        {
            float speed = 0.01f * (float)ControlsWindow.camSpeed;

            MouseState mouseState = OpenTK.Input.Mouse.GetState();
            KeyboardState keyboardState = OpenTK.Input.Keyboard.GetState();

            MDVertical = 0;
            MDDepth = 0;
            MDHorizontal = 0;

            if (mouseOverRenderArea)
            {
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

                if (keyboardState.IsKeyDown(Key.I))
                {
                    Console.WriteLine(ActiveCamera.Pos.ToString());
                }
            }

            if (mouseOverRenderArea && mouseState.LeftButton == ButtonState.Pressed)
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

            if (!mouseOverRenderArea || mouseState.LeftButton == ButtonState.Released)
            {
                mouseDragging = false;
            }
        }

        void RenderFrame(object sender, EventArgs e) //This is called every frame
        {
            if (!gLoaded) { return; }
            glControl.MakeCurrent();

            //ActiveCamera.Pos = new Vector3(dragX, dragY, dragZ);

            ActiveCamera.tick(0.001f, dragX, dragY, MDHorizontal, MDDepth, MDVertical);

            ActiveCamera.setupGLRenderMatrix();

            if (!gLoaded) return;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.Texture2D);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);

            GL.Rotate(180f, 0.0, 0.0, 1.0);

            if (!isWMO)
            {
                // M2
                GL.BindBuffer(BufferTarget.ArrayBuffer, cache.doodadBatches[filename].vertexBuffer);
                GL.NormalPointer(NormalPointerType.Float, 8 * sizeof(float), (IntPtr)0);
                GL.TexCoordPointer(2, TexCoordPointerType.Float, 8 * sizeof(float), (IntPtr)(3 * sizeof(float)));
                GL.VertexPointer(3, VertexPointerType.Float, 8 * sizeof(float), (IntPtr)(5 * sizeof(float)));
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, cache.doodadBatches[filename].indiceBuffer);

                for (int i = 0; i < cache.doodadBatches[filename].submeshes.Count(); i++)
                {
                    switch (cache.doodadBatches[filename].submeshes[i].blendType)
                    {
                        case 0: //Combiners_Opaque (Blend disabled)
                            GL.Disable(EnableCap.Blend);
                            break;
                        case 1: //Combiners_Mod (Blend enabled, Src = ONE, Dest = ZERO, SrcAlpha = ONE, DestAlpha = ZERO)
                            GL.Enable(EnableCap.Blend);
                            //Not BlendingFactorSrc.One and BlendingFactorDest.Zero!
                            //GL.BlendFuncSeparate(BlendingFactorSrc.One, BlendingFactorDest.Zero, BlendingFactorSrc.One, BlendingFactorDest.Zero);
                            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                            break;
                        case 2: //Combiners_Decal (Blend enabled, Src = SRC_ALPHA, Dest = INV_SRC_ALPHA, SrcAlpha = SRC_ALPHA, DestAlpha = INV_SRC_ALPHA )
                            GL.Enable(EnableCap.Blend);
                            GL.BlendFuncSeparate(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                            //Tried:
                            //BlendingFactorSrc.SrcAlpha, BlendingFactorDest.DstAlpha
                            //BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha
                            //BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusDstAlpha
                            break;
                        case 3: //Combiners_Add (Blend enabled, Src = SRC_COLOR, Dest = DEST_COLOR, SrcAlpha = SRC_ALPHA, DestAlpha = DEST_ALPHA )
                            GL.Enable(EnableCap.Blend);
                            GL.BlendFunc(BlendingFactorSrc.SrcColor, BlendingFactorDest.DstColor);
                            break;
                        case 4: //Combiners_Mod2x (Blend enabled, Src = SRC_ALPHA, Dest = ONE, SrcAlpha = SRC_ALPHA, DestAlpha = ONE )
                            GL.Enable(EnableCap.Blend);
                            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
                            break;
                        case 5: //Combiners_Fade (Blend enabled, Src = SRC_ALPHA, Dest = INV_SRC_ALPHA, SrcAlpha = SRC_ALPHA, DestAlpha = INV_SRC_ALPHA )
                            GL.Enable(EnableCap.Blend);
                            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                            break;
                        case 6: //Used in the Deeprun Tram subway glass, supposedly (Blend enabled, Src = DEST_COLOR, Dest = SRC_COLOR, SrcAlpha = DEST_ALPHA, DestAlpha = SRC_ALPHA )
                            GL.Enable(EnableCap.Blend);
                            GL.BlendFunc(BlendingFactorSrc.DstColor, BlendingFactorDest.SrcColor);
                            break;
                        case 7: //World\Expansion05\Doodads\Shadowmoon\Doodads\6FX_Fire_Grassline_Doodad_blue_LARGE.m2
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

                for (int i = 0; i < cache.worldModelBatches[filename].groupBatches.Count(); i++)
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, cache.worldModelBatches[filename].groupBatches[i].vertexBuffer);
                    GL.NormalPointer(NormalPointerType.Float, 8 * sizeof(float), (IntPtr)0);
                    GL.TexCoordPointer(2, TexCoordPointerType.Float, 8 * sizeof(float), (IntPtr)(3 * sizeof(float)));
                    GL.VertexPointer(3, VertexPointerType.Float, 8 * sizeof(float), (IntPtr)(5 * sizeof(float)));
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, cache.worldModelBatches[filename].groupBatches[i].indiceBuffer);

                    for (int j = 0; j < cache.worldModelBatches[filename].wmoRenderBatch.Count(); j++)
                    {
                        switch (cache.worldModelBatches[filename].wmoRenderBatch[i].blendType)
                        {
                            case 0: //Combiners_Opaque (Blend disabled)
                                GL.Disable(EnableCap.Blend);
                                break;
                            case 1: //Combiners_Mod (Blend enabled, Src = ONE, Dest = ZERO, SrcAlpha = ONE, DestAlpha = ZERO)
                                GL.Enable(EnableCap.Blend);
                                //Not BlendingFactorSrc.One and BlendingFactorDest.Zero!
                                //GL.BlendFuncSeparate(BlendingFactorSrc.One, BlendingFactorDest.Zero, BlendingFactorSrc.One, BlendingFactorDest.Zero);
                                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                                break;
                            case 2: //Combiners_Decal (Blend enabled, Src = SRC_ALPHA, Dest = INV_SRC_ALPHA, SrcAlpha = SRC_ALPHA, DestAlpha = INV_SRC_ALPHA )
                                GL.Enable(EnableCap.Blend);
                                GL.BlendFuncSeparate(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                                //Tried:
                                //BlendingFactorSrc.SrcAlpha, BlendingFactorDest.DstAlpha
                                //BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha
                                //BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusDstAlpha
                                break;
                            case 3: //Combiners_Add (Blend enabled, Src = SRC_COLOR, Dest = DEST_COLOR, SrcAlpha = SRC_ALPHA, DestAlpha = DEST_ALPHA )
                                GL.Enable(EnableCap.Blend);
                                GL.BlendFunc(BlendingFactorSrc.SrcColor, BlendingFactorDest.DstColor);
                                break;
                            case 4: //Combiners_Mod2x (Blend enabled, Src = SRC_ALPHA, Dest = ONE, SrcAlpha = SRC_ALPHA, DestAlpha = ONE )
                                GL.Enable(EnableCap.Blend);
                                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
                                break;
                            case 5: //Combiners_Fade (Blend enabled, Src = SRC_ALPHA, Dest = INV_SRC_ALPHA, SrcAlpha = SRC_ALPHA, DestAlpha = INV_SRC_ALPHA )
                                GL.Enable(EnableCap.Blend);
                                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                                break;
                            case 6: //Used in the Deeprun Tram subway glass, supposedly (Blend enabled, Src = DEST_COLOR, Dest = SRC_COLOR, SrcAlpha = DEST_ALPHA, DestAlpha = SRC_ALPHA )
                                GL.Enable(EnableCap.Blend);
                                GL.BlendFunc(BlendingFactorSrc.DstColor, BlendingFactorDest.SrcColor);
                                break;
                            case 7: //World\Expansion05\Doodads\Shadowmoon\Doodads\6FX_Fire_Grassline_Doodad_blue_LARGE.m2
                                break;
                            default:
                                throw new Exception("Unknown blend type " + cache.worldModelBatches[filename].wmoRenderBatch[i].blendType);
                        }
                        GL.BindTexture(TextureTarget.Texture2D, cache.worldModelBatches[filename].wmoRenderBatch[j].materialID[0]);
                        GL.DrawRangeElements(PrimitiveType.Triangles, cache.worldModelBatches[filename].wmoRenderBatch[j].firstFace, (cache.worldModelBatches[filename].wmoRenderBatch[j].firstFace + cache.worldModelBatches[filename].wmoRenderBatch[j].numFaces), (int)cache.worldModelBatches[filename].wmoRenderBatch[j].numFaces, DrawElementsType.UnsignedInt, new IntPtr(cache.worldModelBatches[filename].wmoRenderBatch[j].firstFace * 4));
                    }
                }

            }

            var error = GL.GetError().ToString();

            if (error != "NoError")
            {
                Console.WriteLine(error);
            }

            glControl.SwapBuffers();
            glControl.Invalidate();
        }
    }
}