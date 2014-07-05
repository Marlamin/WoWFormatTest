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
        public string modelName;

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

            var version = bin.ReadUInt32();
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

        private RibbonEmitter[] readRibbonEmitters(uint nRibbonEmitters, uint ofsRibbonEmitters, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsRibbonEmitters;
            var ribbonEmitters = new RibbonEmitter[nRibbonEmitters];
            for (int i = 0; i < nRibbonEmitters; i++)
            {
                ribbonEmitters[i] = bin.Read<RibbonEmitter>();
            }
            return ribbonEmitters;
        }

        private CameraLookup[] readCameraLookup(uint nCameraLookup, uint ofsCameraLookup, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsCameraLookup;
            var cameraLookup = new CameraLookup[nCameraLookup];
            for (int i = 0; i < nCameraLookup; i++)
            {
                cameraLookup[i] = bin.Read<CameraLookup>();
            }
            return cameraLookup;
        }

        private Camera[] readCameras(uint nCameras, uint ofsCameras, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsCameras;
            var cameras = new Camera[nCameras];
            for (int i = 0; i < nCameras; i++)
            {
                bin.Read<Camera>();
            }
            return cameras;
        }

        private Light[] readLights(uint nLights, uint ofsLights, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsLights;
            var lights = new Light[nLights];
            for (int i = 0; i < nLights; i++)
            {
                bin.Read<Light>();
            }
            return lights;
        }

        private Event[] readEvents(uint nEvents, uint ofsEvents, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsEvents;
            var events = new Event[nEvents];
            for (int i = 0; i < nEvents; i++)
            {
                events[i] = bin.Read<Event>();
            }
            return events;
        }

        private AttachLookup[] readAttachLookup(uint nAttachLookup, uint ofsAttachLookup, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsAttachLookup;
            var attachlookup = new AttachLookup[nAttachLookup];
            for (int i = 0; i < nAttachLookup; i++)
            {
                attachlookup[i] = bin.Read<AttachLookup>();
            }
            return attachlookup;
        }

        private Attachment[] readAttachments(uint nAttachments, uint ofsAttachments, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsAttachments;
            var attachments = new Attachment[nAttachments];
            for (int i = 0; i < nAttachments; i++)
            {
                attachments[i] = bin.Read<Attachment>();
            }
            return attachments;
        }

        private BoundingNormal[] readBoundingNormals(uint nBoundingNormals, uint ofsBoundingNormals, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsBoundingNormals;
            var boundingNormals = new BoundingNormal[nBoundingNormals];
            for (int i = 0; i < nBoundingNormals; i++)
            {
                boundingNormals[i] = bin.Read<BoundingNormal>();
            }
            return boundingNormals;
        }

        private BoundingTriangle[] readBoundingTriangles(uint nBoundingTriangles, uint ofsBoundingTriangles, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsBoundingTriangles;
            var boundingTriangles = new BoundingTriangle[nBoundingTriangles / 3];
            for (int i = 0; i < nBoundingTriangles / 3; i++)
            {
                boundingTriangles[i] = bin.Read<BoundingTriangle>();
            }
            return boundingTriangles;
        }

        private UVAnimLookup[] readUVAnimLookup(uint nUVAnimLookup, uint ofsUVAnimLookup, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsUVAnimLookup;
            var uvanimlookup = new UVAnimLookup[nUVAnimLookup];
            for (int i = 0; i < nUVAnimLookup; i++)
            {
                uvanimlookup[i] = bin.Read<UVAnimLookup>();
            }
            return uvanimlookup;
        }

        private TransLookup[] readTransLookup(uint nTransLookup, uint ofsTranslookup, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsTranslookup;
            var translookup = new TransLookup[nTransLookup];
            for (int i = 0; i < nTransLookup; i++)
            {
                bin.Read<TransLookup>();
            }
            return translookup;
        }

        private void readUnk1(uint nUnk1, uint ofsUnk1, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsUnk1;
            for (int i = 0; i < nUnk1; i++)
            {
                //wot
            }
        }

        private TexLookup[] readTexLookup(uint nTexLookup, uint ofsTexLookup, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsTexLookup;
            var texlookup = new TexLookup[nTexLookup];
            for (int i = 0; i < nTexLookup; i++)
            {
                texlookup[i] = bin.Read<TexLookup>();
            }
            return texlookup;
        }

        private BoneLookupTable[] readBoneLookupTable(uint nBoneLookupTable, uint ofsBoneLookupTable, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsBoneLookupTable;
            var bonelookuptable = new BoneLookupTable[nBoneLookupTable];
            for (int i = 0; i < nBoneLookupTable; i++)
            {
                bonelookuptable[i] = bin.Read<BoneLookupTable>();
            }
            return bonelookuptable;
        }

        private RenderFlag[] readRenderFlags(uint nRenderFlags, uint ofsRenderFlags, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsRenderFlags;
            var renderflags = new RenderFlag[nRenderFlags];
            for (int i = 0; i < nRenderFlags; i++)
            {
                renderflags[i] = bin.Read<RenderFlag>();
            }
            return renderflags;
        }

        private TexReplace[] readTexReplace(uint nTexReplace, uint ofsTexReplace, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsTexReplace;
            var texreplace = new TexReplace[nTexReplace];
            for (int i = 0; i < nTexReplace; i++)
            {
                bin.Read<TexReplace>();
            }
            return texreplace;
        }

        private UVAnimation[] readUVAnimation(uint nUVAnimation, uint ofsUVAnimation, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsUVAnimation;
            var uvanimations = new UVAnimation[nUVAnimation];
            for (int i = 0; i < nUVAnimation; i++)
            {
                uvanimations[i] = bin.Read<UVAnimation>();
            }
            return uvanimations;
        }

        private Transparency[] readTransparency(uint nTransparency, uint ofsTransparency, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsTransparency;
            var transparency = new Transparency[nTransparency];
            for (int i = 0; i < nTransparency; i++)
            {
                transparency[i] = bin.Read<Transparency>();
            }
            return transparency;
        }

        private Color[] readColors(uint nColors, uint ofsColors, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsColors;
            var colors = new Color[nColors];
            for (int i = 0; i < nColors; i++)
            {
                bin.Read<Color>();
            }
            return colors;
        }

        private Vertice[] readVertices(uint nVertices, uint ofsVertices, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsVertices;
            var vertices = new Vertice[nVertices];
            for (int i = 0; i < nVertices; i++)
            {
                vertices[i] = bin.Read<Vertice>();
            }
            return vertices;
        }

        private void readKeyboneLookup(uint nKeyboneLookup, uint ofsKeyboneLookup, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsKeyboneLookup;
            for (int i = 0; i < nKeyboneLookup; i++)
            {
                var bone = bin.ReadUInt16();
            }
        }

        private Bone[] readBones(uint nBones, uint ofsBones, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsBones;
            var bones = new Bone[nBones];
            for (int i = 0; i < nBones; i++)
            {
                bones[i] = bin.Read<Bone>();
            }
            return bones;
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
