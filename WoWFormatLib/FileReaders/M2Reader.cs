using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            //vec3f[2] vertexbox
            //1
            bin.ReadSingle();
            bin.ReadSingle();
            bin.ReadSingle();
            //2
            bin.ReadSingle();
            bin.ReadSingle();
            bin.ReadSingle();
            var VertexRadius = bin.ReadSingle();
            //vec3f[2] boundingbox
            //1
            bin.ReadSingle();
            bin.ReadSingle();
            bin.ReadSingle();
            //2
            bin.ReadSingle();
            bin.ReadSingle();
            bin.ReadSingle();
            var BoundingRadius = bin.ReadSingle();
            var nBoundingTriangles = bin.ReadUInt32();
            var ofsBoundingTriangles = bin.ReadUInt32();
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
            var modelname = new string(bin.ReadChars(int.Parse(lenModelname.ToString())));

            readTextures(bin, nTextures, ofsTextures);
            readSkins(nViews, filename);

            m2.Close();
        }

        private void readTextures(BinaryReader bin, UInt32 num, UInt32 offset)
        {
            bin.BaseStream.Position = offset;
            for (int i = 0; i <= num; i++)
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
