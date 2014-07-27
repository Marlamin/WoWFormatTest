using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WoWFormatLib.Structs.SKIN;
using WoWFormatLib.Utils;

namespace WoWFormatLib.FileReaders
{
    public class SKINReader
    {
        private string basedir;
        public SKIN skin;

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
            FileStream stream = File.Open(Path.Combine(basedir, filename), FileMode.Open);
            BinaryReader bin = new BinaryReader(stream);

            var header = new string(bin.ReadChars(4));
            if (header != "SKIN")
            {
                Console.WriteLine("Invalid SKIN file!");
            }

            skin.filename = filename;
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

            stream.Close();
        }

        private Indice[] ReadIndices(uint nIndices, uint ofsIndices, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsIndices;
            var indices = new Indice[nIndices];
            for (int i = 0; i < nIndices; i++)
            {
                indices[i] = bin.Read<Indice>();
            }
            return indices;
        }
        private Triangle[] ReadTriangles(uint nTriangles, uint ofsTriangles, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsTriangles;
            var triangles = new Triangle[nTriangles / 3];
            for (int i = 0; i < nTriangles / 3; i++)
            {
                triangles[i] = bin.Read<Triangle>();
            }
            return triangles;
        }

        private Property[] ReadProperties(uint nProperties, uint ofsProperties, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsProperties;
            var properties = new Property[nProperties];
            for (int i = 0; i < nProperties; i++)
            {
                properties[i] = bin.Read<Property>();
            }
            return properties;
        }

        private Submesh[] ReadSubmeshes(uint nSubmeshes, uint ofsSubmeshes, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsSubmeshes;
            var submeshes = new Submesh[nSubmeshes];
            for (int i = 0; i < nSubmeshes; i++)
            {
                submeshes[i] = bin.Read<Submesh>();
            }
            return submeshes;
        }

        private TextureUnit[] ReadTextureUnits(uint nTextureUnits, uint ofsTextureUnits, BinaryReader bin)
        {
            bin.BaseStream.Position = ofsTextureUnits;
            var textureunits = new TextureUnit[nTextureUnits];
            for (int i = 0; i < nTextureUnits; i++)
            {
                textureunits[i] = bin.Read<TextureUnit>();
            }
            return textureunits;
        }
    }
}
