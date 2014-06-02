using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace WoWRenderTest
{
    public class Wmo
    {
        private static Buffer _vertexBuffer;
        private int count;
        private int index;
        private static long vertexOffset;

        public Wmo(Device device, Vector4[] vertices)
        {
            var context = device.ImmediateContext;
            if (_vertexBuffer == null)
            {
                _vertexBuffer = new Buffer(device, Utilities.SizeOf<Vector4>() * 10000000, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, Utilities.SizeOf<Vector4>());
                context.InputAssembler.SetVertexBuffers(2, new SharpDX.Direct3D11.VertexBufferBinding(_vertexBuffer, Utilities.SizeOf<Vector4>(), 0));
            }

            DataStream stream;
            context.MapSubresource(_vertexBuffer, MapMode.WriteNoOverwrite, MapFlags.None, out stream);

            stream.Seek(vertexOffset, SeekOrigin.Begin);

            count = vertices.Length;
            index = (int)stream.Position / 16;

            stream.WriteRange(vertices);

            vertexOffset = stream.Position;

            context.UnmapSubresource(_vertexBuffer, 0);
        }

        public void Render(Device device)
        {
            var context = device.ImmediateContext;

            context.Draw(count, index);
        }

        public static Vector4[] Parse(string s, Vector3 position, Vector3 rotation)
        {
            List<Vector4> vertices = new List<Vector4>();

            var kuk = new RootWmo(s);
            foreach (string hej in kuk.GroupFiles)
            {
                vertices.AddRange(GroupWmo.Parse(hej, position, rotation));
            }

            return vertices.ToArray();
        }

        private class RootWmo
        {
            public string[] GroupFiles;

            public RootWmo(string s)
            {
                var file = new MpqFile(MpqArchive.Open(s));
                file.Seek(file.GetChunkPosition("MOHD"), SeekOrigin.Begin);
                var header = file.ReadStruct<ChunkHeader>();
                var mohd = file.ReadStruct<MOHD>();

                var root = s.Split(new[] { '.' })[0];

                GroupFiles = new string[mohd.nGroups];
                for (int i = 0; i < mohd.nGroups; i++)
                {
                    GroupFiles[i] = string.Format("{0}_{1:000}.WMO", root, i);
                }

                /*foreach (var group in GroupFiles)
                {
                    GroupWmo.Parse(group);
                }*/
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOHD
        {
            private int nMaterials;
            public int nGroups;
            private int nPortals;
            private int nLights;
            private int nModels;
            private int nDoodads;
            private int nSets;
            private int ambient_color;
            private int WMO_ID;
            public Vector3 upperBounds;
            public Vector3 lowerBounds;
            private int unknown;
        }

        private class GroupWmo
        {
            /*public GroupWmo(string s)
            {

            }*/

            public static Vector4[] Parse(string s, Vector3 position, Vector3 rotation)
            {
                List<Vector4> vertices = new List<Vector4>();
                var color = new[]
                {
                    new Color4(1, 1, 0, 1),
                    new Color4(.8f, .8f, 0, 1)
                };
                var file = new MpqFile(MpqArchive.Open(s));

                var offset = file.GetChunkPosition("MOGP");
                offset += 0x4C;
                file.Seek(file.GetChunkPosition("MOVT", offset), SeekOrigin.Begin);
                var header = file.ReadStruct<ChunkHeader>();

                var num = header.Size / Vector3.SizeInBytes;
                float[] tmp = new float[3];
                Vector4[] vertices2 = new Vector4[num];

                for (int i = 0; i < num; i++)
                {
                    tmp[0] = file.ReadSingle();
                    tmp[1] = file.ReadSingle();
                    tmp[2] = file.ReadSingle();

                    vertices2[i] = new Vector4(tmp[1], tmp[2], -tmp[0], 1);
                }

                Matrix m = Matrix.Identity;
                float d = (float)(Math.PI / 180);
                m *= Matrix.RotationX(rotation.X * d);
                m *= Matrix.RotationY(-rotation.Y * d);
                m *= Matrix.RotationZ(rotation.Z * d);
                m *= Matrix.Translation(position);

                Vector4.Transform(vertices2, ref m, vertices2);

                file.Seek(file.GetChunkPosition("MOVI", offset), SeekOrigin.Begin);
                header = file.ReadStruct<ChunkHeader>();
                num = header.Size / sizeof(short);

                short[] indices = new short[num];

                for (int i = 0; i < num; i++)
                {
                    indices[i] = file.ReadInt16();
                }

                for (int i = 0; i < num; i += 3)
                {
                    vertices.AddRange(new[]
                    {
                        vertices2[indices[i + 2]],
                        vertices2[indices[i + 1]],
                        vertices2[indices[i]]
                    });
                }

                return vertices.ToArray();
            }
        }
    }
}
