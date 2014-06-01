using System.IO;
using System.Runtime.InteropServices;
using System;

namespace WoWRenderTest
{
    class MH2O : AdtChunk
    {
        public MH2OBlock[] chunks = new MH2OBlock[256];

        public MH2O(long position, AdtInfo info)
            : base(position, info)
        {
            info.File.Seek(position, SeekOrigin.Begin);
            var header = info.File.ReadStruct<ChunkHeader>();

            Blocks = new MH2OBlock[16,16];

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    var layer = info.File.ReadStruct<MH2OHeader>();
                    if (layer.LayerCount > 0)
                    {
                        var pos = info.File.Position;

                        info.File.Seek(position + layer.DataOffset + 8, SeekOrigin.Begin);
                        var hej = info.File.ReadStruct<MH2OData>();

                        var t = new MH2OBlock();
                        t.Type = hej.Type;
                        //t.X = (byte) (8 - hej.Width - hej.X);
                        t.X = hej.X;
                        //t.Y = hej.Y;
                        t.Y = (byte) (8 - hej.Height - hej.Y);
                        t.Level1 = hej.level1;
                        t.Level2 = hej.level2;
                        t.Width = hej.Width;
                        t.Height = hej.Height;
                        t.LayerCount = layer.LayerCount;

                        hej.HeightmapOffset += (int) position + 8;
                        hej.MaskOffset += (int) position + 8;

                        info.File.Seek(hej.HeightmapOffset, SeekOrigin.Begin);
                        if (hej.HeightmapOffset == 54246)
                            Console.WriteLine(hej.HeightmapOffset);
                        t.Heights = new float[8,8];
                        for (int heightY = t.Y; heightY < t.Y + t.Height; heightY++)
                        {
                            for (int heightX = t.X; heightX < t.X + t.Width; heightX++)
                            {
                                //t.Heights[heightX, heightY] = info.File.ReadSingle();
                                /*if (t.Heights[heightX, heightY] == 0)
                                    t.Heights[heightX, heightY] = t.Level1;*/
                                t.Heights[heightX, heightY] = t.Level1;
                            }
                        }

                        /*for (int y2 = 0; y2 < 8; y2++)
                            for (int x2 = 0; x2 < 8; x2++)
                                t.Heights[x2, y2] = info.File.ReadSingle();*/

                        info.File.Seek(layer.RenderOffset + position + 8, SeekOrigin.Begin);
                        t.RenderMask = info.File.ReadInt64();

                        /*if (hej.MaskOffset > 0)
                        {
                            hej.MaskOffset += (int)position + 8;
                        }*/

                        chunks[y * 16 + x] = t;
                        Blocks[x, y] = t;

                        info.File.Seek(pos, SeekOrigin.Begin);
                    }
                }
            }
        }

        public MH2OBlock this[int i]
        {
            get
            {
                return chunks[i];
            }
        }

        public MH2OBlock[,] Blocks;
    }



    struct MH2OBlock
    {
        public MH2OData Data;
        public MH2OHeader Header;
        public short Type;
        public byte X, Y;
        public float Level1, Level2;
        public byte Width, Height;
        public long RenderMask;
        public float[,] Heights;
        public int LayerCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MH2OData
    {
        public short Type;
        public short Flags;
        public float level1;
        public float level2;
        public byte X;
        public byte Y;
        public byte Width;
        public byte Height;
        public int MaskOffset;
        public int HeightmapOffset;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MH2OHeader
    {
        public int DataOffset;
        public int LayerCount;
        public int RenderOffset;
    }
}