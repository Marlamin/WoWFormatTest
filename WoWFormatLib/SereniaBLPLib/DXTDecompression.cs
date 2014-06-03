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

// most of the algorithms and data used in this Class-file has been ported from LibSquish!
// http://code.google.com/p/libsquish/

using System;

namespace WoWFormatLib.SereniaBLPLib
{
    public class DXTDecompression
    {
        public enum DXTFlags
        {
            DXT1 = 1 << 0,
            DXT3 = 1 << 1,
            DXT5 = 1 << 2,
            // Additional Enums not implemented :o
        }

        private void Decompress(ref byte[] rgba, byte[] block, int flags)
        {
            // get the block locations
            byte[] colourBlock = new byte[8];
            byte[] alphaBlock = block;
            if ((flags & ((int)DXTFlags.DXT3 | (int)DXTFlags.DXT5)) != 0)
                Buffer.BlockCopy(block, 8, colourBlock, 0, 8);
            else
                Buffer.BlockCopy(block, 0, colourBlock, 0, 8);                

            // decompress color
            DecompressColor(ref rgba, colourBlock, ((flags & (int)DXTFlags.DXT1) != 0));

            // decompress alpha separately if necessary
            if ((flags & (int)DXTFlags.DXT3) != 0)
                DecompressAlphaDxt3(ref rgba, alphaBlock);
            else if ((flags & (int)DXTFlags.DXT5) != 0)
                DecompressAlphaDxt5(ref rgba, alphaBlock);

        }

        private void DecompressAlphaDxt3(ref byte[] rgba, byte[] block)
        {
            byte[] bytes = block;

            // Unpack the alpha values pairwise
            for (int i = 0; i < 8; i++)
            {
                // Quantise down to 4 bits
                byte quant = bytes[i];

                byte lo = (byte)(quant & 0x0F);
                byte hi = (byte)(quant & 0xF0);

                // Convert back up to bytes
                rgba[8 * i + 3] = (byte)(lo | (lo << 4));
                rgba[8 * i + 7] = (byte)(hi | (hi >> 4));
            }
        }

        private void DecompressAlphaDxt5(ref byte[] rgba, byte[] block)
        {
            // Get the two alpha values
            byte[] bytes = block;
            int alpha0 = bytes[0];
            int alpha1 = bytes[1];

            // compare the values to build the codebook
            byte[] codes = new byte[8];
            codes[0] = (byte)alpha0;
            codes[1] = (byte)alpha1;
            if (alpha0 <= alpha1)
            {
                // Use 5-Alpha Codebook
                for (int i = 1; i < 5; i++)
                    codes[1 + i] = (byte)(((5 - i) * alpha0 + i * alpha1) / 5);
                codes[6] = 0;
                codes[7] = 255;
            }
            else
            {
                // Use 7-Alpha Codebook
                for (int i = 1; i < 7; i++)
                {
                    codes[i + 1] = (byte)(((7 - i) * alpha0 + i * alpha1) / 7);
                }
            }

            // decode indices
            byte[] indices = new byte[16];
            byte[] blockSrc = bytes;
            int blockSrc_pos = 2;
            byte[] dest = indices;
            int indices_pos = 0;
            for (int i = 0; i < 2; i++)
            {
                // grab 3 bytes
                int value = 0;
                for (int j = 0; j < 3; j++)
                {
                    int _byte = blockSrc[blockSrc_pos++];
                    value |= (_byte << 8 * j);
                }

                // unpack 8 3-bit values from it
                for (int j = 0; j < 8; j++)
                {
                    int index = (value >> 3 * j) & 0x07;
                    dest[indices_pos++] = (byte)index;
                }
            }

            // write out the indexed coebook values
            for (int i = 0; i < 16; i++)
            {
                rgba[4 * i + 3] = codes[indices[i]];
            }
        }

        private void DecompressColor(ref byte[] rgba, byte[] block, bool isDxt1)
        {
            byte[] bytes = block;

            // Unpack Endpoints
            byte[] codes = new byte[16];
            int a = Unpack565(bytes, 0, ref codes, 0);
            int b = Unpack565(bytes, 2, ref codes, 4);

            // generate Midpoints
            for (int i = 0; i < 3; i++)
            {
                int c = codes[i];
                int d = codes[4 + i];

                if (isDxt1 && a <= b)
                {
                    codes[8 + i] = (byte)((c + d) / 2);
                    codes[12 + i] = 0;
                }
                else
                {
                    codes[8 + i] = (byte)((2 * c + d) / 3);
                    codes[12 + i] = (byte)((c + 2 * d) / 3);
                }
            }

            // Fill in alpha for intermediate values
            codes[8 + 3] = 255;
            codes[12 + 3] = (isDxt1 && a <= b) ? (byte)0 : (byte)255;

            //unpack the indices
            byte[] indices = new byte[16];
            for (int i = 0; i < 4; i++)
            {
                byte packed = bytes[4 + i];

                indices[0 + i * 4] = (byte)(packed & 0x3);
                indices[1 + i * 4] = (byte)((packed >> 2) & 0x3);
                indices[2 + i * 4] = (byte)((packed >> 4) & 0x3);
                indices[3 + i * 4] = (byte)((packed >> 6) & 0x3);
            }

            // store out the colours
            DoRGBAstuff(rgba, codes, indices);
        }

