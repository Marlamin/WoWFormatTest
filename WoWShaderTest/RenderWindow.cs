using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Drawing;
using WoWFormatLib;
using WoWFormatLib.FileReaders;

namespace WoWShaderTest
{
    class RenderWindow : GameWindow
    {
        private int vertexBuffer;
        private int elementBuffer;

        private int vertexAttribObject;

        private int vertexShader;
        private int fragmentShader;
        private int shaderProgram;

        OldCamera ActiveCamera;
        private float dragX;
        private float dragY;
        private float dragZ;
        private float angle;

        private float camSpeed;

        private float lightHeight;

        private bool mouseDragging = true;

        private Dictionary<Key, int> CoolOffKeys = new Dictionary<Key, int>();

        private Point mouseOldCoords;

        public RenderWindow() : base(800, 600, new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8), "Shader test", GameWindowFlags.Default, DisplayDevice.Default, 3, 0, OpenTK.Graphics.GraphicsContextFlags.Debug)
        {
            Keyboard.KeyDown += Keyboard_KeyDown;
        }

        protected override void OnLoad(EventArgs e)
        {
            // Get OpenGL version
            Console.WriteLine("OpenGL version: " + GL.GetString(StringName.Version));
            Console.WriteLine("OpenGL vendor: " + GL.GetString(StringName.Vendor));

            // Set up camera
            ActiveCamera = new OldCamera(Width, Height);
            ActiveCamera.Pos = new Vector3(0, 0, 0);

            // Vertex Attribute Object
            vertexAttribObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexAttribObject);

            // Vertices
            float[] vertices = new float[] {
                    -0.5f,  0.5f, 0.0f, 0.0f, 0.0f, // Top-left
				     0.5f,  0.5f, 1.0f, 1.0f, 0.0f, // Top-right
				     0.5f, -0.5f, 0.0f, 1.0f, 1.0f, // Bottom-right
				    -0.5f, -0.5f, 0.0f, 0.0f, 1.0f  // Bottom-left
            };

            vertexBuffer = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);

            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) (vertices.Count() * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            int verticeBufferSize = 0;

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out verticeBufferSize);

            Console.WriteLine("Vertices in buffer: " + verticeBufferSize / 5 / sizeof(float));

            // Elements
            int[] elements = new int[] {
                0, 1, 2,
                2, 3, 0
            };

            elementBuffer = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(elements.Count() * sizeof(int)), elements, BufferUsageHint.StaticDraw);

            // Vertex shader
            vertexShader = GL.CreateShader(ShaderType.VertexShader);

            string vertexSource = File.ReadAllText("Shaders/vertex.shader");
            Console.WriteLine(vertexSource);
            GL.ShaderSource(vertexShader, vertexSource);

            GL.CompileShader(vertexShader);

            int vertexShaderStatus;
            GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out vertexShaderStatus);
            Console.WriteLine("[VERTEX] Shader compile status: " + vertexShaderStatus);

            string vertexShaderLog;
            GL.GetShaderInfoLog(vertexShader, out vertexShaderLog);
            Console.Write(vertexShaderLog);

            // Fragment shader
            fragmentShader = GL.CreateShader(ShaderType.FragmentShader);

            string fragmentSource = File.ReadAllText("Shaders/fragment.shader");
            Console.WriteLine(fragmentSource);
            GL.ShaderSource(fragmentShader, fragmentSource);

            GL.CompileShader(fragmentShader);

            int fragmentShaderStatus;
            GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out fragmentShaderStatus);
            Console.WriteLine("[FRAGMENT] Shader compile status: " + fragmentShaderStatus);

            string fragmentShaderLog;
            GL.GetShaderInfoLog(fragmentShader, out fragmentShaderLog);
            Console.Write(fragmentShaderLog);

            // Shader program
            shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);

            GL.BindFragDataLocation(shaderProgram, 0, "outColor");

            GL.LinkProgram(shaderProgram);
            string programInfoLog = GL.GetProgramInfoLog(shaderProgram);
            Console.Write(programInfoLog);

            int programStatus;
            GL.GetProgram(shaderProgram, GetProgramParameterName.LinkStatus, out programStatus);
            Console.WriteLine("[FRAGMENT] Program link status: " + programStatus);
            GL.UseProgram(shaderProgram);

            // Set up matrix
            ActiveCamera.setupGLRenderMatrix(shaderProgram);

            // Shader settings
            int posAttrib = GL.GetAttribLocation(shaderProgram, "position");
            GL.EnableVertexAttribArray(posAttrib);
            GL.VertexAttribPointer(posAttrib, 3, VertexAttribPointerType.Float, false, sizeof(float) * 5, 0);

            int texCoordAttrib = GL.GetAttribLocation(shaderProgram, "texCoord");
            GL.EnableVertexAttribArray(texCoordAttrib);
            GL.VertexAttribPointer(texCoordAttrib, 2, VertexAttribPointerType.Float, false, sizeof(float) * 5, sizeof(float) * 3);

            // Clear 
            GL.ClearColor(Color.Black);

            // Uniforms
            int uniColor = GL.GetUniformLocation(shaderProgram, "triangleColor");
            Vector3 uniCol3 = new Vector3(1.0f, 0.0f, 0.0f);
            GL.Uniform3(uniColor, uniCol3);

            // Textures
            int[] textureIds = new int[2];
            GL.GenTextures(2, textureIds);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureIds[0]);

            var blp = new BLPReader();

            blp.LoadBLP(File.OpenRead(@"Z:\WoW extracts\20363_full\Textures\ShaneCube.blp"));

            System.Drawing.Imaging.BitmapData bmp_data = blp.bmp.LockBits(new System.Drawing.Rectangle(0, 0, blp.bmp.Width, blp.bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            blp.bmp.UnlockBits(bmp_data);

            GL.Uniform1(GL.GetUniformLocation(shaderProgram, "tex"), 0);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, textureIds[1]);

        }


        private void DrawAxes()
        {
            GL.Begin(PrimitiveType.Lines);

            GL.Vertex2(-10, 0);
            GL.Vertex2(10, 0);

            GL.Vertex2(0, -10);
            GL.Vertex2(0, 10);

            GL.Vertex2(0, 0);
            GL.Vertex2(0, 0);

            GL.End();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
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

            if (!CoolOffKeys.ContainsKey(Key.Right) && keyboardState.IsKeyDown(Key.Right) && keyboardState.IsKeyUp(Key.ShiftLeft))
            {
                angle += 90;
                CoolOffKey(Key.Right);
            }

            if (!CoolOffKeys.ContainsKey(Key.Left) && keyboardState.IsKeyDown(Key.Left) && keyboardState.IsKeyUp(Key.ShiftLeft))
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
            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            angle = angle % 360;

            ActiveCamera.Pos = new Vector3(dragX, dragY, dragZ);

            ActiveCamera.setupGLRenderMatrix(shaderProgram);

            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
            //DrawAxes();

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
        }

        void Keyboard_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Exit();
        }
    }
}
