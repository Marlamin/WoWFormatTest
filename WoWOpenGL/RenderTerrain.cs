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
    public class RenderTerrain : GameWindow
    {
        private static float angle = 0.0f;
        private GLControl glControl;
        private bool gLoaded = false;
        private bool modelLoaded;


        public RenderTerrain()
        {
        }

        public RenderTerrain(string ModelPath)
        {
            Console.WriteLine(ModelPath);

            string[] adt = ModelPath.Split('_');

            Console.WriteLine("MAP {0}, X {1}, Y {2}", adt[0], adt[1], adt[2]);
            LoadADT(adt[0], adt[1], adt[2]);

            modelLoaded = true;

            System.Windows.Forms.Integration.WindowsFormsHost wfc = RenderWindow.winFormControl;
            glControl = new GLControl(OpenTK.Graphics.GraphicsMode.Default, 3, 0, OpenTK.Graphics.GraphicsContextFlags.Default);

            glControl.Width = 800;
            glControl.Height = 600;
            glControl.Left = 0;
            glControl.Top = 0;

            glControl.Load += glControl_Load;
            glControl.Paint += RenderFrame;
            glControl.Resize += glControl_Resize;
            glControl_Resize(glControl, EventArgs.Empty);
            glControl.MakeCurrent();

            wfc.Child = glControl;

            Console.WriteLine(glControl.Width + "x" + glControl.Height);
        }

        private void glControl_Load(object sender, EventArgs e)
        {
            glControl.MakeCurrent();
            GL.Enable(EnableCap.Texture2D);
            GL.ClearColor(OpenTK.Graphics.Color4.SkyBlue);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            gLoaded = true;
            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            float aspect_ratio = glControl.Width / glControl.Height;
            Matrix4 perpective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect_ratio, 1, 128);
            GL.Ortho(0, glControl.Width, 0, glControl.Height, -1, 1);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref perpective);
        }

        private void glControl_Resize(object sender, EventArgs e)
        {
        }

        private void LoadADT(string map, string x, string y)
        {
            ADTReader reader = new ADTReader();
            reader.LoadADT("World/Maps/" + map + "/" + map + "_" + x + "_" + y + ".adt");
            Console.WriteLine(reader.adtfile.version);
        }

        private void RenderFrame(object sender, EventArgs e) //This is called every frame
        {
            glControl.MakeCurrent();
            //Do stuff!
            glControl.Invalidate();
        }
    }
}