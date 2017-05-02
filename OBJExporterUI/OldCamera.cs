using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace OBJExporterUI
{
    class OldCamera
    {
        int Width, Height; // window viewport size
        Matrix4 projectionMatrix;

        public Vector3 Pos = new Vector3(0, 0, 0);
        public Vector3 Dir = new Vector3(1, 0, 0);
        public Vector3 Up = Vector3.UnitZ;

        public OldCamera(int viewportWidth, int viewportHeight)
        {
            viewportSize(viewportWidth, viewportHeight);
        }

        public void viewportSize(int viewportWidth, int viewportHeight)
        {
            this.Width = viewportWidth;
            this.Height = viewportHeight;
            float aspectRatio = Width / (float)Height;
            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 1.0f, 4096.0f);
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

        public void tick(float timeDelta, float dragX, float dragY, float MDHorizontal, float MDDepth, float MDVertical) {
            Vector3 dir = new Vector3(1, 0, 0);
            float moveSpeed = 60f;

            float dTime = timeDelta;

            /* Calc look at position */
            
            Matrix3 rotationY = Matrix3.CreateRotationY(DegreeToRadian(dragY));
            Vector3.Transform(ref dir, ref rotationY, out dir);
            Matrix3 rotationZ = Matrix3.CreateRotationZ(DegreeToRadian(-dragX));
            Vector3.Transform(ref dir, ref rotationZ, out dir);

            /* Calc camera position */
            if (MDHorizontal != 0.0f) {
                Vector3 right;
                rotationZ = Matrix3.CreateRotationZ(DegreeToRadian(-90));
                Vector3.Transform(ref dir, ref rotationZ, out right);
                right.Z = 0.0f;
                right.Normalize();
                Vector3.Multiply(ref right, dTime * moveSpeed * MDHorizontal, out right);
                
                Pos = Pos + right;
            }

            if (MDDepth != 0.0) {
                Vector3 movDir = new Vector3(dir);
                Vector3.Multiply(ref movDir, dTime * moveSpeed * MDDepth, out movDir);
                
                Pos = Pos + movDir;
            }
            if (MDVertical != 0.0f) {
                Pos.Z = Pos.Z + dTime * moveSpeed * MDVertical;
            }

            Dir = dir;
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
            Matrix3 cameraRotationMatrix = Matrix3.CreateFromQuaternion(cameraMatrix.ExtractRotation());

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

            Matrix3 newOrientation = Matrix3.CreateFromQuaternion(qResult);

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
