using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using WoWFormatLib.Utils;

namespace WoWFormatLib
{
    public struct ArrayReference<T> where T : struct
    {
        public uint Number;
        private IntPtr _ElementsPtr;

        public IEnumerable<T> GetElements()
        {
            var type = typeof(T);
            for (int i = 0; i < Number; i++)
                yield return (T)Marshal.PtrToStructure(IntPtr.Add(_ElementsPtr, i * Marshal.SizeOf(type)), type);
        }
    }

    public struct ABlock<T> where T : struct
    {
        public ushort InterpolationType;
        public ushort GlobalSequence;
        public ArrayReference<ArrayReference<uint>> Timestamps;
        public ArrayReference<ArrayReference<T>> Values;
    }

    public struct Bone
    {
        public int BoneId;
        public uint Flags;
        public short ParentBone;
        private unsafe fixed ushort unk[3];
        public ABlock<Vector3> Translation;
        public ABlock<Quaternion> Rotation;
        public ABlock<Vector3> Scale;
        public Vector3 Pivot;
    }

    // --------

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
