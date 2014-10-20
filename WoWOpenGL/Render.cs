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
using WoWFormatLib.FileReaders;
using OpenTK.Input;
using System.Timers;
namespace WoWOpenGL
{
    public class Render : GameWindow
    {
        Camera ActiveCamera;
        float mouseScale = 0.001f;
        bool mouseDragging = false;
        
        private static float angle = 90.0f;
        private GLControl glControl;
        private bool gLoaded = false;
        private Material[] materials;
        private bool modelLoaded;
        private RenderBatch[] renderbatches;
        private uint[] VBOid;
        private static float dragX;
        private static float dragY;
        private static float dragZ;
        private static bool isWMO = false;
        private static bool mouseInRender = false;
        public Render()
        {
            //RenderModel(@"World\ArtTest\Boxtest\xyz.m2");
        }

        public Render(string ModelPath)
        {
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
            
            
            glControl = new GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8), 3, 0, OpenTK.Graphics.GraphicsContextFlags.Default);
            glControl.Width = (int)wfc.ActualWidth;
            glControl.Height = (int)wfc.ActualHeight;
            glControl.Left = 0;
            glControl.Top = 0;

            glControl.MouseEnter += glControl_MouseEnter;
            glControl.MouseLeave += glControl_MouseLeave;
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

        private void glControl_MouseEnter(object sender, EventArgs e)
        {
            mouseInRender = true;
        }

        private void glControl_MouseLeave(object sender, EventArgs e)
        {
            mouseInRender = false;
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
            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(OpenTK.Graphics.Color4.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            InitializeInputTick();
            ActiveCamera.setupGLRenderMatrix();
            DebugLog("GLcontrol is done loading!");

        }

        private void glControl_Resize(object sender, EventArgs e)
        {
        }

        private void LoadM2(string modelpath)
        {
            DebugLog("Loading M2 file ("+modelpath+")..");
            M2Reader reader = new M2Reader();

            string filename = modelpath;
            reader.LoadM2(filename);
            VBOid = new uint[2];
            GL.GenBuffers(2, VBOid);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[1]);

            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);

            List<uint> indicelist = new List<uint>();
            for (int i = 0; i < reader.model.skins[0].triangles.Count(); i++)
            {
                indicelist.Add(reader.model.skins[0].triangles[i].pt1);
                indicelist.Add(reader.model.skins[0].triangles[i].pt2);
                indicelist.Add(reader.model.skins[0].triangles[i].pt3);
            }

