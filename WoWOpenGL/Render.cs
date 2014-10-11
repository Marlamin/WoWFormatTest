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
        private static float angle = 0.0f;
        private string basedir;
        private GLControl glControl;
        private bool gLoaded = false;
        private Material[] materials;
        private bool modelLoaded;
        private RenderBatch[] renderbatches;
        private uint[] VBOid;


        public Render()
        {
            //RenderModel(@"World\ArtTest\Boxtest\xyz.m2");
        }

        public Render(string ModelPath)
        {
            basedir = ConfigurationManager.AppSettings["basedir"];
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

            System.Windows.Forms.Integration.WindowsFormsHost wfc = MainWindow.winFormControl;
            glControl = new GLControl(OpenTK.Graphics.GraphicsMode.Default, 3, 0, OpenTK.Graphics.GraphicsContextFlags.Default);

            glControl.Width = 800;
            glControl.Height = 600;
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

        private void glControl_Load(object sender, EventArgs e)
        {
            Console.WriteLine("Loaded!");
            glControl.MakeCurrent();
            GL.Enable(EnableCap.Texture2D);
            GL.ClearColor(OpenTK.Graphics.Color4.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            gLoaded = true;

            GL.Viewport(0, 0, glControl.Width, glControl.Height);

            float aspect_ratio = glControl.Width / glControl.Height;

            Console.WriteLine("Creating perspective for " + aspect_ratio.ToString());
            Matrix4 perpective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect_ratio, 1, 128);
            GL.Ortho(0, glControl.Width, 0, glControl.Height, -1, 1);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref perpective);


        }

        private void glControl_Resize(object sender, EventArgs e)
        {
        }

        private void LoadM2(string modelpath)
        {
            M2Reader reader = new M2Reader(basedir);
            string filename = modelpath;
            reader.LoadM2(filename);
            VBOid = new uint[2];
            GL.GenBuffers(2, VBOid);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[1]);

            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[1]);

            List<ushort> indicelist = new List<ushort>();
            for (int i = 0; i < reader.model.skins[0].triangles.Count(); i++)
            {
                indicelist.Add(reader.model.skins[0].triangles[i].pt1);
                indicelist.Add(reader.model.skins[0].triangles[i].pt2);
                indicelist.Add(reader.model.skins[0].triangles[i].pt3);
            }

            ushort[] indices = indicelist.ToArray();

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

            int buffersize;
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[1]);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out buffersize);
            GL.Enable(EnableCap.Texture2D);

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
                vertices[i].Normal = new Vector3(reader.wmofile.group[0].mogp.normals[i].normal.X, reader.wmofile.group[0].mogp.normals[i].normal.X, reader.wmofile.group[0].mogp.normals[i].normal.Y);
                vertices[i].TexCoord = new Vector2(reader.wmofile.group[0].mogp.textureCoords[i].X, reader.wmofile.group[0].mogp.textureCoords[i].X);
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
            glControl.MakeCurrent();

            var mouse = OpenTK.Input.Mouse.GetState();
            
            if (!gLoaded) return;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 lookat = Matrix4.LookAt(0, 5, 5, 0, 0, 0, 0, 5, 0);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookat);

            GL.Rotate(angle, 0.0f, 1.0f, mouse.Wheel / 0.1f);
            angle += 0.25f;
            GL.Enable(EnableCap.Texture2D);
            //GL.EnableClientState(ArrayCap.VertexArray);
            // GL.VertexPointer(8, VertexPointerType.Float, 0, vertices);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.VertexPointer(3, VertexPointerType.Float, 32, 0);
            GL.EnableClientState(ArrayCap.NormalArray);
            GL.NormalPointer(NormalPointerType.Float, 32, 12);
            GL.EnableClientState(ArrayCap.TextureCoordArray);
            GL.TexCoordPointer(2, TexCoordPointerType.Float, 32, 24);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[1]);

            for (int i = 0; i < renderbatches.Count(); i++)
            {
                //if (renderbatches[i].materialID > materials.Count() - 1) //temp hackfix
                //{
                //    continue;
                //}
                //GL.BindTexture(TextureTarget.Texture2D, materials[renderbatches[i].materialID].textureID);
                //Console.WriteLine("Rendering batch " + i + " Face: " + renderbatches[i].firstFace);
                GL.DrawRangeElements(PrimitiveType.Triangles, renderbatches[i].firstFace, renderbatches[i].firstFace + renderbatches[i].numFaces, (int)renderbatches[i].numFaces + 20, DrawElementsType.UnsignedShort, IntPtr.Zero);
                //Console.WriteLine(GL.GetError().ToString());
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
    }
}