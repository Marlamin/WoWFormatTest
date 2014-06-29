using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WoWFormatLib.Utils;

namespace WoWFormatLib.FileReaders
{
    public class SKINReader
    {
        private string basedir;

        public SKINReader(string basedir)
        {
            this.basedir = basedir;
        }

        public void LoadSKIN(string filename)
        {
            filename = Path.ChangeExtension(filename, ".skin");
            if (!File.Exists(Path.Combine(basedir, filename)))
            {
                new WoWFormatLib.Utils.MissingFile(filename);
            }

           // Console.WriteLine("Reading " + filename);
            FileStream skin = File.Open(Path.Combine(basedir, filename), FileMode.Open);
            BinaryReader bin = new BinaryReader(skin);

            var header = new string(bin.ReadChars(4));
            if (header != "SKIN")
            {
                Console.WriteLine("Invalid SKIN file!");
            }

            var nIndices = bin.ReadUInt32();
            var ofsIndices = bin.ReadUInt32();
            var nTriangles = bin.ReadUInt32();
            var ofsTriangles = bin.ReadUInt32();
            var nProperties = bin.ReadUInt32();
            var ofsProperties = bin.ReadUInt32();
            var nSubmeshes = bin.ReadUInt32();
            var ofsSubmeshes = bin.ReadUInt32();
            var nTextureUnits = bin.ReadUInt32();
            var ofsTextureUnits = bin.ReadUInt32();
            var bones = bin.ReadUInt32();

            ReadIndices(nIndices, ofsIndices, bin);
            ReadTriangles(nTriangles, ofsTriangles, bin);
            ReadProperties(nProperties, ofsProperties, bin);
            ReadSubmeshes(nSubmeshes, ofsSubmeshes, bin);
            ReadTextureUnits(nTextureUnits, ofsTextureUnits, bin);
            

            skin.Close();
        }

        private void ReadIndices(uint nIndices, uint ofsIndices, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsIndices;
            for (int i = 0; i < nIndices; i++)
            {
                var vertex = bin.ReadUInt16();
            }
        }
        private void ReadTriangles(uint nTriangles, uint ofsTriangles, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsTriangles;
            for (int i = 0; i < (nTriangles / 3); i++)
            {
                ushort[] indices = new ushort[3];
                indices[0] = bin.ReadUInt16();
                indices[1] = bin.ReadUInt16();
                indices[2] = bin.ReadUInt16();
            }
        }

        private void ReadProperties(uint nProperties, uint ofsProperties, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsProperties;
            for (int i = 0; i < nProperties; i++)
            {
                byte[] properties = new byte[4];
                properties[0] = bin.ReadByte();
                properties[1] = bin.ReadByte();
                properties[2] = bin.ReadByte();
                properties[3] = bin.ReadByte();
            }
        }

        private void ReadSubmeshes(uint nSubmeshes, uint ofsSubmeshes, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsSubmeshes;
            for (int i = 0; i < nSubmeshes; i++)
            {
                var submeshID = bin.ReadUInt16();  
                var unk1 = bin.ReadUInt16();       
                var ofsVertex = bin.ReadUInt16();  
                var nVertices = bin.ReadUInt16();  
                var ofsTriangle = bin.ReadUInt16();
                var nTriangles = bin.ReadUInt16(); 
                var nBones = bin.ReadUInt16();     
                var ofsBones = bin.ReadUInt16();   
                var unk2 = bin.ReadUInt16();       
                var rootBone = bin.ReadUInt16();
                var centerMass = new Vector3(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                var centerBoundingBox = new Vector3(bin.ReadSingle(), bin.ReadSingle(), bin.ReadSingle());
                var radius = bin.ReadSingle();
            }
        }

        private void ReadTextureUnits(uint nTextureUnits, uint ofsTextureUnits, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsTextureUnits;
            for (int i = 0; i < nTextureUnits; i++)
            {
                var flags = bin.ReadUInt16();
                var shading = bin.ReadUInt16();
                var submeshIndex = bin.ReadUInt16();
                var submeshIndex2 = bin.ReadUInt16();
                var colorIndex = bin.ReadInt16();
                var renderFlags = bin.ReadUInt16();
                var texUnitNumber = bin.ReadUInt16();
                var mode = bin.ReadUInt16();
                var texture = bin.ReadUInt16();
                var texUnitNumber2 = bin.ReadUInt16();
                var transparency = bin.ReadUInt16();
                var textureAnim = bin.ReadUInt16();
            }
        }
    }
}