            uint[] indices = indicelist.ToArray();

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[1]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(uint)), indices, BufferUsageHint.StaticDraw);
            
            Vertex[] vertices = new Vertex[reader.model.vertices.Count()];

            for (int i = 0; i < reader.model.vertices.Count(); i++)
            {
                vertices[i].Position = new Vector3(reader.model.vertices[i].position.X, reader.model.vertices[i].position.Z, reader.model.vertices[i].position.Y);
                vertices[i].Normal = new Vector3(reader.model.vertices[i].normal.X, reader.model.vertices[i].normal.Z, reader.model.vertices[i].normal.Y);
                vertices[i].TexCoord = new Vector2(reader.model.vertices[i].textureCoordX, reader.model.vertices[i].textureCoordY);
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * 8 * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            GL.Enable(EnableCap.Texture2D);

            materials = new Material[reader.model.textures.Count()];
            DebugLog("Loading textures..");
            for (int i = 0; i < reader.model.textures.Count(); i++)
            {
                DebugLog("Loading texture " + i);
                string texturefilename = "Dungeons\\Textures\\testing\\COLOR_13.blp";
                materials[i].flags = reader.model.textures[i].flags;
                DebugLog("      Requires type " + reader.model.textures[i].type + " texture");
                switch (reader.model.textures[i].type)
                {
                    case 0:
                        DebugLog("      Texture given in file!");
                        texturefilename = reader.model.textures[i].filename;
                        break;
                    case 1:
                        string[] csfilenames = WoWFormatLib.DBC.DBCHelper.getTexturesByModelFilename(filename, (int)reader.model.textures[i].type);
                        if(csfilenames.Count() > 0){
                            texturefilename = csfilenames[0];
                        }
                        else
                        {
                            DebugLog("      No type 1 texture found, falling back to placeholder texture");
                        }
                        break;
                    case 2:
                        if (WoWFormatLib.Utils.CASC.FileExists(Path.ChangeExtension(modelpath, ".blp")))
                        {
                            DebugLog("      BLP exists!");
                            texturefilename = Path.ChangeExtension(modelpath, ".blp");
                        }
                        else
                        {
                            DebugLog("      Type 2 does not exist!");
                            //needs lookup?
                        }
                        break;
                    case 11:
                        string[] cdifilenames = WoWFormatLib.DBC.DBCHelper.getTexturesByModelFilename(filename, (int)reader.model.textures[i].type);
                        for (int ti = 0; ti < cdifilenames.Count(); ti++)
                        {
                            texturefilename = modelpath.Replace(reader.model.name + ".M2", cdifilenames[ti] + ".blp");
                        }
                        break;
                    default:
                        DebugLog("      Falling back to placeholder texture");
                        break;
                }

                Console.WriteLine("      Eventual filename is " + texturefilename);
                materials[i].textureID = GL.GenTexture();
                
                var blp = new BLPReader();

                materials[i].filename = texturefilename;

                blp.LoadBLP(texturefilename);

                if (blp.bmp == null)
                {
                    throw new Exception("BMP is null!");
                }
                else
                {
                    GL.BindTexture(TextureTarget.Texture2D, materials[i].textureID);
                    BitmapData bmp_data = blp.bmp.LockBits(new Rectangle(0, 0, blp.bmp.Width, blp.bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                    //Console.WriteLine(blp.bmp.PixelFormat);
                    DebugLog("Created texture \"" + texturefilename + "\" of " + bmp_data.Width + "x" + bmp_data.Height);
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
                        //Console.WriteLine("SubmeshIndex: " + i);
                        renderbatches[i].blendType = reader.model.renderflags[reader.model.skins[0].textureunit[tu].renderFlags].blendingMode;
                        //Console.WriteLine("Material ID: " + renderbatches[i].materialID);
                        //Console.WriteLine("BlendType: " +renderbatches[i].blendType);
                       renderbatches[i].materialID = reader.model.texlookup[reader.model.skins[0].textureunit[tu].texture].textureID;
                    }
                }
            }
            DebugLog("  " + reader.model.skins.Count() + " skins");
            DebugLog("  " + renderbatches.Count() + " renderbatches");
            DebugLog("  " + reader.model.vertices.Count() + " vertices");
            DebugLog("Done loading M2 file!");
            
            gLoaded = true;
        }

        private void LoadWMO(string modelpath)
        {
            DebugLog("Loading WMO file..");
            WMOReader reader = new WMOReader();
            string filename = modelpath;
            //Load WMO
            reader.LoadWMO(filename);

            //Enable Vertex Arrays
            GL.EnableClientState(ArrayCap.VertexArray);
            //Enable Normal Arrays
            GL.EnableClientState(ArrayCap.NormalArray);
            //Enable TexCoord arrays
            GL.EnableClientState(ArrayCap.TextureCoordArray);
           
            //Set up buffer IDs
            VBOid = new uint[(reader.wmofile.group.Count() * 2) + 2];
            GL.GenBuffers((reader.wmofile.group.Count() * 2) + 2, VBOid);

            for (int g = 0; g < reader.wmofile.group.Count(); g++)
            {
                //Switch to Vertex buffer
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[g * 2]);

                Vertex[] vertices = new Vertex[reader.wmofile.group[g].mogp.vertices.Count()];

                for (int i = 0; i < reader.wmofile.group[g].mogp.vertices.Count(); i++)
                {
                    vertices[i].Position = new Vector3(reader.wmofile.group[g].mogp.vertices[i].vector.X, reader.wmofile.group[g].mogp.vertices[i].vector.Z, reader.wmofile.group[g].mogp.vertices[i].vector.Y);
                    vertices[i].Normal = new Vector3(reader.wmofile.group[g].mogp.normals[i].normal.X, reader.wmofile.group[g].mogp.normals[i].normal.Z, reader.wmofile.group[g].mogp.normals[i].normal.Y);
                    vertices[i].TexCoord = new Vector2(reader.wmofile.group[g].mogp.textureCoords[i].X, reader.wmofile.group[g].mogp.textureCoords[i].Y);
                }

                //Push to buffer
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * 8 * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

                //Switch to Index buffer
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[(g * 2) + 1]);

                List<uint> indicelist = new List<uint>();
                for (int i = 0; i < reader.wmofile.group[g].mogp.indices.Count(); i++)
                {
                    indicelist.Add(reader.wmofile.group[g].mogp.indices[i].indice);
                }

                uint[] indices = indicelist.ToArray();

                //Push to buffer
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(uint)), indices, BufferUsageHint.StaticDraw);
            }

            GL.Enable(EnableCap.Texture2D);

            materials = new Material[reader.wmofile.materials.Count()];
            for (int i = 0; i < reader.wmofile.materials.Count(); i++)
            {
                for (int ti = 0; ti < reader.wmofile.textures.Count(); ti++)
                {
                    
                    if (reader.wmofile.textures[ti].startOffset == reader.wmofile.materials[i].texture1)
                    {
                        materials[i].textureID = GL.GenTexture();
                        var blp = new BLPReader();
                        blp.LoadBLP(reader.wmofile.textures[ti].filename);
                        if (blp.bmp == null)
                        {
                            throw new Exception("BMP is null!");
                        }
                        else
                        {
                            GL.BindTexture(TextureTarget.Texture2D, materials[i].textureID);
                            BitmapData bmp_data = blp.bmp.LockBits(new Rectangle(0, 0, blp.bmp.Width, blp.bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

                            DebugLog("Created texture \"" + reader.wmofile.textures[ti].filename + "\" (ID "+materials[i].textureID+") of " + bmp_data.Width + "x" + bmp_data.Height);
                            blp.bmp.UnlockBits(bmp_data);
                        }
                        materials[i].filename = reader.wmofile.textures[ti].filename;
                    }
                }
            }

            int numRenderbatches = 0;
            //Get total amount of render batches
            for (int i = 0; i < reader.wmofile.group.Count(); i++)
            {
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
                    renderbatches[rb].blendType = reader.wmofile.materials[group.mogp.renderBatches[i].materialID].blendMode;
                    renderbatches[rb].groupID = (uint)g;
                    rb++;
                }
            }

            DebugLog("  " + reader.wmofile.group.Count() + " skins");
            DebugLog("  " + materials.Count() + " materials");
            DebugLog("  " + renderbatches.Count() + " renderbatches");
            DebugLog("  " + reader.wmofile.group[0].mogp.vertices.Count() + " vertices");
            DebugLog("Done loading WMO file!");
            
            gLoaded = true;
            isWMO = true;
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
            
            OpenTK.Input.MouseState mouseState = OpenTK.Input.Mouse.GetState();
            OpenTK.Input.KeyboardState keyboardState = OpenTK.Input.Keyboard.GetState();

            /*if (keyboardState.IsKeyDown(Key.Up))
            {
                dragX = dragX + 0.01f;
            }

            if (keyboardState.IsKeyDown(Key.Down))
            {
                dragX = dragX - 0.01f;
            }*/

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


            //if (mouseInRender)
            //{
                dragZ = (mouseState.WheelPrecise/ 10) - 7.5f; //Startzoom is at -7.5f 
            //}
            
        }

        private void RenderFrame(object sender, EventArgs e) //This is called every frame
        {
            if (!gLoaded) { return; }
            glControl.MakeCurrent();

            ActiveCamera.Pos = new Vector3(dragX, dragY, dragZ);

            ActiveCamera.setupGLRenderMatrix();

            if (!gLoaded) return;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            GL.Enable(EnableCap.Texture2D);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);

            GL.Rotate(angle, 0.0, 1.0, 0.0);

            for (int i = 0; i < renderbatches.Count(); i++)
            {
                if (!isWMO)
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);
                    GL.VertexPointer(3, VertexPointerType.Float, 32, 0);
                    GL.NormalPointer(NormalPointerType.Float, 32, 12);
                    GL.TexCoordPointer(2, TexCoordPointerType.Float, 32, 24);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[1]);
                }
                else
                {
                    //DebugLog("Switching to buffer " + renderbatches[i].groupID * 2);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[renderbatches[i].groupID * 2]);
                    GL.VertexPointer(3, VertexPointerType.Float, 32, 0);
                    GL.NormalPointer(NormalPointerType.Float, 32, 12);
                    GL.TexCoordPointer(2, TexCoordPointerType.Float, 32, 24);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[(renderbatches[i].groupID * 2) + 1]);
                }
                
                if (renderbatches[i].materialID > materials.Count() - 1) //temp hackfix
                {
                    DebugLog("[ERROR] Material ID encountered which is lower than material count!!!");
                    //continue;
                }
                else
                {
                    switch(renderbatches[i].blendType)
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
                        default:
                            throw new Exception("Unknown blend type " + renderbatches[i].blendType);
                    }
                    GL.BindTexture(TextureTarget.Texture2D, materials[renderbatches[i].materialID].textureID);
                }
                
                GL.DrawRangeElements(PrimitiveType.Triangles, renderbatches[i].firstFace, (renderbatches[i].firstFace + renderbatches[i].numFaces), (int)renderbatches[i].numFaces, DrawElementsType.UnsignedInt, new IntPtr(renderbatches[i].firstFace * 4));
                if (GL.GetError().ToString() != "NoError")
                {
                    DebugLog(GL.GetError().ToString());
                }
            }

            DrawAxes();
            glControl.SwapBuffers();
            glControl.Invalidate();
        }

        public struct RenderBatch
        {
            public uint firstFace;
            public uint materialID;
            public uint numFaces;
            public uint groupID;
            public uint blendType;
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