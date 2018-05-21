using System;
using System.IO;
using WoWFormatLib.Structs.SKIN;
using WoWFormatLib.Utils;

namespace WoWFormatLib.FileReaders
{
    public class SKINReader
    {
        public SKIN skin;
        public void LoadSKIN(int fileDataID)
        {
            using (var bin = new BinaryReader(CASC.cascHandler.OpenFile(fileDataID)))
            {
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
                skin.bones = bin.ReadUInt32();
                skin.indices = ReadIndices(nIndices, ofsIndices, bin);
                skin.triangles = ReadTriangles(nTriangles, ofsTriangles, bin);
                skin.properties = ReadProperties(nProperties, ofsProperties, bin);
                skin.submeshes = ReadSubmeshes(nSubmeshes, ofsSubmeshes, bin);
                skin.textureunit = ReadTextureUnits(nTextureUnits, ofsTextureUnits, bin);
            }
        }
        private Indice[] ReadIndices(uint nIndices, uint ofsIndices, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsIndices;
            var indices = new Indice[nIndices];
            for (var i = 0; i < nIndices; i++)
            {
                indices[i] = bin.Read<Indice>();
            }
            return indices;
        }
        private Property[] ReadProperties(uint nProperties, uint ofsProperties, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsProperties;
            var properties = new Property[nProperties];
            for (var i = 0; i < nProperties; i++)
            {
                properties[i] = bin.Read<Property>();
            }
            return properties;
        }
        private Submesh[] ReadSubmeshes(uint nSubmeshes, uint ofsSubmeshes, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsSubmeshes;
            var submeshes = new Submesh[nSubmeshes];
            
            for (var i = 0; i < nSubmeshes; i++)
            {
                submeshes[i].submeshID = bin.ReadUInt16();
                submeshes[i].level = bin.ReadUInt16();
                submeshes[i].startVertex = bin.ReadUInt16();
                submeshes[i].nVertices = bin.ReadUInt16();
                submeshes[i].startTriangle = bin.ReadUInt16() + (uint)(submeshes[i].level << 16);
                submeshes[i].nTriangles = bin.ReadUInt16();
                submeshes[i].nBones = bin.ReadUInt16();
                submeshes[i].startBones = bin.ReadUInt16();
                submeshes[i].boneInfluences = bin.ReadUInt16();
                submeshes[i].centerBoneIndex = bin.ReadUInt16();
                submeshes[i].centerPosition = bin.Read<Vector3>();
                submeshes[i].sortCenterPosition = bin.Read<Vector3>();
                submeshes[i].sortRadius = bin.ReadSingle();
            }
            return submeshes;
        }
        private TextureUnit[] ReadTextureUnits(uint nTextureUnits, uint ofsTextureUnits, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsTextureUnits;
            var textureunits = new TextureUnit[nTextureUnits];
            for (var i = 0; i < nTextureUnits; i++)
            {
                textureunits[i] = bin.Read<TextureUnit>();
            }
            return textureunits;
        }
        private Triangle[] ReadTriangles(uint nTriangles, uint ofsTriangles, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsTriangles;
            var triangles = new Triangle[nTriangles / 3];
            for (var i = 0; i < nTriangles / 3; i++)
            {
                triangles[i] = bin.Read<Triangle>();
            }
            return triangles;
        }
    }
}
