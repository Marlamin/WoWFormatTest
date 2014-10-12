using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWFormatLib.FileReaders;
using OpenTK.Input;
namespace WoWOpenGL
{
    public class Render : GameWindow
    {
        Camera ActiveCamera;
        float mouseScale = 0.001f;
        bool mouseDragging = false;

        
        private static float angle = 90.0f;
        private string basedir;
        private GLControl glControl;
        private bool gLoaded = false;
        private Material[] materials;
        private bool modelLoaded;
        private RenderBatch[] renderbatches;
        private uint[] VBOid;
        private float dragX;
        private float dragY;
        private float dragZ;

        public Render()
        {
            //RenderModel(@"World\ArtTest\Boxtest\xyz.m2");
        }

        public Render(string ModelPath)
        {
            basedir = ConfigurationManager.AppSettings["basedir"];

            dragX = 0.0f;
            dragY = 0.0f;
            dragZ = -7.5f;

            System.Windows.Forms.Integration.WindowsFormsHost wfc = MainWindow.winFormControl;

            ActiveCamera = new Camera((int)wfc.ActualWidth, (int)wfc.ActualHeight);
            ActiveCamera.Pos = new Vector3(10.0f, -10.0f, -7.5f);
            Console.WriteLine(ModelPath);

            if (ModelPath.EndsWith(".m2", StringComparison.OrdinalIgnoreCase))
            {
                modelLoaded = true;
                LoadM2(ModelPath);
            }
            else if (ModelPath.EndsWith(".wmo", StringComparison.OrdinalIgnoreCase))
            {
                modelLoaded = true;
                LoadWMO(ModelPath);
            }
            else
            {
                modelLoaded = false;
            }
            
            
            glControl = new GLControl(OpenTK.Graphics.GraphicsMode.Default, 3, 0, OpenTK.Graphics.GraphicsContextFlags.Default);

            glControl.Width = (int)wfc.ActualWidth;
            glControl.Height = (int)wfc.ActualHeight;
            glControl.Left = 0;
            glControl.Top = 0;

            glControl.Load += glControl_Load;
            glControl.Paint += RenderFrame;
            glControl.Resize += glControl_Resize;
           /*glControl.MouseMove += new MouseEventHandler(glControl_MouseMove);
            glControl.MouseDown += new MouseEventHandler(glControl_MouseDown);
            glControl.MouseUp += new MouseEventHandler(glControl_MouseUp);
            */
            glControl_Resize(glControl, EventArgs.Empty);
            glControl.MakeCurrent();

            wfc.Child = glControl;


            Console.WriteLine(glControl.Width + "x" + glControl.Height);
        }

        public void DrawAxes()
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

        private void glControl_Load(object sender, EventArgs e)
        {
            DebugLog("Loading GLcontrol..");
            glControl.MakeCurrent();
            GL.Enable(EnableCap.Texture2D);
            GL.ClearColor(OpenTK.Graphics.Color4.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            ActiveCamera.setupGLRenderMatrix();
            DebugLog("GLcontrol is done loading!");

        }

        private void glControl_Resize(object sender, EventArgs e)
        {
        }

        private void LoadM2(string modelpath)
        {
            DebugLog("Loading M2 file ("+modelpath+")..");
            M2Reader reader = new M2Reader(basedir);
            string filename = modelpath;
            reader.LoadM2(filename);
            VBOid = new uint[2];
            GL.GenBuffers(2, VBOid);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[1]);

            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);

           

            List<ushort> indicelist = new List<ushort>();
            for (int i = 0; i < reader.model.skins[0].triangles.Count(); i++)
            {
                indicelist.Add(reader.model.skins[0].triangles[i].pt1);
                indicelist.Add(reader.model.skins[0].triangles[i].pt2);
                indicelist.Add(reader.model.skins[0].triangles[i].pt3);
            }

