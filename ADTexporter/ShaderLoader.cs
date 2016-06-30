using OpenTK.Graphics.OpenGL;
using System;
using System.IO;

namespace ADTexporter
{
    class ShaderLoader
    {
        public static int LoadShader(string name)
        {
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            var vertexSource = File.ReadAllText("Shaders/" + name + ".vert");

            GL.ShaderSource(vertexShader, vertexSource);

            GL.CompileShader(vertexShader);

            int shaderStatus;

            GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out shaderStatus);

            if (shaderStatus != 1)
            {
                string vsInfo;
                GL.GetShaderInfoLog(vertexShader, out vsInfo);
                throw new Exception("Error setting up Vertex Shader: " + vsInfo);
            }

            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);

            var fragmentSource = File.ReadAllText("Shaders/" + name + ".frag");

            GL.ShaderSource(fragmentShader, fragmentSource);

            GL.CompileShader(fragmentShader);

            GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out shaderStatus);

            if (shaderStatus != 1)
            {
                string fsInfo;
                GL.GetShaderInfoLog(fragmentShader, out fsInfo);
                throw new Exception("Error setting up Fragment Shader: " + fsInfo);
            }

            var shaderProgram = GL.CreateProgram();

            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);

            GL.LinkProgram(shaderProgram);

            var programInfoLog = GL.GetProgramInfoLog(shaderProgram);
            Console.Write(programInfoLog);

            GL.ValidateProgram(shaderProgram);

            int linkStatus;
            GL.GetProgram(shaderProgram, GetProgramParameterName.LinkStatus, out linkStatus);

            if (linkStatus != 1)
            {
                string linkInfo;
                GL.GetProgramInfoLog(shaderProgram, out linkInfo);
                throw new Exception("Error linking shaders: " + linkInfo);
            }

            GL.DetachShader(shaderProgram, vertexShader);
            GL.DeleteShader(vertexShader);

            GL.DetachShader(shaderProgram, fragmentShader);
            GL.DeleteShader(fragmentShader);

            return shaderProgram;
        }
    }
}
