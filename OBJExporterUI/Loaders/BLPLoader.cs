using System;
using OpenTK.Graphics.OpenGL;
using WoWFormatLib.SereniaBLPLib;
using WoWFormatLib.Utils;

namespace OBJExporterUI.Loaders
{
    class BLPLoader
    {
        public static int LoadTexture(string filename, CacheStorage cache)
        {
            return LoadTexture(CASC.getFileDataIdByName(filename), cache);
        }

        public static int LoadTexture(uint filedataid, CacheStorage cache)
        {
            GL.ActiveTexture(TextureUnit.Texture0);

            if (cache.materials.ContainsKey(filedataid))
            {
                return cache.materials[filedataid];
            }

            int textureId = GL.GenTexture();

            using (var blp = new BlpFile(CASC.OpenFile(filedataid)))
            {
                var bmp = blp.GetBitmap(0);

                if (bmp == null)
                {
                    throw new Exception("BMP is null!");
                }

                GL.BindTexture(TextureTarget.Texture2D, textureId);
                cache.materials.Add(filedataid, textureId);
                var bmp_data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                bmp.UnlockBits(bmp_data);
            }

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
                    bmp.SetPixel(y, x, color);                   
                }
            }

            GL.BindTexture(TextureTarget.Texture2D, textureId);
            System.Drawing.Imaging.BitmapData bmp_data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int) TextureEnvMode.Modulate);

            bmp.UnlockBits(bmp_data);

           // bmp.Save("alphatest_" + textureId + ".bmp");

            GL.ActiveTexture(TextureUnit.Texture0);

            return textureId;
        }
    }
}
