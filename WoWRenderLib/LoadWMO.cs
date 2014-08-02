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
using WoWRenderLib.Structs.WoWWMO;

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

        public WoWWMOGroup LoadGroupWMO(WoWFormatLib.Structs.WMO.WMOGroupFile group)
        {
            List<float> verticelist = new List<float>();

            for (int i = 0; i < group.mogp.vertices.Count(); i++)
            {
                verticelist.Add(group.mogp.vertices[i].vector.X);
                verticelist.Add(group.mogp.vertices[i].vector.Z * -1);
                verticelist.Add(group.mogp.vertices[i].vector.Y);
                verticelist.Add(1.0f);
                verticelist.Add(group.mogp.normals[i].normal.X);
                verticelist.Add(group.mogp.normals[i].normal.Z * -1);
                verticelist.Add(group.mogp.normals[i].normal.Y);
                verticelist.Add(group.mogp.textureCoords[i].X);
                verticelist.Add(group.mogp.textureCoords[i].Y);
            }

            List<ushort> indicelist = new List<ushort>();

            for (int i = 0; i < group.mogp.indices.Count(); i++)
            {
                indicelist.Add(group.mogp.indices[i].indice);
            }

            WMORenderBatch[] renderBatches = new WMORenderBatch[group.mogp.renderBatches.Count()];

            for (int i = 0; i < group.mogp.renderBatches.Count(); i++)
            {
                renderBatches[i].firstFace = group.mogp.renderBatches[i].firstFace;
                renderBatches[i].numFaces = group.mogp.renderBatches[i].numFaces;
                renderBatches[i].materialID = group.mogp.renderBatches[i].materialID;
            }

            MaterialInfo[] materialInfo = new MaterialInfo[group.mogp.materialInfo.Count()];
            for (int i = 0; i < group.mogp.materialInfo.Count(); i++)
            {
                materialInfo[i].flags = group.mogp.materialInfo[i].flags;
                materialInfo[i].materialID = group.mogp.materialInfo[i].materialID;
            }

            WoWWMOGroup result = new WoWWMOGroup();

            result.indices = indicelist.ToArray();
            result.vertices = verticelist.ToArray();
            result.renderBatches = renderBatches;
            result.materialInfo = materialInfo;

            return result;
        }

        public void LoadWMO()
        {
            WMOReader reader = new WMOReader(basedir);
            reader.LoadWMO(modelPath);

            WMOMaterial[] materials = new WMOMaterial[reader.wmofile.materials.Count()];
            for (int i = 0; i < reader.wmofile.materials.Count(); i++)
            {
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

            WoWWMOGroup[] groups = new WoWWMOGroup[reader.wmofile.header.nGroups];

            for (int i = 0; i < reader.wmofile.header.nGroups; i++)
            {
                groups[i] = LoadGroupWMO(reader.wmofile.group[i]);
            }

            wmo.materials = materials;
            wmo.groups = groups;
        }
    }
}