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

        public RenderWindow() : base(800, 600, new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8), "Shader test", GameWindowFlags.Default, DisplayDevice.Default, 3, 0, OpenTK.Graphics.GraphicsContextFlags.Default)
        {
            Keyboard.KeyDown += Keyboard_KeyDown;
        }

        protected override void OnLoad(EventArgs e)
        {
            // Get OpenGL version
            Console.WriteLine("OpenGL version: " + GL.GetString(StringName.Version));
            Console.WriteLine("OpenGL vendor: " + GL.GetString(StringName.Vendor));

            // Vertex Attribute Object
            vertexAttribObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexAttribObject);

            // Vertices
            float[] vertices = new float[] {
                    -0.5f,  0.5f, 0.0f, 0.0f, // Top-left
				     0.5f,  0.5f, 1.0f, 0.0f, // Top-right
				     0.5f, -0.5f, 1.0f, 1.0f, // Bottom-right
				    -0.5f, -0.5f, 0.0f, 1.0f  // Bottom-left
            };

            vertexBuffer = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);

            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) (vertices.Count() * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            int verticeBufferSize = 0;

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out verticeBufferSize);

            Console.WriteLine("Vertices in buffer: " + verticeBufferSize / 2 / sizeof(float));

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

            // Shader settings
            int posAttrib = GL.GetAttribLocation(shaderProgram, "position");
            GL.EnableVertexAttribArray(posAttrib);
            GL.VertexAttribPointer(posAttrib, 2, VertexAttribPointerType.Float, false, sizeof(float) * 4, 0);

            int texCoordAttrib = GL.GetAttribLocation(shaderProgram, "texCoord");
            GL.EnableVertexAttribArray(texCoordAttrib);
            GL.VertexAttribPointer(texCoordAttrib, 2, VertexAttribPointerType.Float, false, sizeof(float) * 4, sizeof(float) * 2);

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

            var blp2 = new BLPReader();

            blp2.LoadBLP(File.OpenRead(@"Z:\WoW extracts\20363_full\Textures\ShaneCube_new.blp"));

            System.Drawing.Imaging.BitmapData bmp_data2 = blp2.bmp.LockBits(new System.Drawing.Rectangle(0, 0, blp2.bmp.Width, blp2.bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data2.Width, bmp_data2.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data2.Scan0);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            blp2.bmp.UnlockBits(bmp_data2);

            GL.Uniform1(GL.GetUniformLocation(shaderProgram, "shaneCubeNew"), 1);
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
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
            //DrawAxes();

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
        }

        void Keyboard_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Exit();
        }
    }
}
