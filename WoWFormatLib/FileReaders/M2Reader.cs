using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWFormatLib.Structs.M2;
using WoWFormatLib.Utils;

namespace WoWFormatLib.FileReaders
{
    [Flags]
    public enum GlobalModelFlags
    {
        Flag_0x1_TiltX = 0x1,
        Flag_0x2_TiltY = 0x2,
        Flag_0x4 = 0x4,
        Flag_0x8_ExtraHeaderField = 0x8,
        Flag_0x10 = 0x10,
    }
    public class M2Reader
    {
        private List<String> blpFiles;
        private string basedir;

        public uint version;
        public string modelName;
        public GlobalModelFlags modelFlags;
        public Sequence[] sequences;
        public Animation[] animations;

        public M2Reader(string basedir)
        {
            this.basedir = basedir;
        }

        public void LoadM2(string filename)
        {
            filename = Path.ChangeExtension(filename, "M2");
            if (!File.Exists(basedir + filename))
            {
                new WoWFormatLib.Utils.MissingFile(filename);
                return;
            }
            
            blpFiles = new List<string>();
            
            FileStream m2 = File.Open(basedir + filename, FileMode.Open);
            BinaryReader bin = new BinaryReader(m2);

            var header = new string(bin.ReadChars(4));
            if (header != "MD20")
            {
                Console.WriteLine("Invalid M2 file!");
                Console.ReadLine();
            }

            version = bin.ReadUInt32();
            var lenModelname = bin.ReadUInt32();
            var ofsModelname = bin.ReadUInt32();
            var modelFlags = (GlobalModelFlags) bin.ReadUInt32();
            var nSequences = bin.ReadUInt32();
            var ofsSequences = bin.ReadUInt32();
            var nAnimations = bin.ReadUInt32();
            var ofsAnimations = bin.ReadUInt32();
            var nAnimationLookup = bin.ReadUInt32();
            var ofsAnimationLookup = bin.ReadUInt32();
            var nBones = bin.ReadUInt32();
            var ofsBones = bin.ReadUInt32();
            var nKeyboneLookup = bin.ReadUInt32();
            var ofsKeyboneLookup = bin.ReadUInt32();
            var nVertices = bin.ReadUInt32();
            var ofsVertices = bin.ReadUInt32();
            var nViews = bin.ReadUInt32();
            var nColors = bin.ReadUInt32();
            var ofsColors = bin.ReadUInt32();
            var nTextures = bin.ReadUInt32();
            var ofsTextures = bin.ReadUInt32();
            var nTransparency = bin.ReadUInt32();
            var ofsTransparency = bin.ReadUInt32();
            var nUVAnimation = bin.ReadUInt32();
            var ofsUVAnimation = bin.ReadUInt32();
            var nTexReplace = bin.ReadUInt32();
            var ofsTexReplace = bin.ReadUInt32();
            var nRenderFlags = bin.ReadUInt32();
            var ofsRenderFlags = bin.ReadUInt32();
            var nBoneLookupTable = bin.ReadUInt32();
            var ofsBoneLookupTable = bin.ReadUInt32();
            var nTexLookup = bin.ReadUInt32();
            var ofsTexLookup = bin.ReadUInt32();
            var nUnk1 = bin.ReadUInt32();
            var ofsUnk1 = bin.ReadUInt32();
            var nTransLookup = bin.ReadUInt32();
            var ofsTranslookup = bin.ReadUInt32();
            var nUVAnimLookup = bin.ReadUInt32();
            var ofsUVAnimLookup = bin.ReadUInt32();
            var vertexBox = new Vector3[2];
            vertexBox[0] = new Vector3(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
            vertexBox[1] = new Vector3(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
            var VertexRadius = bin.ReadSingle();
            var boundingBox = new Vector3[2];
            boundingBox[0] = new Vector3(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
            boundingBox[1] = new Vector3(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
            var BoundingRadius = bin.ReadSingle();
            var nBoundingTriangles = bin.ReadUInt32();
            var ofsBoundingTriangles = bin.ReadUInt32();
            var nBoundingVertices = bin.ReadUInt32();
            var ofsBoundingVertices = bin.ReadUInt32();
            var nBoundingNormals = bin.ReadUInt32();
            var ofsBoundingNormals = bin.ReadUInt32();
            var nAttachments = bin.ReadUInt32();
            var ofsAttachments = bin.ReadUInt32();
            var nAttachLookup = bin.ReadUInt32();
            var ofsAttachLookup = bin.ReadUInt32();
            var nEvents = bin.ReadUInt32();
            var ofsEvents = bin.ReadUInt32();
            var nLights = bin.ReadUInt32();
            var ofsLights = bin.ReadUInt32();
            var nCameras = bin.ReadUInt32();
            var ofsCameras = bin.ReadUInt32();
            var nCameraLookup = bin.ReadUInt32();
            var ofsCameraLookup = bin.ReadUInt32();
            var nRibbonEmitters = bin.ReadUInt32();
            var ofsRibbonEmitters = bin.ReadUInt32();
            var nParticleEmitters = bin.ReadUInt32();
            var ofsParticleEmitters = bin.ReadUInt32();

            if (GlobalModelFlags.Flag_0x8_ExtraHeaderField != 0) //models with flag 8 have extra field
            {
                var nUnk2 = bin.ReadUInt32();
                var ofsUnk2 = bin.ReadUInt32();
            }

            bin.BaseStream.Position = ofsModelname;
            modelName = new string(bin.ReadChars(int.Parse(lenModelname.ToString())));

            var sequences = readSequences(nSequences, ofsSequences, bin);
            var animations = readAnimations(nAnimations, ofsAnimations, bin);
            readAnimationLookup(nAnimationLookup, ofsAnimationLookup, bin);
            readBones(nBones, ofsBones, bin);
            readKeyboneLookup(nKeyboneLookup, ofsKeyboneLookup, bin);
            readVertices(nVertices, ofsVertices, bin);
            readSkins(nViews, filename);
            readColors(nColors, ofsColors, bin);
            readTextures(nTextures, ofsTextures, bin);
            readTransparency(nTransparency, ofsTransparency, bin);
            readUVAnimation(nUVAnimation, ofsUVAnimation, bin);
            readTexReplace(nTexReplace, ofsTexReplace, bin);
            readRenderFlags(nRenderFlags, ofsRenderFlags, bin);
            readBoneLookupTable(nBoneLookupTable, ofsBoneLookupTable, bin);
            readTexLookup(nTexLookup, ofsTexLookup, bin);
            readUnk1(nUnk1, ofsUnk1, bin);
            readTransLookup(nTransLookup, ofsTranslookup, bin);
            readUVAnimLookup(nUVAnimLookup, ofsUVAnimLookup, bin);
            readBoundingTriangles(nBoundingTriangles, ofsBoundingTriangles, bin);
            readBoundingNormals(nBoundingNormals, ofsBoundingNormals, bin);
            readAttachments(nAttachments, ofsAttachments, bin);
            readAttachLookup(nAttachLookup, ofsAttachLookup, bin);
            readEvents(nEvents, ofsEvents, bin);
            readLights(nLights, ofsLights, bin);
            readCameras(nCameras, ofsCameras, bin);
            readCameraLookup(nCameraLookup, ofsCameraLookup, bin);
            readRibbonEmitters(nRibbonEmitters, ofsRibbonEmitters, bin);
            readParticleEmitters(nParticleEmitters, ofsParticleEmitters, bin);

            m2.Close();
        }

        private void readParticleEmitters(uint nParticleEmitters, uint ofsParticleEmitters, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsParticleEmitters;
            for (int i = 0; i < nParticleEmitters; i++)
            {
                //Apparently really wrong. Who needs particles, right? 
            }
        }

        private void readRibbonEmitters(uint nRibbonEmitters, uint ofsRibbonEmitters, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsRibbonEmitters;
            for (int i = 0; i < nRibbonEmitters; i++)
            {
                var unk = bin.ReadUInt32();
                var boneID = bin.ReadUInt32();
                var position = new Vector3(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                var nTextures = bin.ReadInt32();
                var ofsTextures = bin.ReadInt32();
                var nBlendRef = bin.ReadInt32();
                var ofsBlendRef = bin.ReadInt32();
                var color = bin.ReadBytes(20); //temp while ablock isnt implemented
                var opacity = bin.ReadBytes(20); //temp while ablock isnt implemented
                var above = bin.ReadBytes(20); //temp while ablock isnt implemented
                var below = bin.ReadBytes(20); //temp while ablock isnt implemented
                var resolution = bin.ReadSingle();
                var length = bin.ReadSingle();
                var emissionAngle = bin.ReadSingle();
                var renderFlags = bin.ReadInt16();
                var unkABlock = bin.ReadBytes(20); //temp while ablock isnt implemented
                var unkABlock2 = bin.ReadBytes(20); //temp while ablock isnt implemented
                var unk2 = bin.ReadInt32();
            }
        }

        private void readCameraLookup(uint nCameraLookup, uint ofsCameraLookup, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsCameraLookup;
            for (int i = 0; i < nCameraLookup; i++)
            {
                var cameraID = bin.ReadUInt16();
            }
        }

        private void readCameras(uint nCameras, uint ofsCameras, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsCameras;
            for (int i = 0; i < nCameras; i++)
            {
                var type = bin.ReadUInt32();
                var farClipping = bin.ReadSingle();
                var nearClipping = bin.ReadSingle();
                var translationPos = bin.ReadBytes(20); //temp while ablock isnt implemented
                var position = new Vector3(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                var translationTar = bin.ReadBytes(20); //temp while ablock isnt implemented
                var target = new Vector3(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                var scaling = bin.ReadBytes(20); //temp while ablock isnt implemented
                var unkABlock = bin.ReadBytes(20); //temp while ablock isnt implemented
            }
        }

        private void readLights(uint nLights, uint ofsLights, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsLights;
            for (int i = 0; i < nLights; i++)
            {
                var type = bin.ReadUInt16();
                var bone = bin.ReadInt16();
                var position = new Vector3(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                var ambientColor = bin.ReadBytes(20); //temp while ablock isnt implemented
                var ambientIntensity = bin.ReadBytes(20); //temp while ablock isnt implemented
                var diffuseColor = bin.ReadBytes(20); //temp while ablock isnt implemented
                var diffuseIntensity = bin.ReadBytes(20); //temp while ablock isnt implemented
                var attenuationStart = bin.ReadBytes(20); //temp while ablock isnt implemented
                var attenuationEnd = bin.ReadBytes(20); //temp while ablock isnt implemented
                var unk = bin.ReadBytes(20); //temp while ablock isnt implemented
            }
        }

        private void readEvents(uint nEvents, uint ofsEvents, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsEvents;
            for (int i = 0; i < nEvents; i++)
            {
                //Isn't correct.
                var identifier = Encoding.ASCII.GetString(bin.ReadBytes(4));
                var data = bin.ReadUInt32();
                var bone = bin.ReadUInt32();
                var position = new Vector3(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                var interpolationType = bin.ReadUInt16();
                var globalSequence = bin.ReadUInt16();
                var nTimestampEntries = bin.ReadUInt32();
                var ofsTimestampList = bin.ReadUInt32();
            }
        }

        private void readAttachLookup(uint nAttachLookup, uint ofsAttachLookup, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsAttachLookup;
            for (int i = 0; i < nAttachLookup; i++)
            {
                var attachment = bin.ReadUInt16();
            }
        }

        private void readAttachments(uint nAttachments, uint ofsAttachments, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsAttachments;
            for (int i = 0; i < nAttachments; i++)
            {
                var id = bin.ReadUInt32();
                var bone = bin.ReadUInt32();
                var position = new Vector3(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                var data = bin.ReadBytes(20); //temp while ablock isnt implemented
            }
        }

        private void readBoundingNormals(uint nBoundingNormals, uint ofsBoundingNormals, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsBoundingNormals;
            for (int i = 0; i < nBoundingNormals; i++)
            {
                var normal = new Vector3(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
            }
        }

        private void readBoundingTriangles(uint nBoundingTriangles, uint ofsBoundingTriangles, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsBoundingTriangles;
            for (int i = 0; i < nBoundingTriangles / 3; i++)
            {
                var index = new UInt16[3];
                index[0] = bin.ReadUInt16();
                index[1] = bin.ReadUInt16();
                index[2] = bin.ReadUInt16();
            }
        }

        private void readUVAnimLookup(uint nUVAnimLookup, uint ofsUVAnimLookup, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsUVAnimLookup;
            for (int i = 0; i < nUVAnimLookup; i++)
            {
                var animatedTextureID = bin.ReadUInt16();
            }
        }

        private void readTransLookup(uint nTransLookup, uint ofsTranslookup, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsTranslookup;
            for (int i = 0; i < nTransLookup; i++)
            {
                var transparencyID = bin.ReadUInt16();
            }
        }

        private void readUnk1(uint nUnk1, uint ofsUnk1, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsUnk1;
            for (int i = 0; i < nUnk1; i++)
            {

            }
        }

        private void readTexLookup(uint nTexLookup, uint ofsTexLookup, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsTexLookup;
            for (int i = 0; i < nTexLookup; i++)
            {
                var textureID = bin.ReadUInt16();
            }
        }

        private void readBoneLookupTable(uint nBoneLookupTable, uint ofsBoneLookupTable, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsBoneLookupTable;
            for (int i = 0; i < nBoneLookupTable; i++)
            {
                var bone = bin.ReadUInt16();
            }
        }

        private void readRenderFlags(uint nRenderFlags, uint ofsRenderFlags, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsRenderFlags;
            for (int i = 0; i < nRenderFlags; i++)
            {
                var flags = bin.ReadUInt16();
                var blendingMode = bin.ReadUInt16();
            }
        }

        private void readTexReplace(uint nTexReplace, uint ofsTexReplace, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsTexReplace;
            for (int i = 0; i < nTexReplace; i++)
            {
                var textureID = bin.ReadInt16();
            }
        }

        private void readUVAnimation(uint nUVAnimation, uint ofsUVAnimation, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsUVAnimation;
            for (int i = 0; i < nUVAnimation; i++)
            {
                var translation = bin.ReadBytes(20); //temp while ablock isnt implemented
                var rotation = bin.ReadBytes(20); //temp while ablock isnt implemented
                var scaling = bin.ReadBytes(20); //temp while ablock isnt implemented   
            }
        }

        private void readTransparency(uint nTransparency, uint ofsTransparency, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsTransparency;
            for (int i = 0; i < nTransparency; i++)
            {
                var alpha = bin.ReadBytes(20); //temp while ablock isnt implemented
            }
        }

        private void readColors(uint nColors, uint ofsColors, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsColors;
            for (int i = 0; i < nColors; i++)
            {
                var color = bin.ReadBytes(20); //temp while ablock isnt implemented
                var alpha = bin.ReadBytes(20); //temp while ablock isnt implemented
            }
        }

        private void readVertices(uint nVertices, uint ofsVertices, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsVertices;
            for (int i = 0; i < nVertices; i++)
            {
                var position = new Vector3(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                Byte[] boneWeight = new Byte[4];
                boneWeight[0] = bin.ReadByte();
                boneWeight[1] = bin.ReadByte();
                boneWeight[2] = bin.ReadByte();
                boneWeight[3] = bin.ReadByte();
                Byte[] boneIndices = new Byte[4];
                boneIndices[0] = bin.ReadByte();
                boneIndices[1] = bin.ReadByte();
                boneIndices[2] = bin.ReadByte();
                boneIndices[3] = bin.ReadByte();
                var normal = new Vector3(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                Single[] textureCoords = new Single[2];
                textureCoords[0] = bin.ReadSingle();
                textureCoords[1] = bin.ReadSingle();
                Single[] unknown = new Single[2];
                unknown[0] = bin.ReadSingle();
                unknown[1] = bin.ReadSingle();
            }
        }

        private void readKeyboneLookup(uint nKeyboneLookup, uint ofsKeyboneLookup, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsKeyboneLookup;
            for (int i = 0; i < nKeyboneLookup; i++)
            {
                var bone = bin.ReadUInt16();
            }
        }

        private void readBones(uint nBones, uint ofsBones, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsBones;
            for (int i = 0; i < nBones; i++)
            {
                var bone = bin.Read<Bone>();
            }
        }

        private void readAnimationLookup(uint nAnimationLookup, uint ofsAnimationLookup, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsAnimationLookup;
            for (int i = 0; i < nAnimationLookup; i++)
            {
                var animationID = bin.ReadUInt16();
            }
        }

        private Animation[] readAnimations(uint nAnimations, uint ofsAnimations, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsAnimations;
            var animations = new Animation[nAnimations];
            for (int i = 0; i < nAnimations; i++)
            {
                animations[i] = bin.Read<Animation>();
            }
            return animations;
        }

        private Sequence[] readSequences(uint nSequences, uint ofsSequences, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsSequences;
            var sequences = new Sequence[nSequences];
            for (int i = 0; i < nSequences; i++)
            {
                sequences[i] = bin.Read<Sequence>();
            }
            return sequences;
        }

        private void readTextures(UInt32 num, UInt32 offset, BinaryReader bin)
        {
            bin.BaseStream.Position = offset;
            for (int i = 0; i < num; i++)
            {
                var type = bin.ReadUInt32();
                var flags = bin.ReadUInt32();
                if (flags == 0)
                {
                    var lenFilename = bin.ReadUInt32();
                    var ofsFilename = bin.ReadUInt32();
                    var preFilenamePosition = bin.BaseStream.Position; // probably a better way to do all this
                    bin.BaseStream.Position = ofsFilename;
                    var filename = new string(bin.ReadChars(int.Parse(lenFilename.ToString())));
                    filename = filename.Replace("\0", "");
                    if (!filename.Equals(""))
                    {
                        blpFiles.Add(filename);
                        if (!System.IO.File.Exists(System.IO.Path.Combine(basedir, filename)))
                        {
                            Console.WriteLine("BLP file does not exist!!! {0}", filename);
                            new WoWFormatLib.Utils.MissingFile(filename);
                        }
                    }
                    bin.BaseStream.Position = preFilenamePosition;
                }
            }
        }

        private void readSkins(UInt32 num, String filename)
        {
            for (int i = 0; i < num; i++)
            {
                var skinfilename = filename.Replace(".M2", i.ToString().PadLeft(2, '0') + ".skin");
                if (!System.IO.File.Exists(System.IO.Path.Combine(basedir, skinfilename)))
                {
                    Console.WriteLine(".skin file does not exist!!! {0}", skinfilename);
                    new WoWFormatLib.Utils.MissingFile(filename);
                }
                else
                {
                    SKINReader skinreader = new SKINReader(basedir);
                    skinreader.LoadSKIN(skinfilename);
                }
            }
        }
    }
}
