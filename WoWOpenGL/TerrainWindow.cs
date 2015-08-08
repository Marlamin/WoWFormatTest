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
using WoWOpenGL.Loaders;

namespace WoWOpenGL
{
    public class TerrainWindow : GameWindow
    {
        private static float dragX;
        private static float dragY;
        private static float dragZ;
        private static float angle;
        private static float lightHeight = 0.0f;
        private static float camSpeed = 0.25f;
        private List<Terrain> adts = new List<Terrain>();

        private Dictionary<Key, int> CoolOffKeys = new Dictionary<Key, int>();

        private bool mouseDragging = true;
        private Point mouseOldCoords;

        private CacheStorage cache = new CacheStorage();

        OldCamera ActiveCamera;

        public TerrainWindow(string modelPath)
            : base(1920, 1080, new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8), "Terrain test", GameWindowFlags.Default, DisplayDevice.Default, 3, 0, OpenTK.Graphics.GraphicsContextFlags.Default)
        {
            dragX = 0;
            dragY = 0;
            dragZ = 0;
            angle = 0.0f;

            Keyboard.KeyDown += Keyboard_KeyDown;

            ActiveCamera = new OldCamera(Width, Height);
            ActiveCamera.Pos = new Vector3(10.0f, -10.0f, -7.5f);

            Console.WriteLine(modelPath);

            string[] adt = modelPath.Split('_');

            Console.WriteLine("MAP {0}, X {1}, Y {2}", adt[0], adt[1], adt[2]);
            //LoadADT(adt[0], adt[1], adt[2]);
            LoadMap(adt[0], int.Parse(adt[1]), int.Parse(adt[2]), 1);
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

                            material.textureID = BLPLoader.LoadTexture(reader.adtfile.textures.filenames[ti], cache);

                            materials.Add(material);
                        }

                        var initialChunkY = reader.adtfile.chunks[0].header.position.Y;
                        var initialChunkX = reader.adtfile.chunks[0].header.position.X;

                        /*  if(firstLocation.X == 0)
                          {
                              firstLocation = new Vector3(initialChunkY, initialChunkX, 1.0f);
                              Console.WriteLine("Setting first location to " + firstLocation.ToString());
                          }
                          */
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

                            batch.firstFace = (uint)indicelist.Count();
                            for (int j = 9; j < 145; j++)
                            {
                                indicelist.AddRange(new Int32[] { off + j + 8, off + j - 9, off + j });
                                indicelist.AddRange(new Int32[] { off + j - 9, off + j - 8, off + j });
                                indicelist.AddRange(new Int32[] { off + j - 8, off + j + 9, off + j });
                                indicelist.AddRange(new Int32[] { off + j + 9, off + j + 8, off + j });
                                if ((j + 1) % (9 + 8) == 0) j += 9;
                            }
                            batch.numFaces = (uint)(indicelist.Count()) - batch.firstFace;

                            if (!cache.materials.ContainsKey(reader.adtfile.textures.filenames[reader.adtfile.texChunks[c].layers[0].textureId].ToLower()))
                            {
                                throw new Exception("MaterialCache does not have texture " + reader.adtfile.textures.filenames[reader.adtfile.texChunks[c].layers[0].textureId].ToLower());
                            }

                            batch.materialID = (uint)cache.materials[reader.adtfile.textures.filenames[reader.adtfile.texChunks[c].layers[0].textureId].ToLower()];

