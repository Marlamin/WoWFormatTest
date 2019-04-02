/*
 * Copyright (c) <2011> <by Xalcon @ mmowned.com-Forum>
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace WoWFormatLib.SereniaBLPLib
{
    // Some Helper Struct to store Color-Data
    public struct ARGBColor8
    {
        public byte red;
        public byte green;
        public byte blue;
        public byte alpha;

        /// <summary>
        /// Converts the given Pixel-Array into the BGRA-Format
        /// This will also work vice versa
        /// </summary>
        /// <param name="pixel"></param>
        public static void ConvertToBGRA(byte[] pixel)
        {
            byte tmp = 0;
            for (int i = 0; i < pixel.Length; i += 4)
            {
                tmp = pixel[i]; // store red
                pixel[i] = pixel[i + 2]; // Write blue into red
                pixel[i + 2] = tmp; // write stored red into blue
            }
        }
    }

    public sealed class BlpFile : IDisposable
    {
        private readonly uint formatVersion; // compression: 0 = JPEG Compression, 1 = Uncompressed or DirectX Compression
        private readonly byte colorEncoding; // 1 = Uncompressed, 2 = DirectX Compressed
        private readonly byte alphaDepth; // 0 = no alpha, 1 = 1 Bit, 4 = Bit (only DXT3), 8 = 8 Bit Alpha
        private readonly byte alphaEncoding; // 0: DXT1 alpha (0 or 1 Bit alpha), 1 = DXT2/3 alpha (4 Bit), 7: DXT4/5 (interpolated alpha)
        private readonly byte hasMipmaps; // If true (1), then there are Mipmaps
        private readonly int width; // X Resolution of the biggest Mipmap
        private readonly int height; // Y Resolution of the biggest Mipmap

        private readonly uint[] mipmapOffsets = new uint[16]; // Offset for every Mipmap level. If 0 = no more mitmap level
        private readonly uint[] mipMapSize = new uint[16]; // Size for every level
        private readonly ARGBColor8[] paletteBGRA = new ARGBColor8[256]; // The color-palette for non-compressed pictures

        private Stream str; // Reference of the stream

        /// <summary>
        /// Extracts the palettized Image-Data from the given Mipmap and returns a byte-Array in the 32Bit RGBA-Format
        /// </summary>
        /// <param name="mipmap">The desired Mipmap-Level. If the given level is invalid, the smallest available level is choosen</param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="data"></param>
        /// <returns>Pixel-data</returns>
        private byte[] GetPictureUncompressedByteArray(int w, int h, byte[] data)
        {
            int length = w * h;
            byte[] pic = new byte[length * 4];
            for (int i = 0; i < length; i++)
            {
                pic[i * 4] = paletteBGRA[data[i]].red;
                pic[i * 4 + 1] = paletteBGRA[data[i]].green;
                pic[i * 4 + 2] = paletteBGRA[data[i]].blue;
                pic[i * 4 + 3] = GetAlpha(data, i, length);
            }
            return pic;
        }

        private byte GetAlpha(byte[] data, int index, int alphaStart)
        {
            switch (alphaDepth)
            {
                default:
                    return 0xFF;
                case 1:
                    {
                        byte b = data[alphaStart + (index / 8)];
                        return (byte)((b & (0x01 << (index % 8))) == 0 ? 0x00 : 0xff);
                    }
                case 4:
                    {
                        byte b = data[alphaStart + (index / 2)];
                        return (byte)(index % 2 == 0 ? (b & 0x0F) << 4 : b & 0xF0);
                    }
                case 8:
                    return data[alphaStart + index];
            }
        }

        /// <summary>
        /// Returns the raw Mipmap-Image Data. This data can either be compressed or uncompressed, depending on the Header-Data
        /// </summary>
        /// <param name="mipmapLevel"></param>
        /// <returns></returns>
        private byte[] GetPictureData(int mipmapLevel)
        {
            if (str != null)
            {
                byte[] data = new byte[mipMapSize[mipmapLevel]];
                str.Position = mipmapOffsets[mipmapLevel];
                str.Read(data, 0, data.Length);
                return data;
            }
            return null;
        }

        /// <summary>
        /// Returns the amount of Mipmaps in this BLP-File
        /// </summary>
        public int MipMapCount
        {
            get
            {
                int i = 0;
                while (mipmapOffsets[i] != 0)
                    i++;
                return i;
            }
        }

        public BlpFile(Stream stream)
        {
            str = stream;

            using (BinaryReader br = new BinaryReader(stream, Encoding.ASCII, true))
            {
                // Checking for correct Magic-Code
                if (br.ReadUInt32() != 0x32504c42)
                    throw new Exception("Invalid BLP Format");

                // Reading type
                formatVersion = br.ReadUInt32();

                if (formatVersion != 1)
                    throw new Exception("Invalid BLP-Type! Should be 1 but " + formatVersion + " was found");

                // Reading encoding, alphaBitDepth, alphaEncoding and hasMipmaps
                colorEncoding = br.ReadByte();
                alphaDepth = br.ReadByte();
                alphaEncoding = br.ReadByte();
                hasMipmaps = br.ReadByte();

                // Reading width and height
                width = br.ReadInt32();
                height = br.ReadInt32();

                // Reading MipmapOffset Array
                for (int i = 0; i < 16; i++)
                    mipmapOffsets[i] = br.ReadUInt32();

                // Reading MipmapSize Array
                for (int i = 0; i < 16; i++)
                    mipMapSize[i] = br.ReadUInt32();

                // When encoding is 1, there is no image compression and we have to read a color palette
                if (colorEncoding == 1)
                {
                    // Reading palette
                    for (int i = 0; i < 256; i++)
                    {
                        int color = br.ReadInt32();
                        paletteBGRA[i].blue = (byte)((color >> 0) & 0xFF);
                        paletteBGRA[i].green = (byte)((color >> 8) & 0xFF);
                        paletteBGRA[i].red = (byte)((color >> 16) & 0xFF);
                        paletteBGRA[i].alpha = (byte)((color >> 24) & 0xFF);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the uncompressed image as a bytarray in the 32pppRGBA-Format
        /// </summary>
        private byte[] GetImageBytes(int w, int h, byte[] data)
        {
            switch (colorEncoding)
            {
                case 1:
                    return GetPictureUncompressedByteArray(w, h, data);
                case 2:
                    DXTDecompression.DXTFlags flag = (alphaDepth > 1) ? ((alphaEncoding == 7) ? DXTDecompression.DXTFlags.DXT5 : DXTDecompression.DXTFlags.DXT3) : DXTDecompression.DXTFlags.DXT1;
                    return DXTDecompression.DecompressImage(w, h, data, flag);
                case 3:
                    return data;
                default:
                    return new byte[0];
            }
        }

        /// <summary>
        /// Converts the BLP to a System.Drawing.Bitmap
        /// </summary>
        /// <param name="mipmapLevel">The desired Mipmap-Level. If the given level is invalid, the smallest available level is choosen</param>
        /// <returns>The Bitmap</returns>
        public Bitmap GetBitmap(int mipmapLevel)
        {
            byte[] pic = GetPixels(mipmapLevel, out int w, out int h, colorEncoding == 3 ? false : true);

            Bitmap bmp = new Bitmap(w, h);

            // Faster bitmap Data copy
            BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(pic, 0, bmpdata.Scan0, pic.Length); // copy! :D
            bmp.UnlockBits(bmpdata);

            return bmp;
        }

        /// <summary>
        /// Returns array of pixels in BGRA or RGBA order
        /// </summary>
        /// <param name="mipmapLevel"></param>
        /// <returns></returns>
        public byte[] GetPixels(int mipmapLevel, out int w, out int h, bool bgra = true)
        {
            if (mipmapLevel >= MipMapCount)
                mipmapLevel = MipMapCount - 1;
            if (mipmapLevel < 0)
                mipmapLevel = 0;

            int scale = (int)Math.Pow(2, mipmapLevel);
            w = width / scale;
            h = height / scale;

            byte[] data = GetPictureData(mipmapLevel);
            byte[] pic = GetImageBytes(w, h, data); // This bytearray stores the Pixel-Data

            if (bgra)
            {
                // when we want to copy the pixeldata directly into the bitmap, we have to convert them into BGRA before doing so
                ARGBColor8.ConvertToBGRA(pic);
            }

            return pic;
        }

        /// <summary>
        /// Runs close()
        /// </summary>
        public void Dispose()
        {
            Close();
        }

        /// <summary>
        /// Closes the Memorystream
        /// </summary>
        public void Close()
        {
            if (str != null)
            {
                str.Close();
                str = null;
            }
        }
    }
}
