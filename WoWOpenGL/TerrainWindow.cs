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
        private static float camSpeed = 0.025f;
        private uint[] VBOid;

        private uint[] indices;

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

            float TileSize = 1600.0f / 3.0f;
            float ChunkSize = TileSize / 16.0f;
            float UnitSize = ChunkSize / 8.0f;

            GL.EnableClientState(ArrayCap.VertexArray);

            VBOid = new uint[2];
            GL.GenBuffers(2, VBOid);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[1]);

            List<Vector3> verticelist = new List<Vector3>();
            List<uint> indicelist = new List<uint>();

            for (int c = 0; c < reader.adtfile.chunks.Count(); c++)
            {
                //Console.WriteLine("Reading ADT chunk " + c);
                //Console.WriteLine("ADT is at position " + reader.adtfile.chunks[c].header.position.ToString());
                //Console.WriteLine("ADT has " + reader.adtfile.chunks[c].vertices.vertices.Count() + " vertices!");

                for (int i = 0, idx = 0; i < 17; i++)
                {
                    for (int j = 0; j < (((i % 2) != 0) ? 8 : 9); j++)
                    {
                        var v = new Vector3(((c / 16) * ChunkSize) - (j * UnitSize), ((c % 16) * ChunkSize) - -(i * UnitSize * 0.5f), reader.adtfile.chunks[c].vertices.vertices[idx] + reader.adtfile.chunks[c].header.position.Z);
                        //var v = new Vector3(j * UnitSize, i * UnitSize, reader.adtfile.chunks[c].vertices.vertices[idx]); 
                        if ((i % 2) != 0) v.X += 0.5f * UnitSize;
                        verticelist.Add(v);
                        //Console.WriteLine(reader.adtfile.chunks[c].vertices.vertices[idx]);
                        idx++;
                        
                    }
                }

                for (uint j = 9; j < 8 * 8 + 9 * 8; j++)
                {

                    //Triangle 1
                    indicelist.Add(j);
                    indicelist.Add(j - 9);
                    indicelist.Add(j + 8);
                    //Triangle 2
                    indicelist.Add(j);
                    indicelist.Add(j - 8);
                    indicelist.Add(j - 9);
                    //Triangle 3
                    indicelist.Add(j);
                    indicelist.Add(j + 9);
                    indicelist.Add(j - 8);
                    //Triangle 4
                    indicelist.Add(j);
                    indicelist.Add(j + 8);
                    indicelist.Add(j + 9);

                    if ((j + 1) % (9 + 8) == 0) j += 9;
                }
            }


            Vector3[] vertices = verticelist.ToArray();
            Console.WriteLine("Vertices in array: " + vertices.Count()); //37120, correct

            indices = indicelist.ToArray();
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
