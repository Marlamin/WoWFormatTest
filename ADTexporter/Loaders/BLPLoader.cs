using System;
using WoWFormatLib.FileReaders;
using OpenTK.Graphics.OpenGL;
using ADTexporter;

namespace ADTexporter.Loaders
{
    class BLPLoader
    {
        public static int LoadTexture(string filename, CacheStorage cache)
        {
            GL.ActiveTexture(TextureUnit.Texture0);

            filename = filename.ToLower();
            
            if (cache.materials.ContainsKey(filename))
            {
               // Console.WriteLine("[CACHE HIT] " + filename);
                return cache.materials[filename];
            }

            //Console.WriteLine("[CACHE MISS] " + filename);

            int textureId = GL.GenTexture();

            var blp = new BLPReader();

            blp.LoadBLP(filename);

            if (blp.bmp == null)
            {
                throw new Exception("BMP is null!");
            }
            else
            {
                GL.BindTexture(TextureTarget.Texture2D, textureId);
                cache.materials.Add(filename, textureId);
                System.Drawing.Imaging.BitmapData bmp_data = blp.bmp.LockBits(new System.Drawing.Rectangle(0, 0, blp.bmp.Width, blp.bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

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

            var data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, 64, 64), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            int stride = data.Stride;

            unsafe
            {
                byte* ptr = (byte*)data.Scan0;
                for (int x = 0; x < 64; x++)
                {
                    for (int y = 0; y < 64; y++)
                    {
                        var color = System.Drawing.Color.FromArgb(values[x * 64 + y], values[x * 64 + y], values[x * 64 + y], values[x * 64 + y]);

                        ptr[(y * 4) + x * stride] = color.B;
                        ptr[(y * 4) + x * stride + 1] = color.G;
                        ptr[(y * 4) + x * stride + 2] = color.R;
                        ptr[(y * 4) + x * stride + 3] = color.A;
                    }
                }
            }

            /*
            for (int x = 0; x < 64; x++)
            {
                for (int y = 0; y < 64; y++)
                {
                    bmp.SetPixel(y, x, color);                   
                }
            }
            */

            GL.BindTexture(TextureTarget.Texture2D, textureId);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);

            bmp.UnlockBits(data);

            //GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int) TextureEnvMode.Modulate);


            //bmp.Save("alphatest_" + textureId + ".bmp");

            GL.ActiveTexture(TextureUnit.Texture0);

            return textureId;
        }
    }
}
