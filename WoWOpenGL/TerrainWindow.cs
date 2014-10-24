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

        private uint[] VBOid;

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
            throw new NotImplementedException();
            ADTReader reader = new ADTReader();
            reader.LoadADT("World/Maps/" + map + "/" + map + "_" + xx + "_" + yy + ".adt");

            float TileSize = 1600.0f / 3.0f;
            float ChunkSize = TileSize / 16.0f;
            float UnitSize = ChunkSize / 8.0f;

            VBOid = new uint[2];
            GL.GenBuffers(2, VBOid);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[1]);

            GL.EnableClientState(ArrayCap.VertexArray);
            //GL.EnableClientState(ArrayCap.NormalArray);

            for (int c = 0; c < reader.adtfile.chunks.Count(); c++)
            {
                Console.WriteLine("Reading ADT chunk " + c);
                Console.WriteLine("ADT is at position " + reader.adtfile.chunks[c].header.position.ToString());
                Console.WriteLine("ADT has " + reader.adtfile.chunks[c].vertices.vertices.Count() + " vertices!");

                Vertex[] vertices = new Vertex[145];

                int vindex = 0;
                for (int i = 0; i < 17; i++)
                {
                    for (int j = 0; j < (((i % 2) != 0) ? 8 : 9); j++)
                    {
                        var v = new Vertex();
                        v.Position = new Vector3(0, 0, 0);

                        if ((i % 2) != 0) v.Position.X += 0.5f * UnitSize;
                        vertices[vindex++] = v;
                    }
                }

                List<uint> indicelist = new List<uint>();
                /*
                for (int i = 0; i < reader.model.skins[0].triangles.Count(); i++)
                {
                    indicelist.Add(reader.model.skins[0].triangles[i].pt1);
                    indicelist.Add(reader.model.skins[0].triangles[i].pt2);
                }
                */
                uint[] indices = indicelist.ToArray();

            }
            /*
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * 3 * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[1]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(uint)), indices, BufferUsageHint.StaticDraw);
            */

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

            if (keyboardState.IsKeyDown(Key.Up))
            {
                dragY = dragY + 0.01f;
            }

            if (keyboardState.IsKeyDown(Key.Down))
            {
                dragY = dragY - 0.01f;
            }

            if (keyboardState.IsKeyDown(Key.Left))
            {
                angle = angle + 1.0f;
            }

            if (keyboardState.IsKeyDown(Key.Right))
            {
                angle = angle - 1.0f;
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

            this.SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            Dispose();
            base.OnUnload(e);
            System.Windows.Application.Current.Shutdown();
        }

        private struct Vertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 TexCoord;
        }
    }
}
