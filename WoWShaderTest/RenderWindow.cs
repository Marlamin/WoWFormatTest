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
                    -0.5f,  0.5f, 1.0f, 0.0f, 0.0f, // Top-left
                    0.5f,  0.5f, 0.0f, 1.0f, 0.0f, // Top-right
                    0.5f, -0.5f, 0.0f, 0.0f, 1.0f, // Bottom-right
                    -0.5f, -0.5f, 1.0f, 1.0f, 1.0f  // Bottom-left
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
            GL.VertexAttribPointer(posAttrib, 2, VertexAttribPointerType.Float, false, sizeof(float) * 5, 0);
            GL.EnableVertexAttribArray(posAttrib);

            int colorAttrib = GL.GetAttribLocation(shaderProgram, "color");
            GL.VertexAttribPointer(colorAttrib, 3, VertexAttribPointerType.Float, false, sizeof(float) * 5, sizeof(float) * 2);
            GL.EnableVertexAttribArray(colorAttrib);

            // Clear 
            GL.ClearColor(Color.Black);

            // Uniforms
            int uniColor = GL.GetUniformLocation(shaderProgram, "triangleColor");
            Vector3 uniCol3 = new Vector3(1.0f, 0.0f, 0.0f);
            GL.Uniform3(uniColor, uniCol3);
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
