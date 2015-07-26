using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using WoWFormatLib.FileReaders;

namespace WoWOpenGL
{
    public class TerrainWindow : GameWindow
    {
        bool modelLoaded = false;

        private static float dragX;
        private static float dragY;
        private static float dragZ;
        private static float angle;
        private static float camSpeed = 0.25f;
        private uint[] VBOid;

        private int[] indices;

        Camera ActiveCamera;

        public TerrainWindow(string modelPath)
            : base(1920, 1080, new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8), "Terrain test", GameWindowFlags.Default, DisplayDevice.Default, 3, 0, OpenTK.Graphics.GraphicsContextFlags.Default)
        {
            dragX = 227;
            dragY = 152;
            dragZ = 2868;
            angle = 90.0f;

            Keyboard.KeyDown += Keyboard_KeyDown;

            ActiveCamera = new Camera(Width, Height);
            ActiveCamera.Pos = new Vector3(10.0f, -10.0f, -7.5f);

            Console.WriteLine(modelPath);

            string[] adt = modelPath.Split('_');

            Console.WriteLine("MAP {0}, X {1}, Y {2}", adt[0], adt[1], adt[2]);
            //LoadADT(adt[0], adt[1], adt[2]);
            LoadMap(adt[0], int.Parse(adt[1]), int.Parse(adt[2]), 2);

            modelLoaded = true;
        }
        
        void Keyboard_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Exit();

            if (e.Key == Key.F11)
                if (this.WindowState == WindowState.Fullscreen)
                    this.WindowState = WindowState.Normal;
                else
                    this.WindowState = WindowState.Fullscreen;
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

        private void LoadMap(string map, int centerx, int centery, int distance)
        {
            float TileSize = 1600.0f / 3.0f; //533.333
            float ChunkSize = TileSize / 16.0f; //33.333
            float UnitSize = ChunkSize / 8.0f; //4.166666 // ~~fun fact time with marlamin~~ this times 0.5 ends up being pixelspercoord on minimap
            float MapMidPoint = 32.0f / ChunkSize;
            GL.EnableClientState(ArrayCap.VertexArray);

            VBOid = new uint[2];
            GL.GenBuffers(2, VBOid);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[1]);

            List<Vector3> verticelist = new List<Vector3>();
            List<Int32> indicelist = new List<Int32>();

            for (int x = centerx; x < centerx + distance; x++)
            {
                for (int y = centery; y < centery + distance; y++)
                {
                    string filename = "World/Maps/" + map + "/" + map + "_" + y + "_" + x + ".adt";

                    if (WoWFormatLib.Utils.CASC.FileExists(filename))
                    {
                        ADTReader reader = new ADTReader();
                        reader.LoadADT(filename);
                        var initialChunkY = reader.adtfile.chunks[0].header.position.Y;
                        var initialChunkX = reader.adtfile.chunks[0].header.position.X;

                        for (uint c = 0; c < reader.adtfile.chunks.Count(); c++)
                        {
                            var chunk = reader.adtfile.chunks[c];
                            //Console.WriteLine("Reading ADT chunk " + c);
                            //Console.WriteLine("ADT is at position " + reader.adtfile.chunks[c].header.position.ToString());
                            //Console.WriteLine("ADT has " + reader.adtfile.chunks[c].vertices.vertices.Count() + " vertices!");

                            //int off = chunk.vertices.vertices.Count();

                            int off = verticelist.Count();

                            for (int i = 0, idx = 0; i < 17; i++)
                            {
                                for (int j = 0; j < (((i % 2) != 0) ? 8 : 9); j++)
                                {
                                    //var v = new Vector3(chunk.header.position.Y - (j * UnitSize), chunk.vertices.vertices[idx++] + chunk.header.position.Z, chunk.header.position.X - (i * UnitSize * 0.5f));
                                    var v = new Vector3(chunk.header.position.Y - (j * UnitSize), chunk.vertices.vertices[idx++] + chunk.header.position.Z, -(chunk.header.position.X - (i * UnitSize * 0.5f)));
                                    if ((i % 2) != 0) v.X -= 0.5f * UnitSize;
                                    verticelist.Add(v);
                                }
                            }

                            //Console.WriteLine("First vertice at " + verticelist.First().X + "x" + verticelist.First().Y);

                            for (int j = 9; j < 145; j++)
                            {
                                
                                // if (!MCNK.HasHole(unitidx % 8, unitidx++ / 8))
                                // {
                                indicelist.AddRange(new Int32[] { off + j + 8, off + j - 9, off + j });
                                indicelist.AddRange(new Int32[] { off + j - 9, off + j - 8, off + j });
                                indicelist.AddRange(new Int32[] { off + j - 8, off + j + 9, off + j });
                                indicelist.AddRange(new Int32[] { off + j + 9, off + j + 8, off + j });
                                // }
                                if ((j + 1) % (9 + 8) == 0) j += 9;
                            }

                        }


                    }
                }
            }

