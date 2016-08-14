using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWFormatLib.FileReaders;
using OpenTK.Graphics.OpenGL;
using WoWFormatLib.Utils;

namespace WoWOpenGL.Loaders
{
    class BLPLoader
    {
        public static int LoadTexture(string filename, CacheStorage cache)
        {
            if (cache.materials.ContainsKey(filename))
            {
                return cache.materials[filename];
            }

            var fileDataID = CASC.getFileDataIdByName(filename);

            return LoadTexture(fileDataID, cache);
        }

        public static int LoadTexture(int fileDataID, CacheStorage cache)
        {
            GL.ActiveTexture(TextureUnit.Texture0);

            if (cache.materials.ContainsKey(fileDataID.ToString()))
            {
                return cache.materials[fileDataID.ToString()];
            }

            int textureId = GL.GenTexture();

            var blp = new BLPReader();

            blp.LoadBLP(fileDataID);

            if (blp.bmp == null)
            {
                throw new Exception("BMP is null!");
            }
            else
            {
                GL.BindTexture(TextureTarget.Texture2D, textureId);
                cache.materials.Add(fileDataID.ToString(), textureId);
                System.Drawing.Imaging.BitmapData bmp_data = blp.bmp.LockBits(new System.Drawing.Rectangle(0, 0, blp.bmp.Width, blp.bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                blp.bmp.UnlockBits(bmp_data);
            }

            // Console.WriteLine("[CACHE ADD] " + filename);

            return textureId;
        }

        public static int GenerateAlphaTexture(byte[] values)
        {
            GL.ActiveTexture(TextureUnit.Texture1);

            int textureId = GL.GenTexture();

            var bmp = new System.Drawing.Bitmap(64, 64);

            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    var color = System.Drawing.Color.FromArgb(values[x * 64 + y], values[x * 64 + y], values[x * 64 + y], values[x * 64 + y]);
                    bmp.SetPixel(x, y, color);                   
                }
            }

            GL.BindTexture(TextureTarget.Texture2D, textureId);
            System.Drawing.Imaging.BitmapData bmp_data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);

            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int) TextureEnvMode.Modulate);

            bmp.UnlockBits(bmp_data);

           // bmp.Save("alphatest_" + textureId + ".bmp");

            GL.ActiveTexture(TextureUnit.Texture0);

            return textureId;
        }
    }
}
