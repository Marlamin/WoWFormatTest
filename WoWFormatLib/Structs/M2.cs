using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWFormatLib.Utils;

namespace WoWFormatLib.Structs
{
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
}
