using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWFormatLib.Utils;

namespace WoWFormatLib.Structs.M2
{
    public struct M2Model
    {
        public uint version;
        public string filename;
        public string name;
        public GlobalModelFlags flags;
        public Sequence[] sequences;
        public Animation[] animations;
        public AnimationLookup[] animationlookup;
        public Bone[] bones;
        public KeyBoneLookup[] keybonelookup;
        public Vertice[] vertices;
        public uint nViews;
        public WoWFormatLib.Structs.SKIN.SKIN[] skins;
        public Color[] colors;
        public List<String> textures;
        public Transparency[] transparency;
        public UVAnimation[] uvanimations;
        public TexReplace[] texreplace;
        public RenderFlag[] renderflags;
        public BoneLookupTable[] bonelookuptable;
        public TexLookup[] texlookup;
        //unk1
        public TransLookup[] translookup;
        public UVAnimLookup[] uvanimlookup;
        public Vector3[] vertexbox;
        public float vertexradius;
        public Vector3[] boundingbox;
        public float boundingradius;
        public BoundingTriangle[] boundingtriangles;
        public BoundingNormal[] boundingnormals;
        public Attachment[] attachments;
        public AttachLookup[] attachlookup;
        public Event[] events;
        public Light[] lights;
        public Camera[] cameras;
        public CameraLookup[] cameralookup;
        public RibbonEmitter[] ribbonemitters;
        public ParticleEmitter[] particleemitters;
    }

    [Flags]
    public enum GlobalModelFlags
    {
        Flag_0x1_TiltX = 0x1,
        Flag_0x2_TiltY = 0x2,
        Flag_0x4 = 0x4,
        Flag_0x8_ExtraHeaderField = 0x8,
        Flag_0x10 = 0x10,
    }

    public struct Sequence
    {
        public uint timestamp;
    }

    public struct ParticleEmitter
    {
        //needs filling in
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

    public struct AnimationLookup
    {
        public ushort animationID;
    }

    public struct Bone
    {
        public int boneId;
        public uint flags;
        public short parentBone;
        private unsafe fixed ushort unk[3];
        public ABlock<Vector3> translation;
        public ABlock<Quaternion> rotation;
        public ABlock<Vector3> scale;
        public Vector3 pivot;
    }

    public struct UVAnimation
    {
        public ABlock<Vector3> translation;
        public ABlock<Quaternion> rotation;
        public ABlock<Vector3> scaling;
    }

    public struct Transparency
    {
        public ABlock<short> alpha;
    }

    public struct Color
    {
        public ABlock<RGBColor> color;
        public ABlock<short> alpha;
    }

    public struct Vertice
    {
        public Vector3 position;
        public unsafe fixed byte boneWeight[4];
        public unsafe fixed byte boneIndices[4];
        public Vector3 normal;
        public float textureCoordX;
        public float textureCoordY;
        public unsafe fixed float unknown[2];
    }

    public struct AttachLookup
    {
        public ushort attachment;
    }

    public struct Attachment
    {
        public uint id;
        public uint bone;
        public Vector3 position;
        public ABlock<int> data;
    }

    public struct BoundingNormal
    {
        public Vector3 normal;
    }

    public struct BoundingTriangle
    {
        public unsafe fixed ushort index[3];
    }

    public struct Camera
    {
        public uint type;
        public float farClipping;
        public float nearClipping;
        public ABlock<CameraPosition> translationPos;
        public Vector3 position;
        public ABlock<CameraPosition> translationTar;
        public Vector3 target;
        public ABlock<Vector3> scaling;
        public ABlock<float> unkABlock;
    }

    public struct CameraLookup
    {
        public ushort cameraID;
    }

    public struct CameraPosition
    {
        public unsafe fixed float CameraPos[9];
    }

    public struct Event
    {
        public unsafe fixed char identifier[4];
        public uint data;
        public uint bone;
        public Vector3 position;
        public ushort interpolationType;
        public ushort GlobalSequence;
        public uint nTimestampEntries;
        public uint ofsTimestampList;
    }
    public struct Light
    {
        public short type;
        public short bone;
        public Vector3 position;
        public ABlock<RGBColor> ambientColor;
        public ABlock<float> ambientIntensity;
        public ABlock<RGBColor> diffuseColor;
        public ABlock<float> diffuseIntensity;
        public ABlock<int> attenuationStart;
        public ABlock<int> attenuationEnd;
        public ABlock<int> unk;
    }

    public struct TexReplace
    {
        public short textureID;
    }

    public struct BoneLookupTable
    {
        public ushort bone;
    }

    public struct KeyBoneLookup
    {
        public ushort bone;
    }

    public struct TexLookup{
        public ushort textureID;
    }

    public struct TransLookup
    {
        public ushort transparencyID;
    }
    public struct UVAnimLookup
    {
        public ushort animatedTextureID;
    }

    public struct RenderFlag
    {
        public ushort flags;
        public ushort blendingMode;
    }

    public struct RibbonEmitter
    {
        public uint unk;
        public uint boneID;
        public Vector3 position;
        public int nTextures;
        public int ofsTextures;
        public int nBlendRef;
        public int ofsBlendRef;
        public ABlock<RGBColor> color;
        public ABlock<short> opacity;
        public ABlock<int> above;
        public ABlock<int> below;
        public float resolution;
        public float length;
        public float emissionAngle;
        public short renderFlags;
        public ABlock<short> unkABlock;
        public ABlock<bool> unkABlock2;
        public int unk2;
    }
}
