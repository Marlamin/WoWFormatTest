using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.WPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWFormatLib.FileReaders;
using WoWRenderLib.Structs;

namespace WoWRenderLib
{
    public class WMOLoader
    {
        public WoWWMO wmo;
        private string basedir;
        private SharpDX.Direct3D11.Device device;
        private string modelPath;

        public WMOLoader(string basedir, string modelPath, Device device)
        {
            this.basedir = basedir;
            this.modelPath = modelPath;
            this.device = device;
            //if (modelPath.EndsWith(".m2", StringComparison.OrdinalIgnoreCase))
            // {
            //LoadM2();
            //}
            //else if (modelPath.EndsWith(".wmo", StringComparison.OrdinalIgnoreCase))
            //{
            //}
        }

        public void LoadWMO()
        {
            WMOReader reader = new WMOReader(basedir);
            reader.LoadWMO(modelPath);
            List<float> verticelist = new List<float>();
            for (int i = 0; i < reader.wmofile.group[0].mogp.vertices.Count(); i++)
            {
                verticelist.Add(reader.wmofile.group[0].mogp.vertices[i].vector.X);
                verticelist.Add(reader.wmofile.group[0].mogp.vertices[i].vector.Z * -1);
                verticelist.Add(reader.wmofile.group[0].mogp.vertices[i].vector.Y);
                verticelist.Add(1.0f);
                verticelist.Add(reader.wmofile.group[0].mogp.normals[i].normal.X);
                verticelist.Add(reader.wmofile.group[0].mogp.normals[i].normal.Z * -1);
                verticelist.Add(reader.wmofile.group[0].mogp.normals[i].normal.Y);
                verticelist.Add(reader.wmofile.group[0].mogp.textureCoords[i].X);
                verticelist.Add(reader.wmofile.group[0].mogp.textureCoords[i].Y);
            }

            List<ushort> indicelist = new List<ushort>();
            for (int i = 0; i < reader.wmofile.group[0].mogp.indices.Count(); i++)
            {
                indicelist.Add(reader.wmofile.group[0].mogp.indices[i].indice);
            }

            RenderBatch[] renderBatches = new RenderBatch[reader.wmofile.group[0].mogp.renderBatches.Count()];

            for (int i = 0; i < reader.wmofile.group[0].mogp.renderBatches.Count(); i++)
            {
                renderBatches[i].firstFace = reader.wmofile.group[0].mogp.renderBatches[i].firstFace;
                renderBatches[i].numFaces = reader.wmofile.group[0].mogp.renderBatches[i].numFaces;
                renderBatches[i].materialID = reader.wmofile.group[0].mogp.renderBatches[i].materialID;
            }

            MaterialInfo[] materialInfo = new MaterialInfo[reader.wmofile.group[0].mogp.materialInfo.Count()];
            for (int i = 0; i < reader.wmofile.group[0].mogp.materialInfo.Count(); i++)
            {
                materialInfo[i].flags = reader.wmofile.group[0].mogp.materialInfo[i].flags;
                materialInfo[i].materialID = reader.wmofile.group[0].mogp.materialInfo[i].materialID;
            }

            WMOMaterial[] materials = new WMOMaterial[reader.wmofile.materials.Count()];
            for (int i = 0; i < reader.wmofile.materials.Count(); i++)
            {
                Console.WriteLine(reader.wmofile.group[0].mogp.materialInfo[i].materialID);
                for (int ti = 0; ti < reader.wmofile.textures.Count(); ti++)
                {
                    if (reader.wmofile.textures[ti].startOffset == reader.wmofile.materials[i].texture1)
                    {
                        Texture2D texture;
                        var blp = new BLPReader(basedir);
                        blp.LoadBLP(reader.wmofile.textures[ti].filename);
                        if (blp.bmp == null)
                        {
                            texture = Texture2D.FromFile<Texture2D>(device, "missingtexture.jpg");
                        }
                        else
                        {
                            MemoryStream s = new MemoryStream();
                            blp.bmp.Save(s, System.Drawing.Imaging.ImageFormat.Png);
                            s.Seek(0, SeekOrigin.Begin);
                            texture = Texture2D.FromMemory<Texture2D>(device, s.ToArray());
                            s.Dispose();
                        }
                        materials[i].materialID = (uint)i;
                        materials[i].filename = reader.wmofile.textures[ti].filename;
                        materials[i].texture = texture;
                    }
                }
            }

            wmo.indices = indicelist.ToArray();
            wmo.vertices = verticelist.ToArray();
            wmo.renderBatches = renderBatches;
            wmo.materialInfo = materialInfo;
            wmo.materials = materials;
        }
    }
}