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
using WoWRenderLib.Structs.WoWM2;

namespace WoWRenderLib
{
    public class M2Loader
    {
        public WoWM2 m2;
        private string basedir;
        private SharpDX.Direct3D11.Device device;
        private string modelPath;

        public M2Loader(string basedir, string modelPath, Device device)
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

        public void LoadM2()
        {
            //Load model
            M2Reader reader = new M2Reader(basedir);
            string filename = modelPath;
            reader.LoadM2(filename);

            //Load vertices
            List<float> verticelist = new List<float>();
            for (int i = 0; i < reader.model.vertices.Count(); i++)
            {
                verticelist.Add(reader.model.vertices[i].position.X);
                verticelist.Add(reader.model.vertices[i].position.Z * -1);
                verticelist.Add(reader.model.vertices[i].position.Y);
                verticelist.Add(1.0f);
                verticelist.Add(reader.model.vertices[i].normal.X);
                verticelist.Add(reader.model.vertices[i].normal.Z * -1);
                verticelist.Add(reader.model.vertices[i].normal.Y);
                verticelist.Add(reader.model.vertices[i].textureCoordX);
                verticelist.Add(reader.model.vertices[i].textureCoordY);
            }

            //Load indices
            List<ushort> indicelist = new List<ushort>();
            for (int i = 0; i < reader.model.skins[0].triangles.Count(); i++)
            {
                indicelist.Add(reader.model.skins[0].triangles[i].pt1);
                indicelist.Add(reader.model.skins[0].triangles[i].pt2);
                indicelist.Add(reader.model.skins[0].triangles[i].pt3);
            }

            //Convert to array
            ushort[] indices = indicelist.ToArray();
            float[] vertices = verticelist.ToArray();

            //Get texture, what a mess this could be much better

            M2Material[] materials = new M2Material[reader.model.textures.Count()];
            for (int i = 0; i < reader.model.textures.Count(); i++)
            {
                materials[i].flags = reader.model.textures[i].flags;

                var blp = new BLPReader(basedir);
                if (File.Exists(Path.Combine(basedir, reader.model.filename.Replace("M2", "blp"))))
                {
                    blp.LoadBLP(reader.model.filename.Replace("M2", "blp"));
                }
                else
                {
                    blp.LoadBLP(reader.model.textures[i].filename);
                }

                if (blp.bmp == null)
                {
                    materials[i].texture = Texture2D.FromFile<Texture2D>(device, "missingtexture.jpg");
                }
                else
                {
                    MemoryStream s = new MemoryStream();
                    blp.bmp.Save(s, System.Drawing.Imaging.ImageFormat.Png);
                    s.Seek(0, SeekOrigin.Begin);
                    materials[i].texture = Texture2D.FromMemory<Texture2D>(device, s.ToArray());
                }
            }

            M2RenderBatch[] renderbatches = new M2RenderBatch[reader.model.skins[0].submeshes.Count()];
            for (int i = 0; i < reader.model.skins[0].submeshes.Count(); i++)
            {
                renderbatches[i].firstFace = reader.model.skins[0].submeshes[i].startTriangle;
                renderbatches[i].numFaces = reader.model.skins[0].submeshes[i].nTriangles;
                for (int tu = 0; tu < reader.model.skins[0].textureunit.Count(); tu++)
                {
                    if (reader.model.skins[0].textureunit[tu].submeshIndex == i)
                    {
                        renderbatches[i].materialID = reader.model.skins[0].textureunit[tu].texture;
                    }
                }
            }

            m2.indices = indices;
            m2.vertices = vertices;
            m2.materials = materials;
            m2.renderBatches = renderbatches;
        }
    }
}