using System;
using System.Collections.Generic;
using System.IO;
using WoWFormatLib.Structs.M2;
using WoWFormatLib.Utils;

namespace WoWFormatLib.FileReaders
{
    public class M2Reader
    {
        public M2Model model;
        private string basedir;
        private List<String> blpFiles;

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

            model.version = bin.ReadUInt32();
            var lenModelname = bin.ReadUInt32();
            var ofsModelname = bin.ReadUInt32();
            model.flags = (GlobalModelFlags)bin.ReadUInt32();
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
            model.nViews = bin.ReadUInt32();
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
            model.vertexbox = new Vector3[2];
            model.vertexbox[0] = bin.Read<Vector3>();
            model.vertexbox[1] = bin.Read<Vector3>();
            model.vertexradius = bin.ReadSingle();
            model.boundingbox = new Vector3[2];
            model.boundingbox[0] = bin.Read<Vector3>();
            model.boundingbox[1] = bin.Read<Vector3>();
            model.boundingradius = bin.ReadSingle();
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
            model.name = new string(bin.ReadChars(int.Parse(lenModelname.ToString())));
            model.name = model.name.Remove(model.name.Length - 1); //remove last char, empty
            model.filename = filename;
            model.sequences = readSequences(nSequences, ofsSequences, bin);
            model.animations = readAnimations(nAnimations, ofsAnimations, bin);
            model.animationlookup = readAnimationLookup(nAnimationLookup, ofsAnimationLookup, bin);
            model.bones = readBones(nBones, ofsBones, bin);
            model.keybonelookup = readKeyboneLookup(nKeyboneLookup, ofsKeyboneLookup, bin);
            model.vertices = readVertices(nVertices, ofsVertices, bin);
            model.skins = readSkins(model.nViews, filename);
            model.colors = readColors(nColors, ofsColors, bin);
            model.textures = readTextures(nTextures, ofsTextures, bin);
            model.transparency = readTransparency(nTransparency, ofsTransparency, bin);
            model.uvanimations = readUVAnimation(nUVAnimation, ofsUVAnimation, bin);
            model.texreplace = readTexReplace(nTexReplace, ofsTexReplace, bin);
            model.renderflags = readRenderFlags(nRenderFlags, ofsRenderFlags, bin);
            model.bonelookuptable = readBoneLookupTable(nBoneLookupTable, ofsBoneLookupTable, bin);
            model.texlookup = readTexLookup(nTexLookup, ofsTexLookup, bin);
            readUnk1(nUnk1, ofsUnk1, bin);
            model.translookup = readTransLookup(nTransLookup, ofsTranslookup, bin);
            model.uvanimlookup = readUVAnimLookup(nUVAnimLookup, ofsUVAnimLookup, bin);
            model.boundingtriangles = readBoundingTriangles(nBoundingTriangles, ofsBoundingTriangles, bin);
            model.boundingnormals = readBoundingNormals(nBoundingNormals, ofsBoundingNormals, bin);
            model.attachments = readAttachments(nAttachments, ofsAttachments, bin);
            model.attachlookup = readAttachLookup(nAttachLookup, ofsAttachLookup, bin);
            model.events = readEvents(nEvents, ofsEvents, bin);
            model.lights = readLights(nLights, ofsLights, bin);
            model.cameras = readCameras(nCameras, ofsCameras, bin);
            model.cameralookup = readCameraLookup(nCameraLookup, ofsCameraLookup, bin);
            model.ribbonemitters = readRibbonEmitters(nRibbonEmitters, ofsRibbonEmitters, bin);
            model.particleemitters = readParticleEmitters(nParticleEmitters, ofsParticleEmitters, bin);

            m2.Close();
        }

        private AnimationLookup[] readAnimationLookup(uint nAnimationLookup, uint ofsAnimationLookup, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsAnimationLookup;
            var animationlookup = new AnimationLookup[nAnimationLookup];
            for (int i = 0; i < nAnimationLookup; i++)
            {
                animationlookup[i] = bin.Read<AnimationLookup>();
            }
            return animationlookup;
        }