            ushort[] indices = indicelist.ToArray();

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[1]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(ushort)), indices, BufferUsageHint.StaticDraw);
            
            Vertex[] vertices = new Vertex[reader.model.vertices.Count()];

            for (int i = 0; i < reader.model.vertices.Count(); i++)
            {
                vertices[i].Position = new Vector3(reader.model.vertices[i].position.X, reader.model.vertices[i].position.Z, reader.model.vertices[i].position.Y);
                vertices[i].Normal = new Vector3(reader.model.vertices[i].normal.X, reader.model.vertices[i].normal.Z, reader.model.vertices[i].normal.Y);
                vertices[i].TexCoord = new Vector2(reader.model.vertices[i].textureCoordX, reader.model.vertices[i].textureCoordY);
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * 8 * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            /*GL.Enable(EnableCap.Texture2D);
            
            materials = new Material[reader.model.textures.Count()];
            for (int i = 0; i < reader.model.textures.Count(); i++)
            {
                materials[i].flags = reader.model.textures[i].flags;
                materials[i].textureID = GL.GenTexture();
                
                var blp = new BLPReader(basedir);
               // if (File.Exists(System.IO.Path.Combine(basedir, reader.model.filename.Replace("M2", "BLP"))))
               // {
               //     materials[i].filename = reader.model.filename.Replace("M2", "BLP");
               //     blp.LoadBLP(reader.model.filename.Replace("M2", "BLP"));
               // }
               // else
              //  {
                    materials[i].filename = reader.model.textures[i].filename;
                    blp.LoadBLP(reader.model.textures[i].filename);
              //  }

                if (blp.bmp == null)
                {
                    throw new Exception("BMP is null!");
                }
                else
                {
                    GL.BindTexture(TextureTarget.Texture2D, materials[i].textureID - 1);
                    BitmapData bmp_data = blp.bmp.LockBits(new Rectangle(0, 0, blp.bmp.Width, blp.bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat); 
                    Console.WriteLine("Created texture \"" + reader.model.textures[i].filename + "\" of " + bmp_data.Width + "x" + bmp_data.Height);
                    blp.bmp.UnlockBits(bmp_data);
                }
            }
            */
            renderbatches = new RenderBatch[reader.model.skins[0].submeshes.Count()];
            for (int i = 0; i < reader.model.skins[0].submeshes.Count(); i++)
            {
                renderbatches[i].firstFace = reader.model.skins[0].submeshes[i].startTriangle;
                renderbatches[i].numFaces = reader.model.skins[0].submeshes[i].nTriangles;
                for (int tu = 0; tu < reader.model.skins[0].textureunit.Count(); tu++)
                {
                    if (reader.model.skins[0].textureunit[tu].submeshIndex == i)
                    {
                        renderbatches[i].materialID = reader.model.skins[0].textureunit[tu].texture;
                    }
                }
            }
            DebugLog("  " + reader.model.skins.Count() + " skins");
            DebugLog("  " + renderbatches.Count() + " renderbatches");
            DebugLog("  " + reader.model.vertices.Count() + " vertices");
            DebugLog("Done loading M2 file!");
            System.Threading.Thread.Sleep(100);
            gLoaded = true;
        }

        private void LoadWMO(string modelpath)
        {
            WMOReader reader = new WMOReader(basedir);
            string filename = modelpath;

            //Load WMO
            reader.LoadWMO(filename);
            
            //Set up buffer IDs
            VBOid = new uint[2];
            GL.GenBuffers(2, VBOid);

            //Bind Vertex buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);
            
            //Bind Index buffer
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[1]);

            //Enable Vertex Arrays
            GL.EnableClientState(ArrayCap.VertexArray);
            //Enable Normal Arrays
            GL.EnableClientState(ArrayCap.NormalArray);
            //Enable TexCoord arrays
            GL.EnableClientState(ArrayCap.TextureCoordArray);

            //Switch to Index buffer
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[1]);

            List<ushort> indicelist = new List<ushort>();
            for (int i = 0; i < reader.wmofile.group[0].mogp.indices.Count(); i++)
            {
                indicelist.Add(reader.wmofile.group[0].mogp.indices[i].indice);
            }

            ushort[] indices = indicelist.ToArray();

            //Push to buffer
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(ushort)), indices, BufferUsageHint.StaticDraw);

            //Switch to Vertex buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);

            Vertex[] vertices = new Vertex[reader.wmofile.group[0].mogp.vertices.Count()];

            for (int i = 0; i < reader.wmofile.group[0].mogp.vertices.Count(); i++)
            {
                vertices[i].Position = new Vector3(reader.wmofile.group[0].mogp.vertices[i].vector.X, reader.wmofile.group[0].mogp.vertices[i].vector.Z, reader.wmofile.group[0].mogp.vertices[i].vector.Y);
                vertices[i].Normal = new Vector3(reader.wmofile.group[0].mogp.normals[i].normal.X, reader.wmofile.group[0].mogp.normals[i].normal.Z, reader.wmofile.group[0].mogp.normals[i].normal.Y);
                vertices[i].TexCoord = new Vector2(reader.wmofile.group[0].mogp.textureCoords[i].X, reader.wmofile.group[0].mogp.textureCoords[i].Y);
            }

            //Push to buffer
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * 8 * sizeof(float)), vertices, BufferUsageHint.StaticDraw);
            
            int numRenderbatches = 0;
            //Get total amount of render batches
            for (int i = 0; i < reader.wmofile.group.Count(); i++){
                numRenderbatches = numRenderbatches + reader.wmofile.group[i].mogp.renderBatches.Count();
            }

            renderbatches = new RenderBatch[numRenderbatches];

            int rb = 0;
            for (int g = 0; g < reader.wmofile.group.Count(); g++)
            {
                var group = reader.wmofile.group[g];
                for (int i = 0; i < group.mogp.renderBatches.Count(); i++)
                {
                    renderbatches[rb].firstFace = group.mogp.renderBatches[i].firstFace;
                    renderbatches[rb].numFaces = group.mogp.renderBatches[i].numFaces;
                    renderbatches[rb].materialID = group.mogp.renderBatches[i].materialID;
                }
            }
            
        }

        private void RenderFrame(object sender, EventArgs e) //This is called every frame
        {
            if (!gLoaded) { return; }
            glControl.MakeCurrent();
            
            OpenTK.Input.MouseState mouseState = OpenTK.Input.Mouse.GetState();
            OpenTK.Input.KeyboardState keyboardState = OpenTK.Input.Keyboard.GetState();
            

            if (keyboardState.IsKeyDown(Key.Left))
            {
                angle = angle + 1.0f;

            }

            if (keyboardState.IsKeyDown(Key.Right))
            {
                angle = angle - 1.0f;
            }

            if (keyboardState.IsKeyDown(Key.Up))
            {
                dragY = dragY + 0.1f;
            }

            if (keyboardState.IsKeyDown(Key.Down))
            {
                dragY = dragY - 0.1f;
            }

            
            dragZ = (mouseState.WheelPrecise / 10) - 7.5f; //Startzoom is at -7.5f 

            ActiveCamera.Pos = new Vector3(dragX, dragY, dragZ);

           // ActiveCamera.OrbitXY(dragX, dragY);
            ActiveCamera.setupGLRenderMatrix();

            if (!gLoaded) return;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.Texture2D);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);
            GL.VertexPointer(3, VertexPointerType.Float, 32, 0);
            GL.NormalPointer(NormalPointerType.Float, 32, 12);
            GL.TexCoordPointer(2, TexCoordPointerType.Float, 32, 24);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[1]);
            GL.Rotate(angle, 0.0, 1.0, 0.0);
            for (int i = 0; i < renderbatches.Count(); i++)
            {
                //if (renderbatches[i].materialID > materials.Count() - 1) //temp hackfix
                //{
                //    continue;
                //}
                //GL.BindTexture(TextureTarget.Texture2D, materials[renderbatches[i].materialID].textureID);
                GL.DrawRangeElements(PrimitiveType.Triangles, renderbatches[i].firstFace, (renderbatches[i].firstFace + renderbatches[i].numFaces), (int)renderbatches[i].numFaces, DrawElementsType.UnsignedShort, new IntPtr(renderbatches[i].firstFace * 3));
                if (GL.GetError().ToString() != "NoError")
                {
                    DebugLog(GL.GetError().ToString());
                }
            }

            //GL.DisableClientState(ArrayCap.VertexArray);
            /*
            GL.Begin(PrimitiveType.Triangles);
            for (uint i = 0; i < indices.Length; i++)
            {
                GL.TexCoord2(vertices[indices[i]].TexCoord);
                GL.Normal3(vertices[indices[i]].Normal);
                GL.Vertex3(vertices[indices[i]].Position);
            }
            GL.End();
            */
            DrawAxes();
            glControl.SwapBuffers();
            glControl.Invalidate();
        }

        public struct RenderBatch
        {
            public uint firstFace;
            public uint materialID;
            public uint numFaces;
        }

        private struct Vertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 TexCoord;
        }

        public struct Material
        {
            public string filename;
            public WoWFormatLib.Structs.M2.TextureFlags flags;
            public int textureID;
        }

        public void DebugLog(string log)
        {
            MainWindow.curlogentry = MainWindow.curlogentry + 1;
            MainWindow.debugList.Items.Add("[" + MainWindow.curlogentry + "] " + log);
            MainWindow.debugList.ScrollIntoView(MainWindow.debugList.Items[MainWindow.debugList.Items.Count - 1]);
        }
    }
}