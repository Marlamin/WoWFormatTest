using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using WoWFormatLib.FileReaders;
using OBJExporterUI.Loaders;
using System.ComponentModel;

namespace OBJExporterUI
{
    public class PreviewWindow : GameWindow
    {
        private static float dragX;
        private static float dragY;
        private static float dragZ;
        private static float angle;
        private static float MDDepth = 0;
        private static float MDHorizontal = 0;
        private static float MDVertical = 0;
        private static float lightHeight = 0.0f;
        private static float camSpeed = 0.25f;
        private List<Terrain> adts = new List<Terrain>();

        private string filename; 

        private static float maxSize = 51200 / 3; //17066,66666666667
	    private static float mapSize = maxSize * 2; //34133,33333333333
	    private static float adtSize = mapSize / 64; //533,3333333333333

        private Dictionary<Key, int> CoolOffKeys = new Dictionary<Key, int>();

        private bool mouseDragging = true;

        private bool isWMO;

        private Point mouseOldCoords;

        private CacheStorage cache = new CacheStorage();

        OldCamera ActiveCamera;

        public PreviewWindow(string modelPath)
            : base(1920, 1080, new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8), "Model preview", GameWindowFlags.Default, DisplayDevice.Default, 3, 0, OpenTK.Graphics.GraphicsContextFlags.Default)
        {

            dragX = 0;
            dragY = 0;
            dragZ = 0;
            angle = 0.0f;

            Keyboard.KeyDown += Keyboard_KeyDown;

            ActiveCamera = new OldCamera(Width, Height);

            filename = modelPath;

            if (filename.EndsWith(".m2"))
            {
                Exit();
                //M2Loader.LoadM2(filename, cache);
                isWMO = false;
            }
            else if (filename.EndsWith(".wmo"))
            {
                WMOLoader.LoadWMO(filename, cache);
                isWMO = true;
            }
        }

        void Keyboard_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Exit();
            }
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

        protected override void OnLoad(EventArgs e)
        {

            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.DepthTest);
            GL.ShadeModel(ShadingModel.Smooth);
            GL.ClearColor(Color.White);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            ActiveCamera.Pos = new Vector3(dragX, dragY, dragZ);
            ActiveCamera.setupGLRenderMatrix();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            ActiveCamera = new OldCamera(Width, Height)
            {
                Pos = new Vector3(dragX, dragY, dragZ)
            };
            ActiveCamera.setupGLRenderMatrix();
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (!this.Focused)
            {
                return;
            }

            MouseState mouseState = OpenTK.Input.Mouse.GetState();
            KeyboardState keyboardState = OpenTK.Input.Keyboard.GetState();

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
                MDHorizontal =  1;
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

            if (keyboardState.IsKeyDown(Key.L))
            {
                lightHeight = lightHeight + 50f;
            }
            if (keyboardState.IsKeyDown(Key.K))
            {
                lightHeight = lightHeight - 50f;
            }

            /*
            if (keyboardState.IsKeyDown(Key.X))
            {
                dragZ = (dragZ + 10f) - 1068;
            }

            if (keyboardState.IsKeyDown(Key.Z))
            {
                dragZ = (dragZ - 10f) - 1068;
            }
            */

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

                if (dragY < -89) {
                    dragY = -89;
                } else if (dragY > 89) {
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

            if (!CoolOffKeys.ContainsKey(Key.Right) && keyboardState.IsKeyDown(Key.Right) && keyboardState.IsKeyUp(Key.ShiftLeft))
            {
                angle += 90;
                CoolOffKey(Key.Right);
            }

            if (!CoolOffKeys.ContainsKey(Key.Left) && keyboardState.IsKeyDown(Key.Left) && keyboardState.IsKeyUp(Key.ShiftLeft))
            {
                angle -= 90;
                CoolOffKey(Key.Left);
            }

            dragZ = (mouseState.WheelPrecise / 2) - 500; //Startzoom is at -7.5f 
        }

        private void CoolOffKey(Key kKey)
        {
            if (CoolOffKeys.ContainsKey(kKey))
                CoolOffKeys[kKey] = 1000;
            else CoolOffKeys.Add(kKey, 1000);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            MakeCurrent();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            angle = angle % 360;

            //Position math

            ActiveCamera.tick((float)e.Time, dragX, dragY, MDHorizontal, MDDepth, MDVertical);
            ActiveCamera.setupGLRenderMatrix();

            ActiveCamera.tick(0.02f, dragX, dragY, MDHorizontal, MDDepth, MDVertical);

            ActiveCamera.setupGLRenderMatrix();

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

                for (int j = 0; j < cache.worldModelBatches[filename].wmoRenderBatch.Count(); j++)
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

            if (GL.GetError().ToString() != "NoError")
            {
                Console.WriteLine(GL.GetError().ToString());
            }

            this.SwapBuffers();

            Key[] keys = CoolOffKeys.Keys.ToArray();
            int decreasevalue = (int)(base.RenderTime * 3000d);//TERRIBLE
            for (int i = 0; i < keys.Length; i++)
            {
                Key k = keys[i];

                CoolOffKeys[k] -= decreasevalue;

                if (CoolOffKeys[k] <= 0)
                    CoolOffKeys.Remove(k);
            }
            
        }

        protected override void OnUnload(EventArgs e)
        {
            Dispose();
            base.OnUnload(e);
        }

        public struct Terrain
        {
            public int vertexBuffer;
            public int indiceBuffer;
            public RenderBatch[] renderBatches;
            public Doodad[] doodads;
            public WorldModelBatch[] worldModelBatches;
        }

        public struct Vertex
        {
            public Vector3 Normal;
            public Vector3 Color;
            public Vector2 TexCoord;
            public Vector3 Position;
        }

        public struct M2Vertex
        {
            public Vector3 Normal;
            public Vector2 TexCoord;
            public Vector3 Position;

        }
        public struct Material
        {
            public string filename;
            public int textureID;
            internal WoWFormatLib.Structs.M2.TextureFlags flags;
            internal uint texture1;
        }

        public struct RenderBatch
        {
            public uint firstFace;
            public uint numFaces;
            public uint[] materialID;
            /* WMO ONLY */
            public uint groupID;
            public uint blendType;
            /* ADT ONLY */
            public int[] alphaMaterialID;
        }

        public struct Doodad
        {
            public string filename;
            public Vector3 position;
            public Vector3 rotation;
            public float scale;
        }

        public struct DoodadBatch
        {
            public int vertexBuffer;
            public int indiceBuffer;
            public uint[] indices;
            public Submesh[] submeshes;
            public Material[] mats;
        }

        public struct WorldModelBatch
        {
            public Vector3 position;
            public Vector3 rotation;
            public WorldModel worldModel;
        }

        public struct WMODoodad
        {
            public string filename;
            public short flags;
            public Vector3 position;
            public Quaternion rotation;
            public float scale;
            public Vector4 color;
        }

        public struct Submesh
        {
            public uint firstFace;
            public uint numFaces;
            public uint material;
            public uint blendType;
            public uint groupID;
        }

        public struct WorldModel
        {
            public WorldModelGroupBatches[] groupBatches;
            public Material[] mats;
            public RenderBatch[] wmoRenderBatch;
            public WMODoodad[] doodads;
        }

        public struct WorldModelGroupBatches
        {
            public int vertexBuffer;
            public int indiceBuffer;
            public uint[] indices;
        }
    }
}
