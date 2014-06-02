using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using SharpDX.DXGI;
using Resource = SharpDX.Direct3D11.Resource;
using SharpDX.Direct3D11;

namespace WoWRenderTest
{
    [StructLayout(LayoutKind.Sequential)]
    struct BLP2Header
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] FourCC;

        public int Type;
        public byte Encoding;
        private byte AlphaDepth;
        private byte AlphaEncoding;
        private byte HasMips;
        public int Width;
        public int Height;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public int[] Offsets;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public int[] Lengths;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        private int[] Palette;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct DDSPixelFormat
    {
        public int Size;
        public DDPF Flags;
        public Magic FourCC;
        public int RGBBitCount;
        public uint RBitMask;
        public uint GBitMask;
        public uint BBitMask;
        public uint ABitMask;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct DDSHeader
    {
        public int Size;
        public DDSD Flags;
        public int Height;
        public int Width;
        public int PitchOrLinearSize;
        public int Depth;
        public int MipMapCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
        public int[] Reserved1;

        public DDSPixelFormat PixelFormat;
        public DDSCAPS Caps;
        public int Caps2;
        public int Caps3;
        public int Caps4;
        private int Reserved2;
    }

    internal struct DDSHeaderDXT10
    {
        public Format DXGiFormat;
        public ResourceDimension ResourceDimension;
        public uint MiscFlag;
        public uint ArraySize;
        public uint MiscFlags2;
    }

    class BLP
    {

        private byte[] _data;
        private int _width;
        private int _height;

        public BLP(MpqFile file)
        {
            file.Seek(0, SeekOrigin.Begin);
            var header = file.ReadStruct<BLP2Header>();
            var fcc = new string(header.FourCC);

            if (fcc != "BLP2")
                return;

            file.Seek(header.Offsets[0], SeekOrigin.Begin);
            _data = file.ReadBytes(header.Lengths[0]);
            _width = header.Width;
            _height = header.Height;
        }

        private const int DDSCapsTexture = 0x1000;
        private const int DDPFFourCC = 0x4;

        public byte[] ToDDS()
        {
            var file = new BinaryWriter(new MemoryStream());

            var header = new DDSHeader
            {
                Size = 124,
                Flags = DDSD.Caps | DDSD.Height | DDSD.Width | DDSD.PixelFormat,
                Caps = DDSCAPS.Texture,
                Height = _height,
                Width = _width,
                PixelFormat =
                {
                    Size = 32,
                    Flags = DDPF.FourCC,
                    FourCC = Magic.DXT1
                }
            };

            var buffer = new byte[Marshal.SizeOf(header)];
            GCHandle h = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            Marshal.StructureToPtr(header, h.AddrOfPinnedObject(), false);
            h.Free();

            file.Write((int)Magic.DDS);
            file.Write(buffer);
            file.Write(_data);

            file.Seek(0, SeekOrigin.Begin);
            return new BinaryReader(file.BaseStream).ReadBytes((int)file.BaseStream.Length);
        }
    }

    enum Magic
    {
        DDS = 0x20534444,
        DXT1 = 0x31545844,
        DX10 = 0x30315844
    }

    [Flags]
    enum DDSD
    {
        Caps = 0x1,
        Height = 0x2,
        Width = 0x4,
        Pitch = 0x8,
        Rgb = 0x40,
        PixelFormat = 0x1000
    };

    [Flags]
    enum DDSCAPS
    {
        Texture = 0x1000
    }

    [Flags]
    enum DDPF
    {
        AlphaPixels = 0x1,
        FourCC = 0x4,
        Rgb = 0x40
    }
}
