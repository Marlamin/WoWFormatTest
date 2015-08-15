using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWShaderTest
{
    class OldCamera
    {
        int Width, Height; // window viewport size
        Matrix4 projectionMatrix;

        private Vector3 Pos;
        private Vector3 Target;
        private Vector3 Up;

        private float angleH;
        private float angleV;

        private bool onUpperEdge;
        private bool onLowerEdge;
        private bool onLeftEdge;
        private bool onRightEdge;

        private Vector2 mousePos;

        private float stepSize = 1.0f;

        public OldCamera(int viewportWidth, int viewportHeight, Vector3 pos, Vector3 target, Vector3 up)
        {
            viewportSize(viewportWidth, viewportHeight);

            Pos = pos;

            Target = target;
            Target.Normalize();

            Up = up;
            Up.Normalize();

            Vector3 hTarget = new Vector3(Target.X, 0.0f, Target.Z);
            hTarget.Normalize();

            if (hTarget.Z >= 0.0f)
            {
                if (hTarget.X >= 0.0f)
                {
                    angleH = 360.0f - (float) RadiansToDegrees(Math.Asin(hTarget.Z));
                }
                else
                {
                    angleH = 180.0f + (float) RadiansToDegrees(Math.Asin(hTarget.Z));
                }
            }
            else
            {
                if (hTarget.X >= 0.0f)
                {
                    angleH = (float) RadiansToDegrees(Math.Asin(-hTarget.Z));
                }
                else
                {
                    angleH = 90.0f + (float) RadiansToDegrees(Math.Asin(-hTarget.Z));
                }
            }

            angleV = -(float)RadiansToDegrees(Math.Asin(Target.Y));

            onUpperEdge = false;
            onLowerEdge = false;
            onLeftEdge = false;
            onRightEdge = false;

            mousePos.X = Width / 2;
            mousePos.Y = Height / 2;

            Mouse.SetPosition(mousePos.X, mousePos.Y);
        }

        public void processKeyboardInput(KeyboardState state)
        {
            if (state.IsKeyDown(Key.W))
            {
                Pos += (Target * stepSize);
            }

            if (state.IsKeyDown(Key.S))
            {
                Pos -= (Target * stepSize);
            }

            if (state.IsKeyDown(Key.A))
            {
                Vector3 Left = Vector3.Cross(Target, Up);
                Left.Normalize();
                Left *= stepSize;
                Pos += Left;
            }

            if (state.IsKeyDown(Key.D))
            {
                Vector3 Right = Vector3.Cross(Up, Target);
                Right.Normalize();
                Right *= stepSize;
                Pos += Right;
            }


            /* Mouse movement code temporarily mapped to arrow keys */
            bool shouldUpdate = false;

            if (state.IsKeyDown(Key.Left))
            {
                angleH -= 0.1f;
                shouldUpdate = true;
            }
            else if (state.IsKeyDown(Key.Right))
            {
                angleH += 0.1f;
                shouldUpdate = true;
            }

            if (state.IsKeyDown(Key.Up))
            {
                if (angleV > -90.0f)
                {
                    angleV -= 0.1f;
                    shouldUpdate = true;
                }
            }
            else if (state.IsKeyDown(Key.Down))
            {
                if (angleV < 90.0f)
                {
                    angleV += 0.1f;
                    shouldUpdate = true;
                }
            }

            if (shouldUpdate)
            {
                update();
            }
        }

        public void processMouseInput(int X, int Y)
        {
            int deltaX = X - (int) mousePos.X;
            int deltaY = Y - (int) mousePos.Y;

            float margin = 1.0f;

            mousePos.X = (float)X;
            mousePos.Y = (float)Y;

            angleH += deltaX / 2000.0f;
            angleV += deltaY / 2000.0f;

            Console.WriteLine(angleH);
            Console.WriteLine(angleV);
            if (deltaX == 0)
            {
                if (X <= margin)
                {
                    onLeftEdge = true;
                }
                else if (X >= (Width - margin))
                {
                    onRightEdge = true;
                }
            }
            else
            {
                onLeftEdge = false;
                onRightEdge = false;
            }

            if (deltaY == 0)
            {
                if (Y <= margin)
                {
                    onUpperEdge = true;
                }
                else if (Y >= (Height - margin))
                {
                    onLowerEdge = true;
                }
            }
            else
            {
                onUpperEdge = false;
                onLowerEdge = false;
            }
        }

        public void onRender()
        {
            bool shouldUpdate = false;

            if (onLeftEdge)
            {
                angleH -= 0.01f;
                shouldUpdate = true;
            }else if (onRightEdge)
            {
                angleH += 0.01f;
                shouldUpdate = true;
            }

            if (onUpperEdge)
            {
                if (angleV > -90.0f)
                {
                    angleV -= 0.01f;
                    shouldUpdate = true;
                }
            }
            else if (onLowerEdge)
            {
                if (angleV < 90.0f)
                {
                    angleV += 0.01f;
                    shouldUpdate = true;
                }
            }

            if (shouldUpdate)
            {
                update();
            }
        }
        
        public void update()
        {
            Vector3 vAxis = new Vector3(0.0f, 1.0f, 0.0f);

            // Rotate the view vector by the horizontal angle around the vertical axis
            Vector3 view = new Vector3(1.0f, 0.0f, 0.0f);
            Quaternion rotViewH = Quaternion.FromAxisAngle(vAxis, angleH);
            view = Vector3.Transform(view, rotViewH);
            view.Normalize();

            // Rotate the view vector by the vertical angle around the horizontal axis
            Vector3 hAxis = Vector3.Cross(vAxis, view);
            hAxis.Normalize();

            Quaternion rotViewV = Quaternion.FromAxisAngle(hAxis, angleV);
            view = Vector3.Transform(view, rotViewV);
            view.Normalize();

            Target = view;
            Target.Normalize();

            Up = Vector3.Cross(Target, hAxis);
            Up.Normalize();
        }

        public void viewportSize(int viewportWidth, int viewportHeight)
        {
            this.Width = viewportWidth;
            this.Height = viewportHeight;
            float aspectRatio = Width / (float)Height;
            projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 1.0f, 4096.0f);
        }

        public void setupGLRenderMatrix(int shaderProgram)
        {
            // setup projection
            var projectionMatrixLocation = GL.GetUniformLocation(shaderProgram, "projection_matrix");
            GL.Viewport(0, 0, Width, Height);
            GL.UniformMatrix4(projectionMatrixLocation, false, ref projectionMatrix);

            // create and setup camera view matrix
            var modelviewMatrixLocation = GL.GetUniformLocation(shaderProgram, "modelview_matrix");
            Matrix4 modelViewMatrix = Matrix4.LookAt(Pos, Pos + Target, Up);
            GL.UniformMatrix4(modelviewMatrixLocation, false, ref modelViewMatrix);
        }

        public void Dolly(float distance)
        {
            Pos += distance * Target;
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
            Matrix4 cameraMatrix = Matrix4.LookAt(Pos, Pos + Target, Up);
            // then extract only the camera ROTATION, and turn that into a rotation matrix..
            Matrix4 cameraRotationMatrix = Matrix4.CreateFromQuaternion(cameraMatrix.ExtractRotation());

            // now transform our pan-vector into camera space by using the inverse rotation matrix
            Pos += Vector3.Transform(mousePanVector, cameraRotationMatrix.Inverted());
        }

        public void OrbitXY(float x, float y)
        {
            // extract our "orbit target point"
            Vector3 orbitTarget = Pos + (Target * 10.0f);

            Quaternion camera_Rotation = Matrix4.LookAt(Pos, Pos + Target, Up).ExtractRotation();

            Quaternion yaw_Rotation = Quaternion.FromAxisAngle(Vector3.UnitY, DegreeToRadian(-x));
            Quaternion pitch_Rotation = Quaternion.FromAxisAngle(Vector3.UnitX, DegreeToRadian(y));

            Quaternion qResult = yaw_Rotation * pitch_Rotation;

            Matrix4 newOrientation = Matrix4.CreateFromQuaternion(qResult);

            // recalculate our new orientation
            Target = Vector3.Transform(Target, newOrientation);
            Up = Vector3.Transform(Up, newOrientation);
            // recalulate our new position
            Pos = orbitTarget - (Target * 10.0f);

        }

        private float DegreeToRadian(float angleInDegrees)
        {
            return (float)Math.PI * angleInDegrees / 180.0f;
        }

        public double RadiansToDegrees(double radians)
        {
            const double radToDeg = 180.0 / System.Math.PI;
            return radians * radToDeg;
        }

    }
}
