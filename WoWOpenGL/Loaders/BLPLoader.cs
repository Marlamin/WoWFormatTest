using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWFormatLib.FileReaders;
using OpenTK.Graphics.OpenGL;

namespace WoWOpenGL.Loaders
{
    class BLPLoader
    {
        public static int LoadTexture(string filename, CacheStorage cache)
        {
            filename = filename.ToLower();
            
            if (cache.materials.ContainsKey(filename))
            {
                Console.WriteLine("[CACHE HIT] " + filename);
                return cache.materials[filename];
            }

            Console.WriteLine("[CACHE MISS] " + filename);

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
                blp.bmp.UnlockBits(bmp_data);
            }

            Console.WriteLine("[CACHE ADD] " + filename);

            return textureId;
        }
    }
}