        private void DoRGBAstuff(byte[] rgba, byte[] codes, byte[] indices)
        {
            byte offset = (byte)(4 * indices[0]);
            rgba[0] = codes[offset++];
            rgba[1] = codes[offset++];
            rgba[2] = codes[offset++];
            rgba[3] = codes[offset];
            offset = (byte)(4 * indices[1]);
            rgba[4] = codes[offset++];
            rgba[5] = codes[offset++];
            rgba[6] = codes[offset++];
            rgba[7] = codes[offset];
            offset = (byte)(4 * indices[2]);
            rgba[8] = codes[offset++];
            rgba[9] = codes[offset++];
            rgba[10] = codes[offset++];
            rgba[11] = codes[offset];
            offset = (byte)(4 * indices[3]);
            rgba[12] = codes[offset++];
            rgba[12] = codes[offset++];
            rgba[14] = codes[offset++];
            rgba[15] = codes[offset];
            offset = (byte)(4 * indices[4]);
            rgba[16] = codes[offset++];
            rgba[17] = codes[offset++];
            rgba[18] = codes[offset++];
            rgba[19] = codes[offset];
            offset = (byte)(4 * indices[5]);
            rgba[20] = codes[offset++];
            rgba[21] = codes[offset++];
            rgba[22] = codes[offset++];
            rgba[23] = codes[offset];
            offset = (byte)(4 * indices[6]);
            rgba[24] = codes[offset++];
            rgba[25] = codes[offset++];
            rgba[26] = codes[offset++];
            rgba[27] = codes[offset];
            offset = (byte)(4 * indices[7]);
            rgba[28] = codes[offset++];
            rgba[29] = codes[offset++];
            rgba[30] = codes[offset++];
            rgba[31] = codes[offset];
            offset = (byte)(4 * indices[8]);
            rgba[32] = codes[offset++];
            rgba[33] = codes[offset++];
            rgba[34] = codes[offset++];
            rgba[35] = codes[offset];
            offset = (byte)(4 * indices[9]);
            rgba[36] = codes[offset++];
            rgba[37] = codes[offset++];
            rgba[38] = codes[offset++];
            rgba[39] = codes[offset];
            offset = (byte)(4 * indices[10]);
            rgba[40] = codes[offset++];
            rgba[41] = codes[offset++];
            rgba[42] = codes[offset++];
            rgba[43] = codes[offset];
            offset = (byte)(4 * indices[11]);
            rgba[44] = codes[offset++];
            rgba[45] = codes[offset++];
            rgba[46] = codes[offset++];
            rgba[47] = codes[offset];
            offset = (byte)(4 * indices[12]);
            rgba[48] = codes[offset++];
            rgba[49] = codes[offset++];
            rgba[50] = codes[offset++];
            rgba[51] = codes[offset];
            offset = (byte)(4 * indices[13]);
            rgba[52] = codes[offset++];
            rgba[53] = codes[offset++];
            rgba[54] = codes[offset++];
            rgba[55] = codes[offset];
            offset = (byte)(4 * indices[14]);
            rgba[56] = codes[offset++];
            rgba[57] = codes[offset++];
            rgba[58] = codes[offset++];
            rgba[59] = codes[offset];
            offset = (byte)(4 * indices[15]);
            rgba[60] = codes[offset++];
            rgba[61] = codes[offset++];
            rgba[62] = codes[offset++];
            rgba[63] = codes[offset];
        }

        private int Unpack565(byte[] packed, int packed_offset, ref byte[] colour, int colour_offset)
        {
            // Build packed value
            int value = (int)packed[0 + packed_offset] | ((int)packed[1 + packed_offset] << 8);

            // get components in the stored range
            byte red = (byte)((value >> 11) & 0x1F);
            byte green = (byte)((value >> 5) & 0x3F);
            byte blue = (byte)(value & 0x1F);

            // Scale up to 8 Bit
            colour[0 + colour_offset] = (byte)((red << 3) | (red >> 2));
            colour[1 + colour_offset] = (byte)((green << 2) | (green >> 4));
            colour[2 + colour_offset] = (byte)((blue << 3) | (blue >> 2));
            colour[3 + colour_offset] = 255;

            return value;
        }

        public byte[] DecompressImage(int width, int height, byte[] data, int flags)
        {
            byte[] rgba = new byte[width * height * 4];

            // initialise the block input
            byte[] sourceBlock = data;
            int sourceBlock_pos = 0;
            int bytesPerBlock = ((flags & (int)DXTFlags.DXT1) != 0) ? 8 : 16;

            // loop over blocks
            for (int y = 0; y < height; y += 4)
            {
                for (int x = 0; x < width; x += 4)
                {
                    // decompress the block
                    byte[] targetRGBA = new byte[4 * 16];
                    int targetRGBA_pos = 0;
                    byte[] sourceBlockBuffer = new byte[bytesPerBlock]; // größe korrekt?
                    if (sourceBlock.Length == sourceBlock_pos) continue;
                    Buffer.BlockCopy(sourceBlock, sourceBlock_pos, sourceBlockBuffer, 0, bytesPerBlock);
                    //sourceBlock.CopyTo(sourceBlockBuffer, sourceBlock_pos);
                    Decompress(ref targetRGBA, sourceBlockBuffer, flags);

                    // Write the decompressed pixels to the correct image locations
                    byte[] sourcePixel = new byte[4];

                    for (int py = 0; py < 4; py++)
                    {
                        for (int px = 0; px < 4; px++)
                        {
                            int sx = x + px;
                            int sy = y + py;
                            if (sx < width && sy < height)
                            {
                                int targetPixel = 4 * (width * sy + sx);

                                //targetRGBA.CopyTo(sourcePixel, targetRGBA_pos);
                                Buffer.BlockCopy(targetRGBA, targetRGBA_pos, sourcePixel, 0, 4);
                                targetRGBA_pos += 4;

                                for (int i = 0; i < 4; i++)
                                    rgba[targetPixel + i] = sourcePixel[i];
                            }
                            else
                            {
                                // Ignore that pixel
                                targetRGBA_pos += 4;
                            }
                        }
                    }
                    sourceBlock_pos += bytesPerBlock;
                }
            }
            return rgba;
        }
    }
}