/*
        DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE 
                    Version 2, December 2004 

 Copyright (C) 2004 Sam Hocevar <sam@hocevar.net> 

 Everyone is permitted to copy and distribute verbatim or modified 
 copies of this license document, and changing it is allowed as long 
 as the name is changed. 

            DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE 
   TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION 

  0. You just DO WHAT THE FUCK YOU WANT TO.
*/
using System;
using System.Collections.Generic;
using WoWFormatLib.Utils;

namespace WoWFormatLib.Structs.M2
{
    public struct M2Model
    {
        public uint version;

        public int physFileID;
        public int[] boneFileDataIDs;
        public int[] skinFileDataIDs;
        public int[] lod_skinFileDataIDs;
        public AFID[] animFileData;

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
        public Texture[] textures;
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

    [Flags]
    public enum TextureFlags
    {
        Flag_0x1_WrapX = 0x1,
        Flag_0x2_WrapY = 0x2,
    }
    public struct Sequence
    {
        public uint timestamp;
    }

    public struct ParticleEmitter
    {
        //needs filling in
    }

    public struct AFID
    {
        public short animID;
        public short subAnimID;
        public uint fileDataID; 
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
        private ushort unk_0;
        private ushort unk_1;
        private ushort unk_2;
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
        public byte boneWeight_0;
        public byte boneWeight_1;
        public byte boneWeight_2;
        public byte boneWeight_3;
        public byte boneIndices_0;
        public byte boneIndices_1;
        public byte boneIndices_2;
        public byte boneIndices_3;
        public Vector3 normal;
        public float textureCoordX;
        public float textureCoordY;
        public float unk_0;
        public float unk_1;
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
        public ushort index_0;
        public ushort index_1;
        public ushort index_2;
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
        public float CameraPos_0;
        public float CameraPos_1;
        public float CameraPos_2;
        public float CameraPos_3;
        public float CameraPos_4;
        public float CameraPos_5;
        public float CameraPos_6;
        public float CameraPos_7;
        public float CameraPos_8;
    }

    public struct Event
    {
        public char identifier_0;
        public char identifier_1;
        public char identifier_2;
        public char identifier_3;
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

    public struct TexLookup
    {
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

    public struct Texture
    {
        public uint type;
        public TextureFlags flags;
        public string filename;
    }


}