        private Animation[] readAnimations(uint nAnimations, uint ofsAnimations, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsAnimations;
            var animations = new Animation[nAnimations];
            for (int i = 0; i < nAnimations; i++)
            {
                animations[i] = bin.Read<Animation>();
                if (animations[i].flags == 0)
                {
                    //this check doesnt find all of them yet, needs actual flag parsing
                    string animfilename = model.filename.Replace(".M2", animations[i].animationID.ToString().PadLeft(4, '0') + "-" + animations[i].subAnimationID.ToString().PadLeft(2, '0') + ".anim");
                    if (!File.Exists(basedir + animfilename))
                    {
                        new WoWFormatLib.Utils.MissingFile(animfilename);
                    }
                }
            }
            return animations;
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
                cameras[i] = bin.Read<Camera>();
            }
            return cameras;
        }

        private Color[] readColors(uint nColors, uint ofsColors, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsColors;
            var colors = new Color[nColors];
            for (int i = 0; i < nColors; i++)
            {
                colors[i] = bin.Read<Color>();
            }
            return colors;
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

        private KeyBoneLookup[] readKeyboneLookup(uint nKeyboneLookup, uint ofsKeyboneLookup, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsKeyboneLookup;
            var keybonelookup = new KeyBoneLookup[nKeyboneLookup];
            for (int i = 0; i < nKeyboneLookup; i++)
            {
                keybonelookup[i] = bin.Read<KeyBoneLookup>();
            }
            return keybonelookup;
        }

        private Light[] readLights(uint nLights, uint ofsLights, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsLights;
            var lights = new Light[nLights];
            for (int i = 0; i < nLights; i++)
            {
                lights[i] = bin.Read<Light>();
            }
            return lights;
        }

        private ParticleEmitter[] readParticleEmitters(uint nParticleEmitters, uint ofsParticleEmitters, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsParticleEmitters;
            var particleEmitters = new ParticleEmitter[nParticleEmitters];
            for (int i = 0; i < nParticleEmitters; i++)
            {
                //Apparently really wrong. Who needs particles, right?
            }
            return particleEmitters;
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

        private WoWFormatLib.Structs.SKIN.SKIN[] readSkins(UInt32 num, String filename)
        {
            var skins = new WoWFormatLib.Structs.SKIN.SKIN[num];
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
                    skins[i] = skinreader.skin;
                }
            }
            return skins;
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

        private TexReplace[] readTexReplace(uint nTexReplace, uint ofsTexReplace, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsTexReplace;
            var texreplace = new TexReplace[nTexReplace];
            for (int i = 0; i < nTexReplace; i++)
            {
                texreplace[i] = bin.Read<TexReplace>();
            }
            return texreplace;
        }

        private List<string> readTextures(UInt32 num, UInt32 offset, BinaryReader bin)
        {
            bin.BaseStream.Position = offset;
            var textures = new List<String>();
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
                        textures.Add(filename);
                        if (!System.IO.File.Exists(System.IO.Path.Combine(basedir, filename)))
                        {
                            Console.WriteLine("BLP file does not exist!!! {0}", filename);
                            new WoWFormatLib.Utils.MissingFile(filename);
                        }
                    }
                    bin.BaseStream.Position = preFilenamePosition;
                }
            }
            return textures;
        }

        private TransLookup[] readTransLookup(uint nTransLookup, uint ofsTranslookup, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsTranslookup;
            var translookup = new TransLookup[nTransLookup];
            for (int i = 0; i < nTransLookup; i++)
            {
                translookup[i] = bin.Read<TransLookup>();
            }
            return translookup;
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

        private void readUnk1(uint nUnk1, uint ofsUnk1, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsUnk1;
            for (int i = 0; i < nUnk1; i++)
            {
                //wot
            }
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
    }
}