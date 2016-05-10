using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using WoWFormatLib.FileReaders;
using ADTexporter.Loaders;
using System.ComponentModel;
using ADTexporter;

namespace ADTexporter
{
    public class TerrainWindow : GameWindow
    {
        private static float dragX;
        private static float dragY;
        private static float dragZ;
        private static float angle;
        private static float MDDepth = 0;
        private static float MDHorizontal = 0;
        private static float MDVertical = 0;
        private static float lightHeight = 0.0f;
        private static float camSpeed = 0.25f;
        private List<Terrain> adts = new List<Terrain>();

        private float rot = 0.0f;
        private float rot2 = 0.0f;
        private static float maxSize = 51200 / 3; //17066,66666666667
	    private static float mapSize = maxSize * 2; //34133,33333333333
	    private static float adtSize = mapSize / 64; //533,3333333333333

        private Dictionary<Key, int> CoolOffKeys = new Dictionary<Key, int>();

        private bool mouseDragging = true;
        private Point mouseOldCoords;

        private CacheStorage cache = new CacheStorage();

        private BackgroundWorker worker;

        private static int terrainShader;
        public static int modelShader;
        private bool jumpToMap = false;

        OldCamera ActiveCamera;

        public TerrainWindow(string modelPath, BackgroundWorker renderWorker)
            : base(1920, 1080, new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8), "Terrain test", GameWindowFlags.Default, DisplayDevice.Default, 3, 3, OpenTK.Graphics.GraphicsContextFlags.Debug)
        {
            if(renderWorker == null)
            {
                renderWorker = new BackgroundWorker();
                renderWorker.WorkerReportsProgress = true;
            }

            worker = renderWorker;

            dragX = 0;
            dragY = 0;
            dragZ = 0;
            angle = 0.0f;

            Keyboard.KeyDown += Keyboard_KeyDown;

            ActiveCamera = new OldCamera(Width, Height);

            terrainShader = ShaderLoader.LoadShader("terrain");
            modelShader = ShaderLoader.LoadShader("model");

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

            if(e.Key == Key.Z)
            {
                rot = rot + 0.5f;   
            }

            if (e.Key == Key.X)
            {
                rot = rot - 0.5f;
            }

            if (e.Key == Key.C)
            {
                rot2 = rot2 + 0.5f;
            }

            if (e.Key == Key.V)
            {
                rot2 = rot2 - 0.5f;
            }

            if(e.Key == Key.B)
            {
                jumpToMap = true;
            }

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

            List<Vertex> verticelist = new List<Vertex>();
            List<Int32> indicelist = new List<Int32>();

            worker.ReportProgress(0, "Loading ADT..");
            for (int x = centerx; x < centerx + distance; x++)
            {
                for (int y = centery; y < centery + distance; y++)
                {
                    string filename = "world\\maps\\" + map + "\\" + map + "_" + x + "_" + y + ".adt";

                    if (!WoWFormatLib.Utils.CASC.FileExists(filename))
                    {
                        continue;
                    }
                    ADTReader reader = new ADTReader();
                    reader.LoadADT(filename);

                    Terrain adt = new Terrain();

                    List<Material> materials = new List<Material>();

                    for (int ti = 0; ti < reader.adtfile.textures.filenames.Count(); ti++)
                    {
                        Material material = new Material();
                        material.filename = reader.adtfile.textures.filenames[ti];
                        material.textureID = BLPLoader.LoadTexture(reader.adtfile.textures.filenames[ti], cache);
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
                                Vertex v = new Vertex();
                                v.Normal = new Vector3(chunk.normals.normal_0[idx], chunk.normals.normal_1[idx], chunk.normals.normal_2[idx]);
                                if (chunk.vertexShading.red != null && chunk.vertexShading.red[idx] != 127)
                                {
                                    v.Color = new Vector3(chunk.vertexShading.blue[idx] / 255.0f, chunk.vertexShading.green[idx] / 255.0f, chunk.vertexShading.red[idx] / 255.0f);
                                }
                                else
                                {
                                    v.Color = new Vector3(1.0f, 1.0f, 1.0f);
                                }

                                v.TexCoord = new Vector2(j + ((i % 2) != 0 ? 0.5f : 0.0f), i * 0.5f);
                                v.TexCoordAlpha = new Vector2(j / 8.0f + ((i % 2) != 0 ? (0.5f / 8.0f) : 0), i / 16.0f);

                                v.Position = new Vector3(chunk.header.position.X - (i * UnitSize * 0.5f), chunk.header.position.Y - (j * UnitSize), chunk.vertices.vertices[idx++] + chunk.header.position.Z);

                                if ((i % 2) != 0) v.Position.Y -= 0.5f * UnitSize;

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

                        if(reader.adtfile.texChunks[c].layers.Count() != 0)
                        {
                            if (!cache.materials.ContainsKey(reader.adtfile.textures.filenames[reader.adtfile.texChunks[c].layers[0].textureId].ToLower()))
                            {
                                throw new Exception("MaterialCache does not have texture " + reader.adtfile.textures.filenames[reader.adtfile.texChunks[c].layers[0].textureId].ToLower());
                            }

                            var layermats = new List<uint>();
                            var alphalayermats = new List<int>();

                            for (int li = 0; li < reader.adtfile.texChunks[c].layers.Count(); li++)
                            {
                                if (reader.adtfile.texChunks[c].alphaLayer != null)
                                {
                                    alphalayermats.Add(BLPLoader.GenerateAlphaTexture(reader.adtfile.texChunks[c].alphaLayer[li].layer));
                                }
                                layermats.Add((uint)cache.materials[reader.adtfile.textures.filenames[reader.adtfile.texChunks[c].layers[li].textureId].ToLower()]);
                            }

                            batch.materialID = layermats.ToArray();
                            batch.alphaMaterialID = alphalayermats.ToArray();
                        }


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

                        if (!cache.doodadBatches.ContainsKey(modelfilename))
                        {
                            M2Loader.LoadM2(modelfilename, cache, modelShader);
                        }
                    }

                    List<WorldModelBatch> worldModelBatches = new List<WorldModelBatch>();

                    for (int wmi = 0; wmi < reader.adtfile.objects.worldModels.entries.Count(); wmi++)
                    {
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

                        Console.WriteLine("Loading WMO #" + wmi + " ("+ wmofilename +")");

                        WorldModelBatch wmobatch = new WorldModelBatch();
                        wmobatch.position = new Vector3(-(wmodelentry.position.X - 17066.666f), wmodelentry.position.Y, -(wmodelentry.position.Z - 17066.666f));
                        wmobatch.rotation = new Vector3(wmodelentry.rotation.X, wmodelentry.rotation.Y, wmodelentry.rotation.Z);
                        wmobatch.worldModel = WMOLoader.LoadWMO(wmofilename, cache, modelShader);
                        worldModelBatches.Add(wmobatch);
                    }

                    GL.BindVertexArray(0);
                    //GL.UseProgram(terrainShader);

                    adt.renderBatches = renderBatches.ToArray();
                    adt.doodads = doodads.ToArray();
                    adt.worldModelBatches = worldModelBatches.ToArray();

                    adt.vao = GL.GenVertexArray();
                    Console.WriteLine("Generated ADT VAO " + adt.vao);
                    GL.BindVertexArray(adt.vao);
                    
                    adt.vertexBuffer = GL.GenBuffer();
                    adt.indiceBuffer = GL.GenBuffer();

                    GL.BindBuffer(BufferTarget.ArrayBuffer, adt.vertexBuffer);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, adt.indiceBuffer);

                    adt.renderBatches = renderBatches.ToArray();

                    int[] indices = indicelist.ToArray();
                    Vertex[] vertices = verticelist.ToArray();

                    Console.WriteLine("Vertices in array: " + vertices.Count()); //37120, correct
                    Console.WriteLine("Indices in array: " + indices.Count()); //196608, should be 65.5k which is 196608 / 3. in triangles so its correct?

                    GL.BindBuffer(BufferTarget.ArrayBuffer, adt.vertexBuffer);
                    GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Count() * 13 * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, adt.indiceBuffer);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(int)), indices, BufferUsageHint.StaticDraw);

                    int verticeBufferSize = 0;
                    int indiceBufferSize = 0;

                    GL.BindBuffer(BufferTarget.ArrayBuffer, adt.vertexBuffer);
                    GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out verticeBufferSize);

                    GL.BindBuffer(BufferTarget.ArrayBuffer, adt.indiceBuffer);
                    GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out indiceBufferSize);

                    Console.WriteLine("Vertices in buffer: " + verticeBufferSize / 13 / sizeof(float));
                    Console.WriteLine("Indices in buffer: " + indiceBufferSize / sizeof(int));

                    GL.BindBuffer(BufferTarget.ArrayBuffer, adt.vertexBuffer);

                   // var normalLoc = GL.GetAttribLocation(shaderProgram, "vNormal");
                   // GL.VertexAttribPointer(normalLoc, 3, VertexAttribPointerType.Float, false, 11 * sizeof(float), 0);

                    var colorLoc = GL.GetAttribLocation(terrainShader, "vColor");
                    if(colorLoc != -1) GL.VertexAttribPointer(colorLoc, 3, VertexAttribPointerType.Float, false, 13 * sizeof(float), 3 * sizeof(float));

                    var texCoordLoc = GL.GetAttribLocation(terrainShader, "vTexCoord");
                    GL.EnableVertexAttribArray(texCoordLoc);
                    GL.VertexAttribPointer(texCoordLoc, 2, VertexAttribPointerType.Float, false, 13 * sizeof(float), 6 * sizeof(float));

                    var texCoordAlphaLoc = GL.GetAttribLocation(terrainShader, "vTexCoordAlpha");
                    GL.EnableVertexAttribArray(texCoordAlphaLoc);
                    GL.VertexAttribPointer(texCoordAlphaLoc, 2, VertexAttribPointerType.Float, false, 13 * sizeof(float), 8 * sizeof(float));

                    var posLoc = GL.GetAttribLocation(terrainShader, "vPosition");
                    GL.EnableVertexAttribArray(posLoc);
                    GL.VertexAttribPointer(posLoc, 3, VertexAttribPointerType.Float, false, 13 * sizeof(float), 10 * sizeof(float));

                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, adt.indiceBuffer);

                    GL.BindVertexArray(0);

                    adts.Add(adt);                       
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
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

            MDVertical = 0;
            MDDepth = 0;
            MDHorizontal = 0;

            if (keyboardState.IsKeyDown(Key.I))
            {
                Console.WriteLine("Camera position: " + ActiveCamera.Pos);
                Console.WriteLine("Camera direction: " + ActiveCamera.Dir);
            }

            if (keyboardState.IsKeyDown(Key.Q))
            {
                MDVertical = 1;
            }

            if (keyboardState.IsKeyDown(Key.E))
            {
                MDVertical = -1;
            }

            if (keyboardState.IsKeyDown(Key.W))
            {
                MDDepth = 1;
            }

            if (keyboardState.IsKeyDown(Key.S))
            {
                MDDepth = -1;
            }

            if (keyboardState.IsKeyDown(Key.A))
            {
                MDHorizontal = -1;
            }

            if (keyboardState.IsKeyDown(Key.D))
            {
                MDHorizontal =  1;
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
                    dragY = dragY - camSpeed;
                }

                if (keyboardState.IsKeyDown(Key.Down))
                {
                    dragY = dragY + camSpeed;
                }

                if (keyboardState.IsKeyDown(Key.Left))
                {
                    dragX = dragX - camSpeed;
                }

                if (keyboardState.IsKeyDown(Key.Right))
                {
                    dragX = dragX + camSpeed;
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

                dragY = dragY + mouseMovementY / 20.0f;
                dragX = dragX + mouseMovementX / 20.0f;

                if (dragY < -89) {
                    dragY = -89;
                } else if (dragY > 89) {
                    dragY = 89;
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

            dragZ = (mouseState.WheelPrecise / 2) - 500; //Startzoom is at -7.5f 
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            angle = angle % 360;

            ActiveCamera.tick((float)e.Time, dragX, dragY, MDHorizontal, MDDepth, MDVertical);

            //ActiveCamera.Pos = new Vector3(-161.3381f, -367.0795f, 291.3396f);
            //ActiveCamera.Dir = new Vector3(-0.4147807f, 0.5772285f, -0.7033948f);

            ActiveCamera.setupGLRenderMatrix();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(terrainShader);

            var transformLoc = GL.GetUniformLocation(terrainShader, "projection");
            GL.UniformMatrix4(transformLoc, false, ref ActiveCamera.projectionMatrix);

            var modelviewLoc = GL.GetUniformLocation(terrainShader, "modelview");
            GL.UniformMatrix4(modelviewLoc, false, ref ActiveCamera.cameraViewMatrix);

            for (int adti = 0; adti < adts.Count(); adti++)
            {
                GL.BindVertexArray(adts[adti].vao);
                
                for (int rb = 0; rb < adts[adti].renderBatches.Count(); rb++)
                {
                   if (adts[adti].renderBatches[rb].materialID == null) continue;
                   for (int li = 0; li < adts[adti].renderBatches[rb].materialID.Count(); li++)
                   {
                        var layerLoc = GL.GetUniformLocation(terrainShader, "layer" + li);

                        if(layerLoc == -1) { continue; }

                        GL.ActiveTexture(TextureUnit.Texture0 + li);
                        GL.BindTexture(TextureTarget.Texture2D, (int)adts[adti].renderBatches[rb].materialID[li]);

                        GL.Uniform1(layerLoc, li);

                        if(li > 0)
                        {
                            var blendLoc = GL.GetUniformLocation(terrainShader, "blendLayer" + li);
                            if (blendLoc == -1) { continue; }
                            GL.ActiveTexture(TextureUnit.Texture0 + adts[adti].renderBatches[rb].materialID.Count() + li + 1);
                            GL.BindTexture(TextureTarget.Texture2D, adts[adti].renderBatches[rb].alphaMaterialID[li]);
                            GL.Uniform1(blendLoc, adts[adti].renderBatches[rb].materialID.Count() + li + 1);
                        }
                    }

                    GL.DrawRangeElements(PrimitiveType.Triangles, (int)adts[adti].renderBatches[rb].firstFace, (int)adts[adti].renderBatches[rb].firstFace + (int)adts[adti].renderBatches[rb].numFaces, (int)adts[adti].renderBatches[rb].numFaces, DrawElementsType.UnsignedInt, new IntPtr(adts[adti].renderBatches[rb].firstFace * 4));
                }

                GL.UseProgram(modelShader);

                transformLoc = GL.GetUniformLocation(modelShader, "projection");
                GL.UniformMatrix4(transformLoc, false, ref ActiveCamera.projectionMatrix);

                modelviewLoc = GL.GetUniformLocation(modelShader, "modelview");
                GL.UniformMatrix4(modelviewLoc, false, ref ActiveCamera.cameraViewMatrix);

                var positionLoc = GL.GetUniformLocation(modelShader, "translation");
                var rotationLoc = GL.GetUniformLocation(modelShader, "rotation");
                var worldRotationLoc = GL.GetUniformLocation(modelShader, "worldRotation");

                for (int wb = 0; wb < adts[adti].worldModelBatches.Count(); wb++)
                {
                    for (int wrb = 0; wrb < adts[adti].worldModelBatches[wb].worldModel.groupBatches.Count(); wrb++)
                    {
                        var rotationMatrixX = Matrix4.CreateRotationX(0.0f);
                        var rotationMatrixY = Matrix4.CreateRotationY(0.0f);
                        var rotationMatrixZ = Matrix4.CreateRotationZ(adts[adti].worldModelBatches[wb].rotation.Y - rot);

                        var rotationMatrix = rotationMatrixZ * rotationMatrixX * rotationMatrixY;

                        GL.UniformMatrix4(rotationLoc, false, ref rotationMatrix);
                        
                         var translationMatrix = Matrix4.CreateTranslation(adts[adti].worldModelBatches[wb].position.Z, adts[adti].worldModelBatches[wb].position.X, adts[adti].worldModelBatches[wb].position.Y);

                         GL.UniformMatrix4(positionLoc, false, ref translationMatrix);

                        //var worldRotationMatrixY = Matrix4.CreateRotationX(adts[adti].worldModelBatches[wb].rotation.Y - 270.0f);
                        //var worldRotationMatrixX = Matrix4.CreateRotationY(-adts[adti].worldModelBatches[wb].rotation.X);
                        //var worldRotationMatrixZ = Matrix4.CreateRotationZ(adts[adti].worldModelBatches[wb].rotation.Z - 90.0f);
      
                        var worldRotationMatrixX = Matrix4.CreateRotationX(0.0f);
                        var worldRotationMatrixY = Matrix4.CreateRotationY(0.0f);
                        var worldRotationMatrixZ = Matrix4.CreateRotationZ(rot2);

                        var worldRotationMatrix = worldRotationMatrixZ * worldRotationMatrixY * worldRotationMatrixX;
                        GL.UniformMatrix4(worldRotationLoc, false, ref worldRotationMatrix);
                         
                         
                         /*
                        GL.Rotate(90.0f, 1.0f, 0.0f, 0.0f);
                        GL.Rotate(90.0f, 0.0f, 1.0f, 0.0f);

                        GL.Translate(adts[adti].worldModelBatches[wb].position.X, adts[adti].worldModelBatches[wb].position.Y, adts[adti].worldModelBatches[wb].position.Z);
                        GL.Rotate(adts[adti].worldModelBatches[wb].rotation.Y - 270.0f, 0.0f, 1.0f, 0.0f);
                        GL.Rotate(-adts[adti].worldModelBatches[wb].rotation.X, 0.0f, 0.0f, 1.0f);
                        GL.Rotate(adts[adti].worldModelBatches[wb].rotation.Z - 90.0f, 1.0f, 0.0f, 0.0f);
                        */

                        for (int si = 0; si < adts[adti].worldModelBatches[wb].worldModel.wmoRenderBatch.Count(); si++)
                        {
                            var vao = adts[adti].worldModelBatches[wb].worldModel.groupBatches[adts[adti].worldModelBatches[wb].worldModel.wmoRenderBatch[si].groupID].vao;

                            if (vao == 0) continue;

                            GL.BindVertexArray(vao);

                            //Render opaque first (temp comment out)
                            //if (adts[adti].worldModelBatches[wb].worldModel.wmoRenderBatch[si].blendType != 0) { continue; }
                            GL.ActiveTexture(TextureUnit.Texture0);
                            GL.BindTexture(TextureTarget.Texture2D, adts[adti].worldModelBatches[wb].worldModel.wmoRenderBatch[si].materialID[0]);

                            GL.DrawRangeElements(PrimitiveType.Triangles, adts[adti].worldModelBatches[wb].worldModel.wmoRenderBatch[si].firstFace, (adts[adti].worldModelBatches[wb].worldModel.wmoRenderBatch[si].firstFace + adts[adti].worldModelBatches[wb].worldModel.wmoRenderBatch[si].numFaces), (int)adts[adti].worldModelBatches[wb].worldModel.wmoRenderBatch[si].numFaces, DrawElementsType.UnsignedInt, new IntPtr(adts[adti].worldModelBatches[wb].worldModel.wmoRenderBatch[si].firstFace * 4));
                        }
                    }
                }

                for (int di = 0; di < adts[adti].doodads.Count(); di++)
                {
                    var activeDoodadBatch = cache.doodadBatches[adts[adti].doodads[di].filename];

                    var rotationMatrixX = Matrix4.CreateRotationX(0.0f);
                    var rotationMatrixY = Matrix4.CreateRotationY(0.0f);
                    var rotationMatrixZ = Matrix4.CreateRotationZ(adts[adti].doodads[di].rotation.Y - rot);

                    var rotationMatrix = rotationMatrixZ * rotationMatrixX * rotationMatrixY;

                    GL.UniformMatrix4(rotationLoc, false, ref rotationMatrix);

                    var translationMatrix = Matrix4.CreateTranslation(adts[adti].doodads[di].position.Z, adts[adti].doodads[di].position.X, adts[adti].doodads[di].position.Y);

                    GL.UniformMatrix4(positionLoc, false, ref translationMatrix);

                    //var worldRotationMatrixY = Matrix4.CreateRotationX(adts[adti].worldModelBatches[wb].rotation.Y - 270.0f);
                    //var worldRotationMatrixX = Matrix4.CreateRotationY(-adts[adti].worldModelBatches[wb].rotation.X);
                    //var worldRotationMatrixZ = Matrix4.CreateRotationZ(adts[adti].worldModelBatches[wb].rotation.Z - 90.0f);

                    var worldRotationMatrixX = Matrix4.CreateRotationX(0.0f);
                    var worldRotationMatrixY = Matrix4.CreateRotationY(0.0f);
                    var worldRotationMatrixZ = Matrix4.CreateRotationZ(rot2);

                    var worldRotationMatrix = worldRotationMatrixZ * worldRotationMatrixY * worldRotationMatrixX;
                    GL.UniformMatrix4(worldRotationLoc, false, ref worldRotationMatrix);

                    //TODO SCALE
                    var scale = adts[adti].doodads[di].scale / 1024f;
                    //GL.Scale(scale, scale, scale);

                    for (int si = 0; si < activeDoodadBatch.submeshes.Count(); si++)
                    {
                        var vao = activeDoodadBatch.vao;

                        if (vao == 0) continue;

                        GL.BindVertexArray(vao);

                        GL.ActiveTexture(TextureUnit.Texture0);
                        GL.BindTexture(TextureTarget.Texture2D, activeDoodadBatch.submeshes[si].material);
                        GL.DrawRangeElements(PrimitiveType.Triangles, activeDoodadBatch.submeshes[si].firstFace, (activeDoodadBatch.submeshes[si].firstFace + activeDoodadBatch.submeshes[si].numFaces), (int)activeDoodadBatch.submeshes[si].numFaces, DrawElementsType.UnsignedInt, new IntPtr(activeDoodadBatch.submeshes[si].firstFace * 4));
                    }

                }

                GL.UseProgram(terrainShader);
            }

            var error = GL.GetError().ToString();
            if (error != "NoError")
            {
                Console.WriteLine(error);
            }

            SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            Dispose();
            base.OnUnload(e);
        }
    }
}
