using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWFormatLib.FileReaders;
using OpenTK.Graphics.OpenGL;
using WoWFormatLib.SereniaBLPLib;
using WoWFormatLib.Utils;

namespace OBJExporterUI.Loaders
{
    class BLPLoader
    {
        public static int LoadTexture(string filename, CacheStorage cache)
        {
            GL.ActiveTexture(TextureUnit.Texture0);

            filename = filename.ToLower();
            
            if (cache.materials.ContainsKey(filename))
            {
                return cache.materials[filename];
            }

            int textureId = GL.GenTexture();

            using (var blp = new BlpFile(CASC.cascHandler.OpenFile(filename)))
            {
                switch (blp.encoding)
                {
                    case 1:
                    case 2: // Temporary
                    case 3:
                        var bmp = blp.GetBitmap(0);

                        if (bmp == null)
                        {
                            throw new Exception("BMP is null!");
                        }

                        GL.BindTexture(TextureTarget.Texture2D, textureId);
                        cache.materials.Add(filename, textureId);
                        System.Drawing.Imaging.BitmapData bmp_data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                        bmp.UnlockBits(bmp_data);

                        break;
                    /*case 2:
                        DXTDecompression.DXTFlags flags = (blp.alphaDepth > 1) ? ((blp.alphaEncoding == 7) ? DXTDecompression.DXTFlags.DXT5 : DXTDecompression.DXTFlags.DXT3) : DXTDecompression.DXTFlags.DXT1;

                        var width = blp.width / (int)Math.Pow(2.0, blp.MipMapCount);
                        var height = blp.height / (int)Math.Pow(2.0, blp.MipMapCount);

                        int blockSize;
                        PixelInternalFormat format;
                        
                        if ((flags & DXTDecompression.DXTFlags.DXT1) != 0)
                        {
                            blockSize = 8;
                            format = PixelInternalFormat.CompressedRgbaS3tcDxt1Ext;
                        }
                        else if((flags & DXTDecompression.DXTFlags.DXT3) != 0)
                        {
                            blockSize = 16;
                            format = PixelInternalFormat.CompressedRgbaS3tcDxt3Ext;
                        }
                        else if((flags & DXTDecompression.DXTFlags.DXT5) != 0)
                        {
                            blockSize = 16;
                            format = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
                        }
                        else
                        {
                            throw new Exception("Unsupported DXT format!");
                        }

                        GL.BindTexture(TextureTarget.Texture2D, textureId);
                        cache.materials.Add(filename, textureId);

                        for (var i = blp.MipMapCount - 1; i >= 0; i--)
                        {
                            if ((width *= 2) == 0)
                            {
                                width = 1;
                            }

                            if ((height *= 2) == 0)
                            {
                                height = 1;
                            }

                            var size = ((width + 3) / 4) * ((height + 3) / 4) * blockSize;

                            var data = blp.GetPictureData(i);

                            GL.CompressedTexImage2D(TextureTarget.Texture2D, i, format, width, height, 0, size, );
                        }

                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

                        break;
                    */
                    default:
                        throw new Exception("BLP error!");
                }

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
