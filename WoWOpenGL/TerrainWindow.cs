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
        private static float lightHeight = 0.0f;
        private static float camSpeed = 0.25f;
        private uint[] VBOid = new uint[2];
        private uint[] modelVBOid = new uint[2];
        private int[] indices;
        private List<Terrain> adts = new List<Terrain>();
        private Dictionary<uint, WoWFormatLib.Structs.M2.M2Model> models = new Dictionary<uint, WoWFormatLib.Structs.M2.M2Model>();
        private Dictionary<string, int> materialCache = new Dictionary<string, int>();
        private Dictionary<uint, WoWFormatLib.Structs.WMO.WMO> worldModels = new Dictionary<uint, WoWFormatLib.Structs.WMO.WMO>();
        OldCamera ActiveCamera;

        public TerrainWindow(string modelPath)
            : base(1920, 1080, new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8), "Terrain test", GameWindowFlags.Default, DisplayDevice.Default, 3, 0, OpenTK.Graphics.GraphicsContextFlags.Default)
        {
            dragX = 227;
            dragY = 152;
            dragZ = 2868;
            angle = 90.0f;

            Keyboard.KeyDown += Keyboard_KeyDown;

            ActiveCamera = new OldCamera(Width, Height);
            ActiveCamera.Pos = new Vector3(10.0f, -10.0f, -7.5f);

            Console.WriteLine(modelPath);

            string[] adt = modelPath.Split('_');

            Console.WriteLine("MAP {0}, X {1}, Y {2}", adt[0], adt[1], adt[2]);
            //LoadADT(adt[0], adt[1], adt[2]);
            LoadMap(adt[0], int.Parse(adt[1]), int.Parse(adt[2]), 1);

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
            GL.EnableClientState(ArrayCap.NormalArray);

            GL.GenBuffers(2, VBOid);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[1]);

            List<Vertex> verticelist = new List<Vertex>();
            List<Int32> indicelist = new List<Int32>();

            GL.Enable(EnableCap.Texture2D);

            for (int x = centerx; x < centerx + distance; x++)
            {
                for (int y = centery; y < centery + distance; y++)
                {
                    string filename = "World/Maps/" + map + "/" + map + "_" + y + "_" + x + ".adt";

                    if (WoWFormatLib.Utils.CASC.FileExists(filename))
                    {
                        ADTReader reader = new ADTReader();
                        reader.LoadADT(filename);

                        Terrain adt = new Terrain();

                        List<Material> materials = new List<Material>();
        
                        //Check if textures are already loaded or not, multiple ADTs close together probably use the same ones mostly
                        for (int ti = 0; ti < reader.adtfile.textures.filenames.Count(); ti++)
                        {
                            Material material = new Material();
                            material.filename = reader.adtfile.textures.filenames[ti];

                            if (!WoWFormatLib.Utils.CASC.FileExists(material.filename))
                            {
                                continue;
                            }

                            if (materialCache.ContainsKey(material.filename)){
                                Console.WriteLine("Material " + material.filename + " is already cached!");
                                material.textureID = materialCache[material.filename];
                                materials.Add(material);
                                continue;
                            }

                            material.textureID = GL.GenTexture();

                            var blp = new BLPReader();

                            blp.LoadBLP(reader.adtfile.textures.filenames[ti]);

                            if (blp.bmp == null)
                            {
                                throw new Exception("BMP is null!");
                            }
                            else
                            {
                                GL.BindTexture(TextureTarget.Texture2D, material.textureID);
                                //materialCache.Add(material.filename, material.textureID);
                                System.Drawing.Imaging.BitmapData bmp_data = blp.bmp.LockBits(new Rectangle(0, 0, blp.bmp.Width, blp.bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
                                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                                Console.WriteLine("Created texture " + reader.adtfile.textures.filenames[ti] + " of " + bmp_data.Width + "x" + bmp_data.Height);
                                //blp.bmp.Save("texture_" + material.textureID + ".bmp");
                                blp.bmp.UnlockBits(bmp_data);
                            }

                            materials.Add(material);
                        }
                       
                        var initialChunkY = reader.adtfile.chunks[0].header.position.Y;
                        var initialChunkX = reader.adtfile.chunks[0].header.position.X;

                        List<RenderBatch> renderBatches = new List<RenderBatch>();

                        for (uint c = 0; c < reader.adtfile.chunks.Count(); c++)
                        {
                            var chunk = reader.adtfile.chunks[c];
                            //Console.WriteLine("Reading ADT chunk " + c);
                            //Console.WriteLine("ADT is at position " + reader.adtfile.chunks[c].header.position.ToString());
                            //Console.WriteLine("ADT has " + reader.adtfile.chunks[c].vertices.vertices.Count() + " vertices!");

                            //int off = chunk.vertices.vertices.Count();

                            int off = verticelist.Count();

                            RenderBatch batch = new RenderBatch();

                            for (int i = 0, idx = 0; i < 17; i++)
                            {
                                for (int j = 0; j < (((i % 2) != 0) ? 8 : 9); j++)
                                {
                                    //var v = new Vector3(chunk.header.position.Y - (j * UnitSize), chunk.vertices.vertices[idx++] + chunk.header.position.Z, -(chunk.header.position.X - (i * UnitSize * 0.5f)));
                                    Vertex v = new Vertex();
                                    v.Normal = new Vector3(chunk.normals.normal_0[idx], chunk.normals.normal_1[idx], chunk.normals.normal_2[idx]);
                                    if (chunk.vertexshading.red != null && chunk.vertexshading.red[idx] != 127)
                                    {
                                        v.Color = new Vector3(chunk.vertexshading.blue[idx] / 255.0f, chunk.vertexshading.green[idx] / 255.0f, chunk.vertexshading.red[idx] / 255.0f);
                                        //v.Color = new Vector3(1.0f, 1.0f, 1.0f);
                                    }
                                    else
                                    {
                                        v.Color = new Vector3(1.0f, 1.0f, 1.0f);
                                    }

                                    v.TexCoord = new Vector2(((float)j + (((i % 2) != 0) ? 0.5f : 0f)) / 8f, ((float)i * 0.5f) / 8f);

                                    v.Position = new Vector3(chunk.header.position.Y - (j * UnitSize), chunk.vertices.vertices[idx++] + chunk.header.position.Z, chunk.header.position.X - (i * UnitSize * 0.5f));

                                    if ((i % 2) != 0) v.Position.X -= 0.5f * UnitSize;
                                    verticelist.Add(v);
                                }
                            }

                            //Console.WriteLine("First vertice at " + verticelist.First().X + "x" + verticelist.First().Y);

                            batch.firstFace = indicelist.Count();
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
                            batch.numFaces = (indicelist.Count()) - batch.firstFace;

                            batch.materialID = reader.adtfile.texChunks[c].layers[0].textureId;

                            Console.WriteLine(materials[(int) batch.materialID].filename);

                            renderBatches.Add(batch);
                        }

                        List<DoodadBatch> doodadBatches = new List<DoodadBatch>();

                        for (int mi = 0; mi < reader.adtfile.objects.models.entries.Count(); mi++)
                        {
                            WoWFormatLib.Structs.M2.M2Model model = new WoWFormatLib.Structs.M2.M2Model();
                            var modelentry = reader.adtfile.objects.models.entries[mi];
                            var mmid = reader.adtfile.objects.m2NameOffsets.offsets[modelentry.mmidEntry];
                            for (int mmi = 0; mmi < reader.adtfile.objects.m2Names.offsets.Count(); mmi++)
                            {
                                if (reader.adtfile.objects.m2Names.offsets[mmi] == mmid)
                                {
                                    if (models.ContainsKey(reader.adtfile.objects.m2Names.offsets[mmi]))
                                    {
                                        //Load model from memory
                                        model = models[reader.adtfile.objects.m2Names.offsets[mmi]];
                                        Console.WriteLine("Loaded M2 from memory " + model.filename + " which as " + model.vertices.Count() + " vertices");
                                    }
                                    else
                                    {
                                        //Load model from file
                                        Console.WriteLine(modelentry.mmidEntry + ": " + reader.adtfile.objects.m2Names.filenames[mmi]);
                                        if (WoWFormatLib.Utils.CASC.FileExists(reader.adtfile.objects.m2Names.filenames[mmi]))
                                        {
                                            var modelreader = new M2Reader();
                                            modelreader.LoadM2(reader.adtfile.objects.m2Names.filenames[mmi]);
                                            models.Add(reader.adtfile.objects.m2Names.offsets[mmi], modelreader.model);
                                            model = modelreader.model;
                                            Console.WriteLine("Loaded M2 from disk " + modelreader.model.filename + " which as " + modelreader.model.vertices.Count() + " vertices");
                                        }
                                        else
                                        {
                                            throw new Exception("Model " + reader.adtfile.objects.m2Names.filenames[mmi] + " does not exist!");
                                        }
                                    }
                                }
                            }

                            if(model.filename == null)
                            {
                                throw new Exception("Model isn't loaded!!!!!");
                            }

                            var ddBatch = new DoodadBatch();

                            // Textures
                            ddBatch.mats = new Material[model.textures.Count()];
                            Console.WriteLine("Loading textures..");
                            for (int i = 0; i < model.textures.Count(); i++)
                            {
                                Console.WriteLine("Loading texture " + i);
                                string texturefilename = "Dungeons\\Textures\\testing\\COLOR_13.blp";
                                //materials[i].flags = model.textures[i].flags;
                                Console.WriteLine("      Requires type " + model.textures[i].type + " texture");
                                switch (model.textures[i].type)
                                {
                                    case 0:
                                        Console.WriteLine("      Texture given in file!");
                                        texturefilename = model.textures[i].filename;
                                        break;
                                    default:
                                        Console.WriteLine("      Falling back to placeholder texture");
                                        break;
                                }

                                Console.WriteLine("      Eventual filename is " + texturefilename);
                                ddBatch.mats[i].textureID = GL.GenTexture();

                                var blp = new BLPReader();

                                ddBatch.mats[i].filename = texturefilename;

                                blp.LoadBLP(texturefilename);

                                if (blp.bmp == null)
                                {
                                    throw new Exception("BMP is null!");
                                }
                                else
                                {
                                    GL.BindTexture(TextureTarget.Texture2D, ddBatch.mats[i].textureID);
                                    System.Drawing.Imaging.BitmapData bmp_data = blp.bmp.LockBits(new Rectangle(0, 0, blp.bmp.Width, blp.bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
                                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                                    //Console.WriteLine(blp.bmp.PixelFormat);
                                    Console.WriteLine("Created texture \"" + texturefilename + "\" of " + bmp_data.Width + "x" + bmp_data.Height);
                                    blp.bmp.UnlockBits(bmp_data);
                                }
                            }

                            // Submeshes
                            ddBatch.submeshes = new Submesh[model.skins[0].submeshes.Count()];
                            for (int i = 0; i < model.skins[0].submeshes.Count(); i++)
                            {
                                if (filename.StartsWith("character", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    if (model.skins[0].submeshes[i].submeshID != 0)
                                    {
                                        if (!model.skins[0].submeshes[i].submeshID.ToString().EndsWith("01"))
                                        {
                                            continue;
                                        }
                                    }

                                    Console.WriteLine("Loading submesh " + model.skins[0].submeshes[i].submeshID + "(" + model.skins[0].submeshes[i].unk2 + ")");
                                }

                                ddBatch.submeshes[i].firstFace = model.skins[0].submeshes[i].startTriangle;
                                ddBatch.submeshes[i].numFaces = model.skins[0].submeshes[i].nTriangles;
                                for (int tu = 0; tu < model.skins[0].textureunit.Count(); tu++)
                                {
                                    if (model.skins[0].textureunit[tu].submeshIndex == i)
                                    {
                                        //Console.WriteLine("SubmeshIndex: " + i);
                                        //ddBatch.submeshes[i].blendType = model.renderflags[model.skins[0].textureunit[tu].renderFlags].blendingMode;
                                        //Console.WriteLine("Material ID: " + renderbatches[i].materialID);
                                        //Console.WriteLine("BlendType: " +renderbatches[i].blendType);
                                        ddBatch.submeshes[i].material = model.texlookup[model.skins[0].textureunit[tu].texture].textureID;
                                    }
                                }
                            }

                            // Vertices & indices
                            ddBatch.vertexBuffer = GL.GenBuffer();
                            ddBatch.indiceBuffer = GL.GenBuffer();

                            GL.BindBuffer(BufferTarget.ArrayBuffer, ddBatch.vertexBuffer);
                            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ddBatch.indiceBuffer);

                            List<int> modelindicelist = new List<int>();
                            for (int i = 0; i < model.skins[0].triangles.Count(); i++)
                            {
                                modelindicelist.Add(model.skins[0].triangles[i].pt1);
                                modelindicelist.Add(model.skins[0].triangles[i].pt2);
                                modelindicelist.Add(model.skins[0].triangles[i].pt3);
                            }

                            int[] modelindices = modelindicelist.ToArray();

                            Console.WriteLine(modelindicelist.Count() + " indices!");
                            ddBatch.indices = modelindices;

                            ddBatch.position = new Vector3(-(modelentry.position.X - 17066), modelentry.position.Y, -(modelentry.position.Z - 17066));

                            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ddBatch.indiceBuffer);
                            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(ddBatch.indices.Length * sizeof(int)), ddBatch.indices, BufferUsageHint.StaticDraw);

                            Vertex[] modelvertices = new Vertex[model.vertices.Count()];

                            for (int i = 0; i < model.vertices.Count(); i++)
                            {
                                modelvertices[i].Position = new Vector3(model.vertices[i].position.Y, model.vertices[i].position.Z, model.vertices[i].position.X);
                                modelvertices[i].Color = new Vector3(1.0f, 0.0f, 0.0f);
                                modelvertices[i].Normal = new Vector3(model.vertices[i].normal.Y, model.vertices[i].normal.Z, model.vertices[i].normal.X);
                                modelvertices[i].TexCoord = new Vector2(model.vertices[i].textureCoordX, model.vertices[i].textureCoordY);
                            }
                            GL.BindBuffer(BufferTarget.ArrayBuffer, ddBatch.vertexBuffer);
                            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(modelvertices.Length * 8 * sizeof(float)), modelvertices, BufferUsageHint.StaticDraw);

                            doodadBatches.Add(ddBatch);
                        }

                        List<WorldModelBatch> worldModelBatches = new List<WorldModelBatch>();

                        // WMO loading goes here

                        adt.materials = materials.ToArray();
                        adt.renderBatches = renderBatches.ToArray();
                        adt.doodadBatches = doodadBatches.ToArray();
                        adt.worldModelBatches = worldModelBatches.ToArray();
                        adts.Add(adt);
                    }
                }
            }

            indices = indicelist.ToArray();
            Vertex[] vertices = verticelist.ToArray();

            Console.WriteLine("Vertices in array: " + vertices.Count()); //37120, correct

            //indices = indicelist.ToArray();
            Console.WriteLine("Indices in array: " + indices.Count()); //196608, should be 65.5k which is 196608 / 3. in triangles so its correct?

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Count() * 11 * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[1]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(int)), indices, BufferUsageHint.StaticDraw);

            int verticeBufferSize = 0;
            int indiceBufferSize = 0;

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out verticeBufferSize);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[1]);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out indiceBufferSize);

            Console.WriteLine("Vertices in buffer: " + verticeBufferSize / 11 / sizeof(float));
            Console.WriteLine("Indices in buffer: " + indiceBufferSize / sizeof(int));
        }

        protected override void OnLoad(EventArgs e)
        {

            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.DepthTest);
            GL.ShadeModel(ShadingModel.Smooth);
            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            ActiveCamera.Pos = new Vector3(dragX, dragY, dragZ);
            ActiveCamera.setupGLRenderMatrix();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            ActiveCamera = new OldCamera(Width, Height);
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

            if (keyboardState.IsKeyDown(Key.L))
            {
                lightHeight = lightHeight + 50f;
            }
            if (keyboardState.IsKeyDown(Key.K))
            {
                lightHeight = lightHeight - 50f;
            }
            dragZ = (mouseState.WheelPrecise / 2) - 1068; //Startzoom is at -7.5f 
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            ActiveCamera.Pos = new Vector3(dragX, dragY, dragZ);  
            ActiveCamera.setupGLRenderMatrix();

            GL.Rotate(angle, 0.0, 1.0f, 0.0);
            DrawAxes();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.VertexArray);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.Enable(EnableCap.NormalArray);
            GL.EnableClientState(ArrayCap.NormalArray);
            GL.Enable(EnableCap.ColorArray);
            GL.EnableClientState(ArrayCap.ColorArray);
            GL.Enable(EnableCap.Texture2D);
            GL.EnableClientState(ArrayCap.TextureCoordArray);
            
            // GL.Enable(EnableCap.Blend);
            // GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);

            // GL.Light(LightName.Light0, LightParameter.Position, new float[] { 0.0f, 0.0f, 0.0f, 0.0f });
            // GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { 0.0f, 0.0f, 0.0f, 0.0f });
            // GL.Light(LightName.Light0, LightParameter.Specular, new float[] { 0.0f, 0.0f, 0.0f, 0.0f });
            // GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { 0.0f, 0.0f, 0.0f, 0.0f });
            // GL.Light(LightName.Light0, LightParameter.SpotExponent, 0.0f);
            // GL.LightModel(LightModelParameter.LightModelAmbient, new float[] { 0.0f, 0.0f, 0.0f, 0.0f });
            // GL.ShadeModel(ShadingModel.Smooth);

            
            // GL.Enable(EnableCap.Lighting);
            // GL.Enable(EnableCap.Light0);
            // GL.Enable(EnableCap.ColorMaterial);
            // GL.ColorMaterial(MaterialFace.Front, ColorMaterialParameter.AmbientAndDiffuse);

            // GL.Material(MaterialFace.Front, MaterialParameter.Specular, new float[] { 0.0f, 0.0f, 0.0f, 0.0f });
            // GL.Material(MaterialFace.Front, MaterialParameter.Shininess, new float[] { 0.0f, 0.0f, 0.0f, 0.0f });
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOid[0]);
            GL.NormalPointer(NormalPointerType.Float, 11 * sizeof(float), (IntPtr) 0);
            GL.ColorPointer(3, ColorPointerType.Float, 11 * sizeof(float), (IntPtr)(3 * sizeof(float)));
            GL.TexCoordPointer(2, TexCoordPointerType.Float, 11 * sizeof(float), (IntPtr)(6 * sizeof(float)));
            GL.VertexPointer(3, VertexPointerType.Float, 11 * sizeof(float), (IntPtr)(8 * sizeof(float)));
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOid[1]);

            for (int adti = 0; adti < adts.Count(); adti++)
            {
                for (int rb = 0; rb < adts[adti].renderBatches.Count(); rb++)
                {
                    //GL.DrawElements(PrimitiveType.Triangles, indices.Count(), DrawElementsType.UnsignedInt, 0);
                    GL.BindTexture(TextureTarget.Texture2D, adts[adti].materials[(int)adts[adti].renderBatches[rb].materialID].textureID);
                    GL.DrawRangeElements(PrimitiveType.Triangles, adts[adti].renderBatches[rb].firstFace, adts[adti].renderBatches[rb].firstFace + adts[adti].renderBatches[rb].numFaces, adts[adti].renderBatches[rb].numFaces, DrawElementsType.UnsignedInt, new IntPtr(adts[adti].renderBatches[rb].firstFace * 4));
                    //GL.DrawArrays(PrimitiveType.Triangles, renderBatches[rb].firstFace, renderBatches[rb].numFaces);
                }

                

                for (int db = 0; db < adts[adti].doodadBatches.Count(); db++)
                {
                    GL.PushMatrix();

                    GL.Translate(adts[adti].doodadBatches[db].position.X, adts[adti].doodadBatches[db].position.Y, adts[adti].doodadBatches[db].position.Z);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, adts[adti].doodadBatches[db].vertexBuffer);

                    //int verticeBufferSize = 0;
                    //GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out verticeBufferSize);
                    //Console.WriteLine("Vertices in buffer: " + verticeBufferSize / 11 / sizeof(float));

                    GL.NormalPointer(NormalPointerType.Float, 11 * sizeof(float), (IntPtr)0);
                    GL.ColorPointer(3, ColorPointerType.Float, 11 * sizeof(float), (IntPtr)(3 * sizeof(float)));
                    GL.TexCoordPointer(2, TexCoordPointerType.Float, 11 * sizeof(float), (IntPtr)(6 * sizeof(float)));
                    GL.VertexPointer(3, VertexPointerType.Float, 11 * sizeof(float), (IntPtr)(8 * sizeof(float)));
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, adts[adti].doodadBatches[db].indiceBuffer);

                    //int indiceBufferSize = 0;
                    //GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out indiceBufferSize);
                    //Console.WriteLine("Indices in buffer: " + indiceBufferSize / sizeof(int));

                    GL.DrawElements(PrimitiveType.Triangles, adts[adti].doodadBatches[db].indices.Count(), DrawElementsType.UnsignedInt, 0);

                    GL.PopMatrix();
                }
            }


            //GL.BindBuffer(BufferTarget.ArrayBuffer, modelVBOid[0]);
            //GL.NormalPointer(NormalPointerType.Float, 8 * sizeof(float), (IntPtr) 0);
            //GL.TexCoordPointer(2, TexCoordPointerType.Float, 8 * sizeof(float), (IntPtr)(3 * sizeof(float)));
            //GL.VertexPointer(3, VertexPointerType.Float, 8 * sizeof(float), (IntPtr)(5 * sizeof(float)));
            //GL.BindBuffer(BufferTarget.ElementArrayBuffer, modelVBOid[1]);

            if (GL.GetError().ToString() != "NoError")
            {
                Console.WriteLine(GL.GetError().ToString());
            }

            this.SwapBuffers();

        }

        protected override void OnUnload(EventArgs e)
        {
            Dispose();
            base.OnUnload(e);
            System.Windows.Application.Current.Shutdown();
        }

        struct Terrain
        {
            public RenderBatch[] renderBatches;
            public DoodadBatch[] doodadBatches;
            public WorldModelBatch[] worldModelBatches;
            public Material[] materials;
        }

        struct Vertex
        {
            public Vector3 Normal;
            public Vector3 Color;
            public Vector2 TexCoord;
            public Vector3 Position;
        }

        public struct Material
        {
            public string filename;
            public int textureID;
        }

        public struct RenderBatch
        {
            public int firstFace;
            public int numFaces;
            public uint materialID;
        }

        public struct DoodadBatch
        {
            public int vertexBuffer;
            public int indiceBuffer;
            public int[] indices;
            public Vector3 position;
            public Submesh[] submeshes;
            public Material[] mats;
        }

        public struct Submesh
        {
            public uint firstFace;
            public uint numFaces;
            public uint material;
        }

        public struct WorldModelBatch
        {
            public int vertexBuffer;
            public int indiceBuffer;
            public int[] indices;
            public Vector3 position;
        }
    }
}
