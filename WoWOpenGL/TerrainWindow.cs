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

        private uint[] indices = new uint[768 * 256];

        Camera ActiveCamera;

        public TerrainWindow(string modelPath)
            : base(800, 600, new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8), "Terrain test", GameWindowFlags.Default, DisplayDevice.Default, 3, 0, OpenTK.Graphics.GraphicsContextFlags.Default)
        {
            dragX = 0.0f;
            dragY = 0.0f;
            dragZ = -7.5f;
            angle = 90.0f;

            Keyboard.KeyDown += Keyboard_KeyDown;

            ActiveCamera = new Camera(Width, Height);
            ActiveCamera.Pos = new Vector3(10.0f, -10.0f, -7.5f);

            Console.WriteLine(modelPath);

            string[] adt = modelPath.Split('_');

            Console.WriteLine("MAP {0}, X {1}, Y {2}", adt[0], adt[1], adt[2]);
            LoadADT(adt[0], adt[1], adt[2]);

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

        private void LoadADT(string map, string xx, string yy)
        {
   
            ADTReader reader = new ADTReader();
            reader.LoadADT("World/Maps/" + map + "/" + map + "_" + xx + "_" + yy + ".adt");

            float TileSize = 1600.0f / 3.0f; //533.333
            float ChunkSize = TileSize / 16.0f; //33.333
            float UnitSize = ChunkSize / 8.0f; //4.166666 //this times 0.5 ends up being pixelspercoord on minimap
            float MapMidPoint = 32.0f / ChunkSize;

            GL.EnableClientState(ArrayCap.VertexArray);

            VBOid = new uint[2];
            GL.GenBuffers(2, VBOid);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[1]);

            List<Vector3> verticelist = new List<Vector3>();
            List<uint> indicelist = new List<uint>();

            var initialChunkY = reader.adtfile.chunks[0].header.position.Y;
            var initialChunkX = reader.adtfile.chunks[0].header.position.X;

            for (uint c = 0; c < reader.adtfile.chunks.Count(); c++)
            {
                var chunk = reader.adtfile.chunks[c];
                //Console.WriteLine("Reading ADT chunk " + c);
                //Console.WriteLine("ADT is at position " + reader.adtfile.chunks[c].header.position.ToString());
                //Console.WriteLine("ADT has " + reader.adtfile.chunks[c].vertices.vertices.Count() + " vertices!");

                var posx = chunk.header.position.Y - initialChunkY;
                var posy = chunk.header.position.X - initialChunkX;

                for (int i = 0, idx = 0; i < 17; i++)
                {
                    for (int j = 0; j < (((i % 2) != 0) ? 8 : 9); j++)
                    {
                        var height = chunk.vertices.vertices[idx] + chunk.header.position.Z;
                        var x = posx + j * UnitSize;
                        if ((i % 2) != 0) x += 0.5f * UnitSize;
                        var y = posy - i * UnitSize * 0.5f;

                        //var v = new Vector3(((c / 16) * ChunkSize) - (j * UnitSize * 0.5f), ((c % 16) * ChunkSize) - (i * UnitSize * 0.5f), height);
                        var v = new Vector3(x, y, height); 
                        
                        verticelist.Add(v);
                        //Console.WriteLine(reader.adtfile.chunks[c].vertices.vertices[idx]);
                        idx++;
                        
                    }
                }

                for (uint y = 0; y < 8; ++y)
                {
                    for (uint x = 0; x < 8; ++x)
                    {
                        //var i = ;
                        var i = y * 8 * 12 + x * 12 + (c * 768);
                        //Console.WriteLine(i);
                        indices[i + 0] = y * 17 + x + (c * 145);
                        indices[i + 1] = y * 17 + x + 9 + (c * 145);
                        indices[i + 2] = y * 17 + x + 1 + (c * 145);

                        indices[i + 3] = y * 17 + x + 1 + (c * 145);
                        indices[i + 4] = y * 17 + x + 9 + (c * 145);
                        indices[i + 5] = y * 17 + x + 18 + (c * 145);

                        indices[i + 6] = y * 17 + x + 18 + (c * 145);
                        indices[i + 7] = y * 17 + x + 9 + (c * 145);
                        indices[i + 8] = y * 17 + x + 17 + (c * 145);

                        indices[i + 9] = y * 17 + x + 17 + (c * 145);
                        indices[i + 10] = y * 17 + x + 9 + (c * 145);
                        indices[i + 11] = y * 17 + x + (c * 145);
                        //Console.WriteLine(indices[i + 11].ToString());
                    }
                }
            }


            Vector3[] vertices = verticelist.ToArray();
            Console.WriteLine("Vertices in array: " + vertices.Count()); //37120, correct

            //indices = indicelist.ToArray();
            Console.WriteLine("Indices in array: " + indices.Count()); //196608, should be 65.5k which is 196608 / 3. in triangles so its correct?

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Count() * 3 * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[1]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(uint)), indices, BufferUsageHint.StaticDraw);

            int verticeBufferSize = 0;
            int indiceBufferSize = 0;

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out verticeBufferSize);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[1]);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out indiceBufferSize);

            Console.WriteLine("Vertices in buffer: " + verticeBufferSize / 3 / sizeof(float));
            Console.WriteLine("Indices in buffer: " + indiceBufferSize / sizeof(uint));
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
            OpenTK.Input.MouseState mouseState = OpenTK.Input.Mouse.GetState();
            OpenTK.Input.KeyboardState keyboardState = OpenTK.Input.Keyboard.GetState();

            if (keyboardState.IsKeyDown(Key.I))
            {
                Console.WriteLine("Camera position: " + ActiveCamera.Pos);
                Console.WriteLine("Camera direction: " + ActiveCamera.Dir);
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

            dragZ = (mouseState.WheelPrecise / 10) - 7.5f; //Startzoom is at -7.5f 
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            ActiveCamera.Pos = new Vector3(dragX, dragY, dragZ);
            ActiveCamera.setupGLRenderMatrix();
            GL.Rotate(angle, 0.0, 1.0, 0.0);
            DrawAxes();

            GL.Enable(EnableCap.VertexArray);
            GL.EnableClientState(ArrayCap.VertexArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);
            GL.VertexPointer(3, VertexPointerType.Float, 0, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[1]);

            GL.DrawElements(PrimitiveType.Triangles, indices.Count(), DrawElementsType.UnsignedInt, 0);

            this.SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            Dispose();
            base.OnUnload(e);
            System.Windows.Application.Current.Shutdown();
        }

        public struct Triangle<T>
        {
            public T V0;
            public T V1;
            public T V2;

            public TriangleType Type;

            public Triangle(TriangleType type, T v0, T v1, T v2)
            {
                V0 = v0;
                V1 = v1;
                V2 = v2;
                Type = type;
            }
        }

        public enum TriangleType : byte
        {
            Unknown,
            Terrain,
            Water,
            Doodad,
            Wmo
        }

    }
}
