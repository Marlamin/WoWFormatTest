using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWOpenGL
{
    class Camera
    {
        int Width, Height; // window viewport size
        Matrix4 projectionMatrix;

        public Vector3 Pos = new Vector3(0, 0, -1);
        public Vector3 Dir = new Vector3(0, 0, 1);
        public Vector3 Up = Vector3.UnitY;

        public Camera(int viewportWidth, int viewportHeight)
        {
            viewportSize(viewportWidth, viewportHeight);
        }

        public void viewportSize(int viewportWidth, int viewportHeight)
        {
            this.Width = viewportWidth;
            this.Height = viewportHeight;
            float aspectRatio = Width / (float)Height;
            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 1.0f, 64.0f);

        }

        public void setupGLRenderMatrix()
        {
            // setup projection
            GL.Viewport(0, 0, Width, Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projectionMatrix);

            // create and setup camera view matrix
            Matrix4 cameraViewMatrix = Matrix4.LookAt(Pos, Pos + Dir, Up);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref cameraViewMatrix);
        }

        public void Dolly(float distance)
        {
            Pos += distance * Dir;
        }
        public void PanWorldXY(float x, float y)
        {
            Pos += new Vector3(x, y, 0);
        }
        public void PanCameraXY(float x, float y)
        {
            // in order to pan in camera-xy, we need to transform our (x,y) mouse pan vector into camera-space
            Vector3 mousePanVector = new Vector3(-x, y, 0);
            // start by creating our camera matrix from our camera vectors
            Matrix4 cameraMatrix = Matrix4.LookAt(Pos, Pos + Dir, Up);
            // then extract only the camera ROTATION, and turn that into a rotation matrix..
            Matrix4 cameraRotationMatrix = Matrix4.CreateFromQuaternion(cameraMatrix.ExtractRotation());

            // now transform our pan-vector into camera space by using the inverse rotation matrix
            Pos += Vector3.Transform(mousePanVector, cameraRotationMatrix.Inverted());
        }

        public void OrbitXY(float x, float y)
        {
            // extract our "orbit target point"
            Vector3 orbitTarget = Pos + (Dir * 10.0f);

            Quaternion camera_Rotation = Matrix4.LookAt(Pos, Pos + Dir, Up).ExtractRotation();

            Quaternion yaw_Rotation = Quaternion.FromAxisAngle(Vector3.UnitY, DegreeToRadian(-x));
            Quaternion pitch_Rotation = Quaternion.FromAxisAngle(Vector3.UnitX, DegreeToRadian(y));

            Quaternion qResult = yaw_Rotation * pitch_Rotation;

            Matrix4 newOrientation = Matrix4.CreateFromQuaternion(qResult);

            // recalculate our new orientation
            Dir = Vector3.Transform(Dir, newOrientation);
            Up = Vector3.Transform(Up, newOrientation);
            // recalulate our new position
            Pos = orbitTarget - (Dir * 10.0f);

        }

        private float DegreeToRadian(float angleInDegrees)
        {
            return (float)Math.PI * angleInDegrees / 180.0f;
        }


    }
}
