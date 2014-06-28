using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
            //Console.WriteLine("Number of indices: " + nIndices);
            var ofsIndices = bin.ReadUInt32();
            var nTriangles = bin.ReadUInt32();
            //Console.WriteLine("Number of triangles: " + nTriangles);
            var ofsTriangles = bin.ReadUInt32();
            var nProperties = bin.ReadUInt32();
            //Console.WriteLine("Number of properties: " + nProperties);
            var ofsProperties = bin.ReadUInt32();
            var nSubmeshes = bin.ReadUInt32();
            //Console.WriteLine("Number of submeshes: " + nSubmeshes);
            var ofsSubmeshes = bin.ReadUInt32();
            var nTextureUnits = bin.ReadUInt32();
            var ofsTextureUnits = bin.ReadUInt32();
            var bones = bin.ReadUInt32();

            ReadSubmeshes(nSubmeshes, ofsSubmeshes, bin);

            skin.Close();
        }

        private void ReadSubmeshes(uint nSubmeshes, uint ofsSubmeshes, BinaryReader bin)
        {
           // Console.WriteLine("Reading " + nSubmeshes.ToString() + " submeshes at " + ofsSubmeshes.ToString() + " ");
            bin.BaseStream.Position = ofsSubmeshes;
            for (int i = 0; i <= nSubmeshes; i++)
            {
                var submeshID = bin.ReadUInt16();
                bin.ReadBytes(46);
                //Console.WriteLine(submeshID);
                /*
                var unk1 = bin.ReadUInt16();
                var ofsVertex = bin.ReadUInt16();
                var nVertices = bin.ReadUInt16();
                var ofsTriangle = bin.ReadUInt16();
                var nTriangles = bin.ReadUInt16();
                var nBones = bin.ReadUInt16();
                var ofsBones = bin.ReadUInt16();
                var unk2 = bin.ReadUInt16();
                var rootBone = bin.ReadUInt16();
                bin.ReadSingle();
                bin.ReadSingle();
                bin.ReadSingle();
                bin.ReadSingle();
                bin.ReadSingle();
                bin.ReadSingle();
                var radius = bin.ReadUInt16();*/
            }
        }
    }
}
