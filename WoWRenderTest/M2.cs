using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using System.Runtime.InteropServices;

namespace WoWRenderTest
{
    class M2
    {
        private static Buffer _vertexBuffer;
        private static Buffer _indexBuffer;
        private int index;
        private int count;
        private static long vertexOffset;

        public M2(Device device, Vector4[] vertices)
        {
            var context = device.ImmediateContext;
            if (_vertexBuffer == null)
            {
                _vertexBuffer = new Buffer(device, Utilities.SizeOf<Vector4>() * 2 * 100000, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, Utilities.SizeOf<Vector4>() * 2);
                context.InputAssembler.SetVertexBuffers(1, new SharpDX.Direct3D11.VertexBufferBinding(_vertexBuffer, Utilities.SizeOf<Vector4>() * 2, 0));

                _indexBuffer = new Buffer(device, Utilities.SizeOf<short>(), ResourceUsage.Dynamic, BindFlags.IndexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, Utilities.SizeOf<short>());
                context.InputAssembler.SetIndexBuffer(_indexBuffer, Format.R16_UInt, 0);
            }

            DataStream stream;
            context.MapSubresource(_vertexBuffer, MapMode.WriteNoOverwrite, MapFlags.None, out stream);

            stream.Seek(vertexOffset, SeekOrigin.Begin);

            count = vertices.Length / 2;
            index = (int)stream.Position / 32;

            stream.WriteRange(vertices);

            vertexOffset = stream.Position;

            context.UnmapSubresource(_vertexBuffer, 0);
        }

        public void Render(Device device)
        {
            var context = device.ImmediateContext;

            context.Draw(count, index);
        }

        public static Vector4[] Parse(string s, Vector3 position, Vector3 rotation, float scale)
        {
            float d = (float)(Math.PI / 180);

            Matrix m = Matrix.Identity;
            m *= Matrix.RotationX(rotation.X * d);
            m *= Matrix.RotationY(-rotation.Y * d);
            m *= Matrix.RotationZ(rotation.Z * d);

            return Parse(s, position, m, scale);
        }

        public static Vector4[] Parse(string s, Vector3 position, Matrix rotation, float scale)
        {
            var vertices = new List<Vector4>();
            var color = new[]
            {
                new Color4(1, 0, 0, 1),
                new Color4(.8f, 0, 0, 1)
            };

            var file = new MpqFile(MpqArchive.Open(s));
            var header = file.ReadStruct<M2Header>();

            if (header.magic != "MD20")
                throw new NotSupportedException();

            if (header.version != 264)
                throw new NotSupportedException();

            if (header.numBoundingVertices == 0)
                return vertices.ToArray();

            var indices = new short[header.numBoundingTriangles];
            file.Seek(header.offsetBoundingTriangles, SeekOrigin.Begin);

            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = file.ReadInt16();
            }

            var vertices2 = new Vector4[header.numBoundingVertices];
            file.Seek(header.offsetBoundingVertices, SeekOrigin.Begin);
            float[] tmp = new float[3];

            var m = Matrix.Identity;
            m *= rotation;
            m *= Matrix.Scaling(scale);
            m *= Matrix.Translation(position);

            Vector4 pos = new Vector4(position, 0);

            for (int i = 0; i < vertices2.Length; i++)
            {
                tmp[0] = file.ReadSingle();
                tmp[1] = file.ReadSingle();
                tmp[2] = file.ReadSingle();

                vertices2[i] = new Vector4(tmp[1], tmp[2], -tmp[0], 1);
                vertices2[i] = Vector4.Transform(vertices2[i], m);
            }

            for (int i = 0; i < indices.Length; i += 3)
            {
                vertices.AddRange(new[]
                {
                    vertices2[indices[i + 2]], color[i / 3 % 2].ToVector4(),
                    vertices2[indices[i + 1]], color[i / 3 % 2].ToVector4(),
                    vertices2[indices[i]], color[i / 3 % 2].ToVector4(),
                });
            }

            return vertices.ToArray();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct M2Header
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        private char[] _magic;
        public uint version;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0xD0)]
        private byte[] pad;

        public uint numBoundingTriangles;
        public uint offsetBoundingTriangles;
        public uint numBoundingVertices;
        public uint offsetBoundingVertices;

        public string magic
        {
            get { return new string(_magic, 0, 4); }
        }
    }
}
