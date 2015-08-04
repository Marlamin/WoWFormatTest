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
        private List<Terrain> adts = new List<Terrain>();

        private bool mouseDragging = true;
        private Point mouseOldCoords;

        private Dictionary<string, WoWFormatLib.Structs.M2.M2Model> models = new Dictionary<string, WoWFormatLib.Structs.M2.M2Model>();
        private Dictionary<string, int> materialCache = new Dictionary<string, int>();
        private Dictionary<string, WoWFormatLib.Structs.WMO.WMO> worldModels = new Dictionary<string, WoWFormatLib.Structs.WMO.WMO>();
        private Dictionary<string, DoodadBatch> doodadBatches = new Dictionary<string, DoodadBatch>();

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

        private int LoadTexture(string filename)
        {

            filename = filename.ToLower();

            if (materialCache.ContainsKey(filename))
            {
                Console.WriteLine("[CACHE HIT] " + filename);
                return materialCache[filename];
            }

            Console.WriteLine("[CACHE MISS] " + filename);

            int textureId = GL.GenTexture();

            var blp = new BLPReader();

            blp.LoadBLP(filename);

            if (blp.bmp == null)
            {
                throw new Exception("BMP is null!");
            }
            else
            {
                GL.BindTexture(TextureTarget.Texture2D, textureId);
                materialCache.Add(filename, textureId);
                System.Drawing.Imaging.BitmapData bmp_data = blp.bmp.LockBits(new Rectangle(0, 0, blp.bmp.Width, blp.bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                blp.bmp.UnlockBits(bmp_data);
            }

            Console.WriteLine("[CACHE ADD] " + filename);

            return textureId;
        }

        private void LoadMap(string map, int centerx, int centery, int distance)
        {
            float TileSize = 1600.0f / 3.0f; //533.333
            float ChunkSize = TileSize / 16.0f; //33.333
            float UnitSize = ChunkSize / 8.0f; //4.166666 // ~~fun fact time with marlamin~~ this times 0.5 ends up being pixelspercoord on minimap
            float MapMidPoint = 32.0f / ChunkSize;

            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);

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

                        adt.vertexBuffer = GL.GenBuffer();
                        adt.indiceBuffer = GL.GenBuffer();

                        GL.BindBuffer(BufferTarget.ArrayBuffer, adt.vertexBuffer);
                        GL.BindBuffer(BufferTarget.ElementArrayBuffer, adt.indiceBuffer);

                        List<Material> materials = new List<Material>();
        
                        //Check if textures are already loaded or not, multiple ADTs close together probably use the same ones mostly
                        for (int ti = 0; ti < reader.adtfile.textures.filenames.Count(); ti++)
                        {
                            Material material = new Material();
                            material.filename = reader.adtfile.textures.filenames[ti];

                            if (!WoWFormatLib.Utils.CASC.FileExists(material.filename)) { continue; }

                            material.textureID = LoadTexture(reader.adtfile.textures.filenames[ti]);
  
                            materials.Add(material);
                        }
                       
                        var initialChunkY = reader.adtfile.chunks[0].header.position.Y;
                        var initialChunkX = reader.adtfile.chunks[0].header.position.X;

                        List<RenderBatch> renderBatches = new List<RenderBatch>();

                        for (uint c = 0; c < reader.adtfile.chunks.Count(); c++)
                        {
                            var chunk = reader.adtfile.chunks[c];

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

                            batch.firstFace = (uint) indicelist.Count();
                            for (int j = 9; j < 145; j++)
                            {
                                indicelist.AddRange(new Int32[] { off + j + 8, off + j - 9, off + j });
                                indicelist.AddRange(new Int32[] { off + j - 9, off + j - 8, off + j });
                                indicelist.AddRange(new Int32[] { off + j - 8, off + j + 9, off + j });
                                indicelist.AddRange(new Int32[] { off + j + 9, off + j + 8, off + j });
                                if ((j + 1) % (9 + 8) == 0) j += 9;
                            }
                            batch.numFaces = (uint)(indicelist.Count()) - batch.firstFace;

                            if (!materialCache.ContainsKey(reader.adtfile.textures.filenames[reader.adtfile.texChunks[c].layers[0].textureId].ToLower()))
                            {
                                throw new Exception("MaterialCache does not have texture " + reader.adtfile.textures.filenames[reader.adtfile.texChunks[c].layers[0].textureId].ToLower());
                            }

                            batch.materialID = (uint) materialCache[reader.adtfile.textures.filenames[reader.adtfile.texChunks[c].layers[0].textureId]];

                            renderBatches.Add(batch);
                        }

                        List<Doodad> doodads = new List<Doodad>();

                        for (int mi = 0; mi < reader.adtfile.objects.models.entries.Count(); mi++)
                        {
                            WoWFormatLib.Structs.M2.M2Model model = new WoWFormatLib.Structs.M2.M2Model();
                            var modelentry = reader.adtfile.objects.models.entries[mi];
                            var mmid = reader.adtfile.objects.m2NameOffsets.offsets[modelentry.mmidEntry];
                            var doodad = new Doodad();

                            for (int mmi = 0; mmi < reader.adtfile.objects.m2Names.offsets.Count(); mmi++)
                            {
                                if (reader.adtfile.objects.m2Names.offsets[mmi] == mmid)
                                {

                                    if (models.ContainsKey(reader.adtfile.objects.m2Names.filenames[mmi]))
                                    {
                                        //Load model from memory
                                        model = models[reader.adtfile.objects.m2Names.filenames[mmi]];
                                       // Console.WriteLine("Loaded M2 from memory " + model.filename + " which as " + model.vertices.Count() + " vertices");
                                    }
                                    else
                                    {
                                        //Load model from file
                                        if (WoWFormatLib.Utils.CASC.FileExists(reader.adtfile.objects.m2Names.filenames[mmi]))
                                        {
                                            var modelreader = new M2Reader();
                                            modelreader.LoadM2(reader.adtfile.objects.m2Names.filenames[mmi]);
                                            models.Add(reader.adtfile.objects.m2Names.filenames[mmi], modelreader.model);
                                            model = modelreader.model;
                                           // Console.WriteLine("Loaded M2 from disk " + modelreader.model.filename + " which as " + modelreader.model.vertices.Count() + " vertices");
                                        }
                                        else
                                        {
                                            throw new Exception("Model " + reader.adtfile.objects.m2Names.filenames[mmi] + " does not exist!");
                                        }
                                    }
                                }
                            }

                            doodad.filename = model.filename;
                            doodad.position = new Vector3(-(modelentry.position.X - 17066), modelentry.position.Y, -(modelentry.position.Z - 17066));
                            doodad.rotation = new Vector3(modelentry.rotation.X, modelentry.rotation.Y, modelentry.rotation.Z);
                            doodad.scale = modelentry.scale;
                            doodads.Add(doodad);

                            if (doodadBatches.ContainsKey(model.filename))
                            {
                                //Console.WriteLine("Loading doodadbatch from cache " + model.filename + "!");
                                continue;
                            }

                           // Console.WriteLine("Parsing model " + model.filename + "!");

                            if (model.filename == null)
                            {
                                throw new Exception("Model isn't loaded!!!!!");
                            }

                            var ddBatch = new DoodadBatch();

                            // Textures
                            ddBatch.mats = new Material[model.textures.Count()];

                            for (int i = 0; i < model.textures.Count(); i++)
                            {
                                string texturefilename = model.textures[i].filename;
                                ddBatch.mats[i].textureID = LoadTexture(texturefilename);
                                ddBatch.mats[i].filename = texturefilename;
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

                                    //Console.WriteLine("Loading submesh " + model.skins[0].submeshes[i].submeshID + "(" + model.skins[0].submeshes[i].unk2 + ")");
                                }

                                ddBatch.submeshes[i].firstFace = model.skins[0].submeshes[i].startTriangle;
                                ddBatch.submeshes[i].numFaces = model.skins[0].submeshes[i].nTriangles;
                                for (int tu = 0; tu < model.skins[0].textureunit.Count(); tu++)
                                {
                                    if (model.skins[0].textureunit[tu].submeshIndex == i)
                                    {
                                        ddBatch.submeshes[i].blendType = model.renderflags[model.skins[0].textureunit[tu].renderFlags].blendingMode;
                                        if (!materialCache.ContainsKey(model.textures[model.texlookup[model.skins[0].textureunit[tu].texture].textureID].filename.ToLower()))
                                        {
                                           throw new Exception("MaterialCache does not have texture " + model.textures[model.texlookup[model.skins[0].textureunit[tu].texture].textureID].filename.ToLower());
                                        }

                                        ddBatch.submeshes[i].material = (uint) materialCache[model.textures[model.texlookup[model.skins[0].textureunit[tu].texture].textureID].filename.ToLower()];
                                    }
                                }
                            }

                            // Vertices & indices
                            ddBatch.vertexBuffer = GL.GenBuffer();
                            ddBatch.indiceBuffer = GL.GenBuffer();

                            GL.BindBuffer(BufferTarget.ArrayBuffer, ddBatch.vertexBuffer);
                            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ddBatch.indiceBuffer);

                            List<uint> modelindicelist = new List<uint>();
                            for (int i = 0; i < model.skins[0].triangles.Count(); i++)
                            {
                                modelindicelist.Add(model.skins[0].triangles[i].pt1);
                                modelindicelist.Add(model.skins[0].triangles[i].pt2);
                                modelindicelist.Add(model.skins[0].triangles[i].pt3);
                            }

                            uint[] modelindices = modelindicelist.ToArray();

                            //Console.WriteLine(modelindicelist.Count() + " indices!");
                            ddBatch.indices = modelindices;

                            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ddBatch.indiceBuffer);
                            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(ddBatch.indices.Length * sizeof(uint)), ddBatch.indices, BufferUsageHint.StaticDraw);

                            M2Vertex[] modelvertices = new M2Vertex[model.vertices.Count()];

                            for (int i = 0; i < model.vertices.Count(); i++)
                            {
                                modelvertices[i].Position = new Vector3(model.vertices[i].position.X, model.vertices[i].position.Z, model.vertices[i].position.Y);
                                modelvertices[i].Normal = new Vector3(model.vertices[i].normal.X, model.vertices[i].normal.Z, model.vertices[i].normal.Y);
                                modelvertices[i].TexCoord = new Vector2(model.vertices[i].textureCoordX, model.vertices[i].textureCoordY);
                            }
                            GL.BindBuffer(BufferTarget.ArrayBuffer, ddBatch.vertexBuffer);
                            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(modelvertices.Length * 8 * sizeof(float)), modelvertices, BufferUsageHint.StaticDraw);

                            doodadBatches.Add(model.filename, ddBatch);
                        }

                        List<WorldModelBatch> worldModelBatches = new List<WorldModelBatch>();

                        // WMO loading goes here
                        for(int wmi = 0; wmi < reader.adtfile.objects.worldModels.entries.Count(); wmi++)
                        {

                            string wmofilename = "";

                            var wmobatch = new WorldModelBatch();

                            var wmodelentry = reader.adtfile.objects.worldModels.entries[wmi];
                            var mwid = reader.adtfile.objects.wmoNameOffsets.offsets[wmodelentry.mwidEntry];

                            for (int wmfi = 0; wmfi < reader.adtfile.objects.wmoNames.offsets.Count(); wmfi++)
                            {
                                if (reader.adtfile.objects.wmoNames.offsets[wmfi] == mwid) {
                                    wmofilename = reader.adtfile.objects.wmoNames.filenames[wmfi];
                                }

                            }

                            if(wmofilename.Length == 0)
                            {
                                throw new Exception("Unable to find filename for WMO!");
                            }

                            WMOReader wmoreader = new WMOReader();

                            if (worldModels.ContainsKey(wmofilename))
                            {
                                Console.WriteLine("Loading WMO " + wmofilename + " from cache!");
                                wmoreader.wmofile = worldModels[wmofilename];
                            }
                            else
                            {
                                Console.WriteLine("Loading WMO " + wmofilename + " from disk");
                                wmoreader.LoadWMO(wmofilename);
                                worldModels.Add(wmofilename, wmoreader.wmofile);
                            }

                            if(wmoreader.wmofile.group.Count() == 0)
                            {
                                throw new Exception("WMO groups not found!");
                            }

                            //WMO doodads
                            //for (int i = 0; i < wmoreader.wmofile.doodadNames.Count(); i++)
                            //{
                            //Console.WriteLine(reader.wmofile.doodadNames[i].filename);
                            //}

                            wmobatch.groupBatches = new WorldModelGroupBatches[wmoreader.wmofile.group.Count()];

                            for (int g = 0; g < wmoreader.wmofile.group.Count(); g++)
                            {
                                if (wmoreader.wmofile.group[g].mogp.vertices == null) { continue; }

                                wmobatch.groupBatches[g].vertexBuffer = GL.GenBuffer();
                                wmobatch.groupBatches[g].indiceBuffer = GL.GenBuffer();
                                GL.BindBuffer(BufferTarget.ArrayBuffer, wmobatch.groupBatches[g].vertexBuffer);

                                M2Vertex[] wmovertices = new M2Vertex[wmoreader.wmofile.group[g].mogp.vertices.Count()];

                                for (int i = 0; i < wmoreader.wmofile.group[g].mogp.vertices.Count(); i++)
                                {
                                    float f;
                                    wmovertices[i].Position = new Vector3(wmoreader.wmofile.group[g].mogp.vertices[i].vector.X, wmoreader.wmofile.group[g].mogp.vertices[i].vector.Z, wmoreader.wmofile.group[g].mogp.vertices[i].vector.Y);
                                    wmovertices[i].Normal = new Vector3(wmoreader.wmofile.group[g].mogp.normals[i].normal.X, wmoreader.wmofile.group[g].mogp.normals[i].normal.Z, wmoreader.wmofile.group[g].mogp.normals[i].normal.Y);
                                    if (wmoreader.wmofile.group[g].mogp.textureCoords[0] == null)
                                    {
                                        wmovertices[i].TexCoord = new Vector2(0.0f, 0.0f);
                                    }
                                    else
                                    {
                                        wmovertices[i].TexCoord = new Vector2(wmoreader.wmofile.group[g].mogp.textureCoords[0][i].X, wmoreader.wmofile.group[g].mogp.textureCoords[0][i].Y);
                                    }
                                }

                                //Push to buffer
                                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(wmovertices.Length * 8 * sizeof(float)), wmovertices, BufferUsageHint.StaticDraw);

                                //Switch to Index buffer
                                GL.BindBuffer(BufferTarget.ElementArrayBuffer, wmobatch.groupBatches[g].indiceBuffer);

                                List<uint> wmoindicelist = new List<uint>();
                                for (int i = 0; i < wmoreader.wmofile.group[g].mogp.indices.Count(); i++)
                                {
                                    wmoindicelist.Add(wmoreader.wmofile.group[g].mogp.indices[i].indice);
                                }

                                wmobatch.groupBatches[g].indices = wmoindicelist.ToArray();

                                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(wmobatch.groupBatches[g].indices.Length * sizeof(uint)), wmobatch.groupBatches[g].indices, BufferUsageHint.StaticDraw);
                            }

                            GL.Enable(EnableCap.Texture2D);

                            wmobatch.mats = new Material[wmoreader.wmofile.materials.Count()];
                            for (int i = 0; i < wmoreader.wmofile.materials.Count(); i++)
                            {
                                for (int ti = 0; ti < wmoreader.wmofile.textures.Count(); ti++)
                                {

                                    if (wmoreader.wmofile.textures[ti].startOffset == wmoreader.wmofile.materials[i].texture1)
                                    {
                                        wmobatch.mats[i].texture1 = wmoreader.wmofile.materials[i].texture1;
                                        wmobatch.mats[i].textureID = LoadTexture(wmoreader.wmofile.textures[ti].filename);
                                        wmobatch.mats[i].filename = wmoreader.wmofile.textures[ti].filename;
                                    }
                                }
                            }

                            wmobatch.position = new Vector3(-(wmodelentry.position.X - 17066), wmodelentry.position.Y, -(wmodelentry.position.Z - 17066));
                            wmobatch.rotation = new Vector3(wmodelentry.rotation.X, wmodelentry.rotation.Y, wmodelentry.rotation.Z);
                            
                            int numRenderbatches = 0;
                            //Get total amount of render batches
                            for (int i = 0; i < wmoreader.wmofile.group.Count(); i++)
                            {
                                if (wmoreader.wmofile.group[i].mogp.renderBatches == null) { continue; }
                                numRenderbatches = numRenderbatches + wmoreader.wmofile.group[i].mogp.renderBatches.Count();
                            }

                            wmobatch.wmoRenderBatch = new WMORenderBatch[numRenderbatches];

                            int rb = 0;
                            for (int g = 0; g < wmoreader.wmofile.group.Count(); g++)
                            {
                                var group = wmoreader.wmofile.group[g];
                                if (group.mogp.renderBatches == null) { continue; }
                                for (int i = 0; i < group.mogp.renderBatches.Count(); i++)
                                {
                                    wmobatch.wmoRenderBatch[rb].firstFace = group.mogp.renderBatches[i].firstFace;
                                    wmobatch.wmoRenderBatch[rb].numFaces = group.mogp.renderBatches[i].numFaces;
                                    for (int ti = 0; ti < wmobatch.mats.Count(); ti++)
                                    {
                                        if(wmoreader.wmofile.materials[group.mogp.renderBatches[i].materialID].texture1 == wmobatch.mats[ti].texture1)
                                        {
                                            wmobatch.wmoRenderBatch[rb].materialID = (uint) wmobatch.mats[ti].textureID;
                                        }
                                    }

                                    wmobatch.wmoRenderBatch[rb].blendType = wmoreader.wmofile.materials[group.mogp.renderBatches[i].materialID].blendMode;
                                    wmobatch.wmoRenderBatch[rb].groupID = (uint)g;
                                    rb++;
                                }
                            }

                            worldModelBatches.Add(wmobatch);
                        }

                        adt.renderBatches = renderBatches.ToArray();
                        adt.doodads = doodads.ToArray();
                        adt.worldModelBatches = worldModelBatches.ToArray();

                        int[] indices = indicelist.ToArray();
                        Vertex[] vertices = verticelist.ToArray();

                        Console.WriteLine("Vertices in array: " + vertices.Count()); //37120, correct

                        //indices = indicelist.ToArray();
                        Console.WriteLine("Indices in array: " + indices.Count()); //196608, should be 65.5k which is 196608 / 3. in triangles so its correct?

                        GL.BindBuffer(BufferTarget.ArrayBuffer, adt.vertexBuffer);
                        GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Count() * 11 * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

                        GL.BindBuffer(BufferTarget.ElementArrayBuffer, adt.indiceBuffer);
                        GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(int)), indices, BufferUsageHint.StaticDraw);

                        int verticeBufferSize = 0;
                        int indiceBufferSize = 0;

                        GL.BindBuffer(BufferTarget.ArrayBuffer, adt.vertexBuffer);
                        GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out verticeBufferSize);

                        GL.BindBuffer(BufferTarget.ArrayBuffer, adt.indiceBuffer);
                        GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out indiceBufferSize);

                        Console.WriteLine("Vertices in buffer: " + verticeBufferSize / 11 / sizeof(float));
                        Console.WriteLine("Indices in buffer: " + indiceBufferSize / sizeof(int));

                        adts.Add(adt);
                    }
                }
            }


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

                if (keyboardState.IsKeyDown(Key.ShiftLeft))
                {
                    dragY = dragY + mouseMovementY / 2;
                    dragX = dragX + mouseMovementX / 2;

                }
                else
                {
                    //if(mouseMovementX < 0)
                    //{
                    //    dragX = dragX + mouseMovementX * angle;
                    //}

                    angle = angle + mouseMovementX / 10f;
                }

                mouseOldCoords = mouseNewCoords;
            }

            if (mouseState.LeftButton == ButtonState.Released)
            {
                mouseDragging = false;
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

            GL.AlphaFunc(AlphaFunction.Greater, 0.5f);
            GL.Enable(EnableCap.VertexArray);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.Enable(EnableCap.NormalArray);
            GL.EnableClientState(ArrayCap.NormalArray);
            GL.Enable(EnableCap.ColorArray);
            GL.EnableClientState(ArrayCap.ColorArray);
            GL.Enable(EnableCap.Texture2D);
            GL.EnableClientState(ArrayCap.TextureCoordArray);
            GL.Enable(EnableCap.AlphaTest);

            // GL.Enable(EnableCap.Blend);
            // GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);

            //GL.Light(LightName.Light0, LightParameter.Position, new float[] { 0.0f, 0.0f, 0.0f, 0.0f });
            //GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { 0.0f, 0.0f, 0.0f, 0.0f });
            //GL.Light(LightName.Light0, LightParameter.Specular, new float[] { 0.0f, 0.0f, 0.0f, 0.0f });
            //GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { 0.0f, 0.0f, 0.0f, 0.0f });
            //GL.Light(LightName.Light0, LightParameter.SpotExponent, 0.0f);
            //GL.LightModel(LightModelParameter.LightModelAmbient, new float[] { 0.0f, 0.0f, 0.0f, 0.0f });
            //GL.ShadeModel(ShadingModel.Smooth);

            //GL.Enable(EnableCap.Lighting);
            //GL.Enable(EnableCap.Light0);
            //GL.Enable(EnableCap.ColorMaterial);
            //GL.ColorMaterial(MaterialFace.Front, ColorMaterialParameter.AmbientAndDiffuse);

            //GL.Material(MaterialFace.Front, MaterialParameter.Specular, new float[] { 0.0f, 0.0f, 0.0f, 0.0f });
            //GL.Material(MaterialFace.Front, MaterialParameter.Shininess, new float[] { 0.0f, 0.0f, 0.0f, 0.0f });



            for (int adti = 0; adti < adts.Count(); adti++)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, adts[adti].vertexBuffer);
                GL.NormalPointer(NormalPointerType.Float, 11 * sizeof(float), (IntPtr)0);
                GL.ColorPointer(3, ColorPointerType.Float, 11 * sizeof(float), (IntPtr)(3 * sizeof(float)));
                GL.TexCoordPointer(2, TexCoordPointerType.Float, 11 * sizeof(float), (IntPtr)(6 * sizeof(float)));
                GL.VertexPointer(3, VertexPointerType.Float, 11 * sizeof(float), (IntPtr)(8 * sizeof(float)));
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, adts[adti].indiceBuffer);
                for (int rb = 0; rb < adts[adti].renderBatches.Count(); rb++)
                {
                    //GL.DrawElements(PrimitiveType.Triangles, indices.Count(), DrawElementsType.UnsignedInt, 0);
                    GL.BindTexture(TextureTarget.Texture2D, (int)adts[adti].renderBatches[rb].materialID);
                    GL.DrawRangeElements(PrimitiveType.Triangles, (int)adts[adti].renderBatches[rb].firstFace, (int)adts[adti].renderBatches[rb].firstFace + (int)adts[adti].renderBatches[rb].numFaces, (int)adts[adti].renderBatches[rb].numFaces, DrawElementsType.UnsignedInt, new IntPtr(adts[adti].renderBatches[rb].firstFace * 4));
                    //GL.DrawArrays(PrimitiveType.Triangles, renderBatches[rb].firstFace, renderBatches[rb].numFaces);
                }

                GL.Disable(EnableCap.ColorArray);
                GL.DisableClientState(ArrayCap.ColorArray);

                for (int di = 0; di < adts[adti].doodads.Count(); di++)
                { 
                    GL.PushMatrix();

                    var activeDoodadBatch = doodadBatches[adts[adti].doodads[di].filename];

                    GL.Translate(adts[adti].doodads[di].position.X, adts[adti].doodads[di].position.Y, adts[adti].doodads[di].position.Z);
                    //GL.Rotate(Math.Atan2(adts[adti].doodadBatches[db].rotation.X, adts[adti].doodadBatches[db].rotation.Z), adts[adti].doodadBatches[db].rotation.Z, adts[adti].doodadBatches[db].rotation.X, adts[adti].doodadBatches[db].rotation.Z + Math.PI);

                    //GL.Rotate(-adts[adti].doodads[di].rotation.X + (float)ControlsWindow.amb_1, 0, 0, 1);
                    //GL.Rotate((adts[adti].doodads[di].rotation.Y + 90) + (float)ControlsWindow.amb_2, 0, 1, 0);
                    //GL.Rotate(-adts[adti].doodads[di].rotation.Z + (float)ControlsWindow.amb_3, 1, 0, 0);

                    GL.Rotate(adts[adti].doodads[di].rotation.Y - 90.0f, 0.0f, 1.0f, 0.0f);
                    GL.Rotate(-adts[adti].doodads[di].rotation.X, 0.0f, 0.0f, 1.0f);
                    GL.Rotate(adts[adti].doodads[di].rotation.Z, 1.0f, 0.0f, 0.0f);

                    var scale = adts[adti].doodads[di].scale / 1024f;
                    GL.Scale(-scale, scale, scale);

                    GL.BindBuffer(BufferTarget.ArrayBuffer, activeDoodadBatch.vertexBuffer);

                    //int verticeBufferSize = 0;
                    //GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out verticeBufferSize);
                    //Console.WriteLine("Vertices in buffer: " + verticeBufferSize / 11 / sizeof(float));

                    GL.NormalPointer(NormalPointerType.Float, 8 * sizeof(float), (IntPtr)0);
                    GL.TexCoordPointer(2, TexCoordPointerType.Float, 8 * sizeof(float), (IntPtr)(3 * sizeof(float)));
                    GL.VertexPointer(3, VertexPointerType.Float, 8 * sizeof(float), (IntPtr)(5 * sizeof(float)));
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, activeDoodadBatch.indiceBuffer);

                    //int indiceBufferSize = 0;
                    //GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out indiceBufferSize);
                    //Console.WriteLine("Indices in buffer: " + indiceBufferSize / sizeof(int));
                    for (int si = 0; si < activeDoodadBatch.submeshes.Count(); si++)
                    {
                        switch (activeDoodadBatch.submeshes[si].blendType)
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
                                throw new Exception("Unknown blend type " + activeDoodadBatch.submeshes[si].blendType);
                        }
                        GL.BindTexture(TextureTarget.Texture2D, activeDoodadBatch.submeshes[si].material);
                        GL.DrawRangeElements(PrimitiveType.Triangles, activeDoodadBatch.submeshes[si].firstFace, (activeDoodadBatch.submeshes[si].firstFace + activeDoodadBatch.submeshes[si].numFaces), (int)activeDoodadBatch.submeshes[si].numFaces, DrawElementsType.UnsignedInt, new IntPtr(activeDoodadBatch.submeshes[si].firstFace * 4));
                    }

                    GL.PopMatrix();
                }
                
                for (int wb = 0; wb < adts[adti].worldModelBatches.Count(); wb++)
                {
                    for (int wrb = 0; wrb < adts[adti].worldModelBatches[wb].groupBatches.Count(); wrb++)
                    {
                        GL.PushMatrix();

                        GL.Translate(adts[adti].worldModelBatches[wb].position.X, adts[adti].worldModelBatches[wb].position.Y, adts[adti].worldModelBatches[wb].position.Z);

                        // GL.Rotate(-adts[adti].worldModelBatches[wb].rotation.X + (float)ControlsWindow.amb_1, 0, 0, 1);
                        //  GL.Rotate((adts[adti].worldModelBatches[wb].rotation.Y + 90) + (float)ControlsWindow.amb_2, 0, 1, 0);
                        //  GL.Rotate(-adts[adti].worldModelBatches[wb].rotation.Z + (float)ControlsWindow.amb_3, 1, 0, 0);
                        GL.Rotate(adts[adti].worldModelBatches[wb].rotation.Y - 90.0f, 0.0f, 1.0f, 0.0f);
                        GL.Rotate(-adts[adti].worldModelBatches[wb].rotation.X, 0.0f, 0.0f, 1.0f);
                        GL.Rotate(adts[adti].worldModelBatches[wb].rotation.Z, 1.0f, 0.0f, 0.0f);

                        GL.Scale(-1.0f, 1.0f, 1.0f);
                        for (int si = 0; si < adts[adti].worldModelBatches[wb].wmoRenderBatch.Count(); si++)
                        {
                            GL.BindBuffer(BufferTarget.ArrayBuffer, adts[adti].worldModelBatches[wb].groupBatches[adts[adti].worldModelBatches[wb].wmoRenderBatch[si].groupID].vertexBuffer);
                            GL.NormalPointer(NormalPointerType.Float, 8 * sizeof(float), (IntPtr)0);
                            GL.TexCoordPointer(2, TexCoordPointerType.Float, 8 * sizeof(float), (IntPtr)(3 * sizeof(float)));
                            GL.VertexPointer(3, VertexPointerType.Float, 8 * sizeof(float), (IntPtr)(5 * sizeof(float)));
                            GL.BindBuffer(BufferTarget.ElementArrayBuffer, adts[adti].worldModelBatches[wb].groupBatches[adts[adti].worldModelBatches[wb].wmoRenderBatch[si].groupID].indiceBuffer);
                            switch (adts[adti].worldModelBatches[wb].wmoRenderBatch[si].blendType)
                            {
                                case 0:
                                    GL.Disable(EnableCap.Blend);
                                    break;
                                case 1:
                                    GL.Enable(EnableCap.Blend);
                                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                                    break;
                                case 2: 
                                    GL.Enable(EnableCap.Blend);
                                    GL.BlendFuncSeparate(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                                    break;
                                case 3:
                                    GL.Enable(EnableCap.Blend);
                                    GL.BlendFunc(BlendingFactorSrc.SrcColor, BlendingFactorDest.DstColor);
                                    break;
                                case 4:
                                    GL.Enable(EnableCap.Blend);
                                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
                                    break;
                                case 5:
                                    GL.Enable(EnableCap.Blend);
                                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                                    break;
                                case 6:
                                    GL.Enable(EnableCap.Blend);
                                    GL.BlendFunc(BlendingFactorSrc.DstColor, BlendingFactorDest.SrcColor);
                                    break;
                                case 7: 
                                    break;
                                default:
                                    throw new Exception("Unknown blend type " + adts[adti].worldModelBatches[wb].wmoRenderBatch[si].blendType);
                            }
                            GL.BindTexture(TextureTarget.Texture2D, adts[adti].worldModelBatches[wb].wmoRenderBatch[si].materialID);
                            GL.DrawRangeElements(PrimitiveType.Triangles, adts[adti].worldModelBatches[wb].wmoRenderBatch[si].firstFace, (adts[adti].worldModelBatches[wb].wmoRenderBatch[si].firstFace + adts[adti].worldModelBatches[wb].wmoRenderBatch[si].numFaces), (int)adts[adti].worldModelBatches[wb].wmoRenderBatch[si].numFaces, DrawElementsType.UnsignedInt, new IntPtr(adts[adti].worldModelBatches[wb].wmoRenderBatch[si].firstFace * 4));
                        }

                        GL.PopMatrix();
                    }
                }

                GL.Enable(EnableCap.ColorArray);
                GL.EnableClientState(ArrayCap.ColorArray);
            }

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
            public int vertexBuffer;
            public int indiceBuffer;
            public RenderBatch[] renderBatches;
            public Doodad[] doodads;
            public WorldModelBatch[] worldModelBatches;
        }

        struct Vertex
        {
            public Vector3 Normal;
            public Vector3 Color;
            public Vector2 TexCoord;
            public Vector3 Position;
        }

        struct M2Vertex
        {
            public Vector3 Normal;
            public Vector2 TexCoord;
            public Vector3 Position;

        }
        public struct Material
        {
            public string filename;
            public int textureID;
            internal uint texture1;
        }

        public struct RenderBatch
        {
            public uint firstFace;
            public uint numFaces;
            public uint materialID;
        }

        public struct WMORenderBatch
        {
            public uint firstFace;
            public uint materialID;
            public uint numFaces;
            public uint groupID;
            public uint blendType;
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

        public struct Submesh
        {
            public uint firstFace;
            public uint numFaces;
            public uint material;
            public uint blendType;
        }

        public struct WorldModelBatch
        {
            public WorldModelGroupBatches[] groupBatches;
            public Material[] mats;
            public WMORenderBatch[] wmoRenderBatch;
            public Vector3 position;
            public Vector3 rotation;
        }

        public struct WorldModelGroupBatches
        {
            public int vertexBuffer;
            public int indiceBuffer;
            public uint[] indices;
        }
    }
}
