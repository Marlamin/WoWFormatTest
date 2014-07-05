using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWFormatLib.Utils;

namespace WoWFormatLib.Structs.M2
{
    public struct Sequence
    {
        public uint timestamp;
    }

    public struct Animation
    {
        public ushort animationID;
        public ushort subAnimationID;
        public uint length;
        public float movingSpeed;
        public uint flags;
        public short probability;
        public ushort unused;
        public uint unk1;
        public uint unk2;
        public uint playbackSpeed;
        public Vector3 minimumExtent;
        public Vector3 maximumExtent;
        public float boundsRadius;
        public short nextAnimation;
        public ushort index;
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
}