                            renderBatches.Add(batch);
                        }

                        List<Doodad> doodads = new List<Doodad>();

                        for (int mi = 0; mi < reader.adtfile.objects.models.entries.Count(); mi++)
                        {
                            Console.WriteLine("Loading model #" + mi);

                            var modelentry = reader.adtfile.objects.models.entries[mi];
                            var mmid = reader.adtfile.objects.m2NameOffsets.offsets[modelentry.mmidEntry];

                            var modelfilename = "";
                            for (int mmi = 0; mmi < reader.adtfile.objects.m2Names.offsets.Count(); mmi++)
                            {
                                if (reader.adtfile.objects.m2Names.offsets[mmi] == mmid)
                                {
                                    modelfilename = reader.adtfile.objects.m2Names.filenames[mmi].ToLower();
                                }
                            }

                            var doodad = new Doodad();
                            doodad.filename = modelfilename;
                            doodad.position = new Vector3(-(modelentry.position.X - 17066), modelentry.position.Y, -(modelentry.position.Z - 17066));
                            doodad.rotation = new Vector3(modelentry.rotation.X, modelentry.rotation.Y, modelentry.rotation.Z);
                            doodad.scale = modelentry.scale;
                            doodads.Add(doodad);

                            if (cache.doodadBatches.ContainsKey(modelfilename))
                            {
                                continue;
                            }

                            M2Loader.LoadM2(modelfilename, cache);
                        }

                        List<WorldModelBatch> worldModelBatches = new List<WorldModelBatch>();

                        // WMO loading goes here
                        for (int wmi = 0; wmi < reader.adtfile.objects.worldModels.entries.Count(); wmi++)
                        {
                            Console.WriteLine("Loading WMO #" + wmi);
                            string wmofilename = "";

                            var wmodelentry = reader.adtfile.objects.worldModels.entries[wmi];
                            var mwid = reader.adtfile.objects.wmoNameOffsets.offsets[wmodelentry.mwidEntry];

                            for (int wmfi = 0; wmfi < reader.adtfile.objects.wmoNames.offsets.Count(); wmfi++)
                            {
                                if (reader.adtfile.objects.wmoNames.offsets[wmfi] == mwid)
                                {
                                    wmofilename = reader.adtfile.objects.wmoNames.filenames[wmfi].ToLower();
                                }

                            }

                            if (wmofilename.Length == 0)
                            {
                                throw new Exception("Unable to find filename for WMO!");
                            }

                            WorldModelBatch wmobatch = new WorldModelBatch();
                            wmobatch.position = new Vector3(-(wmodelentry.position.X - 17066), wmodelentry.position.Y, -(wmodelentry.position.Z - 17066));
                            wmobatch.rotation = new Vector3(wmodelentry.rotation.X, wmodelentry.rotation.Y, wmodelentry.rotation.Z);
                            wmobatch.worldModel = WMOLoader.LoadWMO(wmofilename, cache);
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
            if (!this.Focused)
            {
                return;
            }

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

            if (keyboardState.IsKeyDown(Key.ShiftLeft))
            {
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

            if (keyboardState.IsKeyDown(Key.R))//Reset
            {
                dragX = dragY = dragZ = angle = 0;
            }

            if (!CoolOffKeys.ContainsKey(Key.Right) && keyboardState.IsKeyDown(Key.Right))
            {
                angle += 90;
                CoolOffKey(Key.Right);
            }

            if (!CoolOffKeys.ContainsKey(Key.Left) && keyboardState.IsKeyDown(Key.Left))
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

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            angle = angle % 360;

            //int dragXa = angle % 90

            ActiveCamera.Pos = new Vector3(dragX, dragY, dragZ);

            ActiveCamera.setupGLRenderMatrix();


            GL.Translate(-dragX, -dragY, -dragZ);
            GL.Rotate(angle, 0.0, 1.0f, 0.0);
            GL.Translate(dragX, dragY, dragZ);
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

                    var activeDoodadBatch = cache.doodadBatches[adts[adti].doodads[di].filename];

                    GL.Translate(adts[adti].doodads[di].position.X, adts[adti].doodads[di].position.Y, adts[adti].doodads[di].position.Z);

                    GL.Rotate(adts[adti].doodads[di].rotation.Y - 90.0f, 0.0f, 1.0f, 0.0f);
                    GL.Rotate(-adts[adti].doodads[di].rotation.X, 0.0f, 0.0f, 1.0f);
                    GL.Rotate(adts[adti].doodads[di].rotation.Z, 1.0f, 0.0f, 0.0f);

                    var scale = adts[adti].doodads[di].scale / 1024f;
                    GL.Scale(-scale, scale, scale);

                    GL.BindBuffer(BufferTarget.ArrayBuffer, activeDoodadBatch.vertexBuffer);
                    GL.NormalPointer(NormalPointerType.Float, 8 * sizeof(float), (IntPtr)0);
                    GL.TexCoordPointer(2, TexCoordPointerType.Float, 8 * sizeof(float), (IntPtr)(3 * sizeof(float)));
                    GL.VertexPointer(3, VertexPointerType.Float, 8 * sizeof(float), (IntPtr)(5 * sizeof(float)));
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, activeDoodadBatch.indiceBuffer);

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
                    for (int wrb = 0; wrb < adts[adti].worldModelBatches[wb].worldModel.groupBatches.Count(); wrb++)
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
                        for (int si = 0; si < adts[adti].worldModelBatches[wb].worldModel.wmoRenderBatch.Count(); si++)
                        {
                            GL.BindBuffer(BufferTarget.ArrayBuffer, adts[adti].worldModelBatches[wb].worldModel.groupBatches[adts[adti].worldModelBatches[wb].worldModel.wmoRenderBatch[si].groupID].vertexBuffer);
                            GL.NormalPointer(NormalPointerType.Float, 8 * sizeof(float), (IntPtr)0);
                            GL.TexCoordPointer(2, TexCoordPointerType.Float, 8 * sizeof(float), (IntPtr)(3 * sizeof(float)));
                            GL.VertexPointer(3, VertexPointerType.Float, 8 * sizeof(float), (IntPtr)(5 * sizeof(float)));
                            GL.BindBuffer(BufferTarget.ElementArrayBuffer, adts[adti].worldModelBatches[wb].worldModel.groupBatches[adts[adti].worldModelBatches[wb].worldModel.wmoRenderBatch[si].groupID].indiceBuffer);
                            switch (adts[adti].worldModelBatches[wb].worldModel.wmoRenderBatch[si].blendType)
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
                                    throw new Exception("Unknown blend type " + adts[adti].worldModelBatches[wb].worldModel.wmoRenderBatch[si].blendType);
                            }
                            GL.BindTexture(TextureTarget.Texture2D, adts[adti].worldModelBatches[wb].worldModel.wmoRenderBatch[si].materialID);
                            GL.DrawRangeElements(PrimitiveType.Triangles, adts[adti].worldModelBatches[wb].worldModel.wmoRenderBatch[si].firstFace, (adts[adti].worldModelBatches[wb].worldModel.wmoRenderBatch[si].firstFace + adts[adti].worldModelBatches[wb].worldModel.wmoRenderBatch[si].numFaces), (int)adts[adti].worldModelBatches[wb].worldModel.wmoRenderBatch[si].numFaces, DrawElementsType.UnsignedInt, new IntPtr(adts[adti].worldModelBatches[wb].worldModel.wmoRenderBatch[si].firstFace * 4));
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
            System.Windows.Application.Current.Shutdown();
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
            public uint materialID;
            /* WMO ONLY */
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
            public Vector3 position;
            public Vector3 rotation;
            public WorldModel worldModel;
        }

        public struct WorldModel
        {
            public WorldModelGroupBatches[] groupBatches;
            public Material[] mats;
            public RenderBatch[] wmoRenderBatch;
        }

        public struct WorldModelGroupBatches
        {
            public int vertexBuffer;
            public int indiceBuffer;
            public uint[] indices;
        }
    }
}