            indices = indicelist.ToArray();
            Vector3[] vertices = verticelist.ToArray();
            Console.WriteLine("Vertices in array: " + vertices.Count()); //37120, correct

            //indices = indicelist.ToArray();
            Console.WriteLine("Indices in array: " + indices.Count()); //196608, should be 65.5k which is 196608 / 3. in triangles so its correct?

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Count() * 3 * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[1]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(int)), indices, BufferUsageHint.StaticDraw);

            int verticeBufferSize = 0;
            int indiceBufferSize = 0;

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out verticeBufferSize);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[1]);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out indiceBufferSize);

            Console.WriteLine("Vertices in buffer: " + verticeBufferSize / 3 / sizeof(float));
            Console.WriteLine("Indices in buffer: " + indiceBufferSize / sizeof(int));
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            ActiveCamera.Pos = new Vector3(dragX, dragY, dragZ);
            ActiveCamera.setupGLRenderMatrix();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            ActiveCamera = new Camera(Width, Height);
            ActiveCamera.Pos = new Vector3(dragX, dragY, dragZ);
            ActiveCamera.setupGLRenderMatrix();
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            MouseState mouseState = OpenTK.Input.Mouse.GetState();
            KeyboardState keyboardState = OpenTK.Input.Keyboard.GetState();

            if (keyboardState.IsKeyDown(Key.I))
            {
                Console.WriteLine("Camera position: " + ActiveCamera.Pos);
                Console.WriteLine("Camera direction: " + ActiveCamera.Dir);
            }

            if (keyboardState.IsKeyDown(Key.Q))
            {
                angle = angle + 0.5f;
            }

            if (keyboardState.IsKeyDown(Key.E))
            {
                angle = angle - 0.5f;
            }

            if (keyboardState.IsKeyDown(Key.O))
            {
                camSpeed = camSpeed + 0.025f;
            }

            if (keyboardState.IsKeyDown(Key.P))
            {
                camSpeed = camSpeed - 0.025f;
            }

            if (keyboardState.IsKeyDown(Key.Up))
            {
                dragY = dragY + camSpeed;
            }

            if (keyboardState.IsKeyDown(Key.Down))
            {
                dragY = dragY - camSpeed;
            }

            if (keyboardState.IsKeyDown(Key.Left))
            {
                dragX = dragX + camSpeed;
            }

            if (keyboardState.IsKeyDown(Key.Right))
            {
                dragX = dragX - camSpeed;
            }

            dragZ = (mouseState.WheelPrecise / 10) - 2868; //Startzoom is at -7.5f 
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            ActiveCamera.Pos = new Vector3(dragX, dragY, dragZ);
            ActiveCamera.setupGLRenderMatrix();
            GL.Rotate(angle, 0.0, 1.0f, 0.0);
            DrawAxes();

            GL.Enable(EnableCap.VertexArray);
            GL.EnableClientState(ArrayCap.VertexArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);
            GL.VertexPointer(3, VertexPointerType.Float, 0, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[1]);

            Console.WriteLine("Drawing " + indices.Count() + " indices");
            GL.DrawElements(PrimitiveType.Triangles, indices.Count(), DrawElementsType.UnsignedInt, 0);
            //GL.DrawArrays(PrimitiveType.Points, 0, indices.Count());
            this.SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            Dispose();
            base.OnUnload(e);
            System.Windows.Application.Current.Shutdown();
        }
    }
}
