using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WoWFormatLib.Utils
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3(float x, float y, float z)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double DistanceTo(Vector3 loc)
        {
            return Math.Sqrt(Math.Pow(X - loc.X, 2) + Math.Pow(Y - loc.Y, 2) + Math.Pow(Z - loc.Z, 2));
        }

        public double Distance2D(Vector3 loc)
        {
            return Math.Sqrt(Math.Pow(X - loc.X, 2) + Math.Pow(Y - loc.Y, 2));
        }

        public double Length
        {
            get { return Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2)); }
        }

        public Vector3 Normalize()
        {
            double len = Length;
            return new Vector3((float)(X / len), (float)(Y / len), (float)(Z / len));
        }

        public float[] ToFloatArray(bool xyz = false)
        {
            if (xyz)
                return new[] { X, Y, Z };
            return new[] { X, Z, Y };
        }

        public float Angle
        {
            get { return (float)Math.Atan2(Y, X); }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var loc = (Vector3)obj;
            if (loc.X != X || loc.Y != Y || loc.Z != Z)
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() | Y.GetHashCode() | Z.GetHashCode();
        }

        public override string ToString()
        {
            return "[" + (int)X + ", " + (int)Y + ", " + (int)Z + "]";
        }

        public static bool operator ==(Vector3 a, Vector3 b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Vector3 a, Vector3 b)
        {
            return !a.Equals(b);
        }

        public static Vector3 Invalid
        {
            get { return default(Vector3); }
        }
    }

    public struct Blackspot
    {
        public Vector3 Location;
        public float Radius;

        public Blackspot(float x, float y, float z, float r)
            : this()
        {
            Location = new Vector3(x, y, z);
            Radius = r;
        }
    }
}
