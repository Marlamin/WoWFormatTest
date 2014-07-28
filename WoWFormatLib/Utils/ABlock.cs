using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace WoWFormatLib
{
    public struct ArrayReference<T> where T : struct
    {
        public uint Number;
        private long elementsOffset;

        public IEnumerable<T> GetElements(BinaryReader bin)
        {
            var type = typeof(T);
            for (int i = 0; i < Number; i++)
            {
                var offset = elementsOffset + (i * Marshal.SizeOf(type));
                bin.BaseStream.Position += offset;
                yield return (T)bin.Read<T>();
            }
        }
    }

    public struct ABlock<T> where T : struct
    {
        public ushort InterpolationType;
        public ushort GlobalSequence;
        public ArrayReference<ArrayReference<uint>> Timestamps;
        public ArrayReference<ArrayReference<T>> Values;
    }

    public static class Extensions
    {
        public static T Read<T>(this BinaryReader bin)
        {
            var bytes = bin.ReadBytes(Marshal.SizeOf(typeof(T)));
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T ret = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return ret;
        }
    }
}