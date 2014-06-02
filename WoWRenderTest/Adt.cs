using System.Collections;
using System.Drawing.Imaging;
using System.IO;
using SharpDX;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using System;
using System.Collections.Generic;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.Toolkit.Graphics;
using Device = SharpDX.Direct3D11.Device;
using Resource = SharpDX.Direct3D11.Resource;
using Texture2D = SharpDX.Direct3D11.Texture2D;
using SamplerState = SharpDX.Direct3D11.SamplerState;
using Buffer = SharpDX.Direct3D11.Buffer;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace WoWRenderTest
{
    class Adt
    {
        public int X;
        public int Y;

        private float chunkSize = 1600 / 3f;
        private float border = 100 / 3f;

        public Vector4[] TerrainVertices
        {
            get
            {
                var vertices = new List<Vector4>();

                foreach (var mcnk in hora._info.MapChunks)
                {
                    vertices.AddRange(mcnk.TerrainVerticesColored);
                }

                return vertices.ToArray();
            }
        }

        public Vector4[] TerrainVerticesTextured
        {
            get
            {
                var vertices = new List<Vector4>();

                foreach (var mcnk in hora._info.MapChunks)
                {
                    vertices.AddRange(mcnk.TerrainVerticesTextured);
                }

                return vertices.ToArray();
            }
        }

        public  int[] TerrainIndices
        {
            get
            {
                return new[ ]
                {
                    1
                };
            }
        }

        public BoundingBox boundingBox;

        public Adt(string file, Device device)
        {
            var s = Path.GetFileName(file).Split(new[] { '_', '.', '\\' });

            int.TryParse(s[1], out X);

            int.TryParse(s[2], out Y);
            string map = s[0];

            var min = new Vector3((X - 32) * chunkSize - border, float.MinValue, (31-Y) * chunkSize - border);
            var max = new Vector3((X - 31) * chunkSize + border, float.MaxValue, (32-Y) * chunkSize + border);

            boundingBox = new BoundingBox(min, max);

            /*for (int y = Y - 1; y <= Y + 1; y++)
            {
                for (int x = X - 1; x <= X + 1; x++)
                {
                    new AdtFile(new Int2(x, y), map, boundingBox);
                }
            }*/

            hora = new AdtFile(new Point(X, Y), map, boundingBox, device);
        }

        public AdtFile hora;
    }

    struct AdtInfo
    {
        public MpqFile File;
        public int X;
        public int Y;
        public Device Device;
        public ShaderResourceView[] Textures;
        public List<int> Doodads;
        public List<int> Wmos;
        public MCNK[,] MapChunks;
    }

    internal class AdtFile
    {
        public MpqFile File;
        public AdtInfo _info;
        public List<M2> adtmodels = new List<M2>();
        public List<Wmo> wmo_models = new List<Wmo>();
        public int waterverticescount;
        
        public AdtFile(Point pos, string map, BoundingBox bbox, Device device)
        {
            File = new MpqFile(MpqArchive.Open(string.Format(@"World\Maps\{0}\{0}_{1}_{2}.adt", map, pos.X, pos.Y)));

            _info = new AdtInfo
            {
                File = File,
                X = pos.X,
                Y = pos.Y,
                Device = device,
                Doodads = new List<int>(),
                Wmos = new List<int>()
            };

            var mtex = new MTEX(GetChunkPosition("MTEX"), _info);
            _info.Textures = mtex.Textures;

            var mcin = new MCIN(GetChunkPosition("MCIN"), _info);

            _info.MapChunks = new MCNK[16,16];

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    _info.MapChunks[x, y] = mcin[x, y];
                }
            }

            var sampler = new SamplerState(device, new SamplerStateDescription
            {
                Filter = Filter.MinMagMipPoint,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                BorderColor = Color.Black,
                ComparisonFunction = Comparison.Never,
                MaximumAnisotropy = 16,
                MipLodBias = 0,
                MinimumLod = 0,
                MaximumLod = 16,
            });

            var sampler2 = new SamplerState(device, new SamplerStateDescription
            {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                BorderColor = Color.Black,
                ComparisonFunction = Comparison.Never,
                MaximumAnisotropy = 16,
                MipLodBias = 0,
                MinimumLod = 0,
                MaximumLod = 16,
            });

            device.ImmediateContext.PixelShader.SetSampler(0, sampler);
            device.ImmediateContext.PixelShader.SetSampler(1, sampler2);

            //Models

            _info.Doodads = _info.Doodads.Distinct().OrderBy(x => x).ToList();
            _info.Wmos = _info.Wmos.OrderBy(wmo => wmo).Distinct().ToList();

            var models = new MDDF(GetChunkPosition("MDDF"), _info);

            _info.File.Seek(GetChunkPosition("MMDX"), SeekOrigin.Begin);
            var header = _info.File.ReadStruct<ChunkHeader>();
            var files = _info.File.ReadString(header.Size - 1).Split(new[] { '\0' });

            foreach (var doodad in _info.Doodads)
            {
                var model = models[doodad];
                var vertices = M2.Parse(files[model.mmidEntry], model.position, model.rotation, model.Scale);
                if (vertices.Any() == false)
                    continue;
                adtmodels.Add(new M2(device, vertices));
            }

            var wmos = new MODF(GetChunkPosition("MODF"), _info);

            _info.File.Seek(GetChunkPosition("MWMO"), SeekOrigin.Begin);
            header = _info.File.ReadStruct<ChunkHeader>();
            files = _info.File.ReadString(header.Size - 1).Split(new[] { '\0' });

            foreach (var wmo in _info.Wmos)
            {
                var kuk = wmos[wmo];
                var vertices = Wmo.Parse(files[wmo], kuk.Position, kuk.Rotation);
                if (vertices.Any() == false)
                    continue;
                wmo_models.Add(new Wmo(device, vertices));
            }

            //Water

            var mh2o = new MH2O(GetChunkPosition("MH2O"), _info);

            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                    _info.MapChunks[x, y].LiquidInfo = mh2o.chunks[y * 16 + x];

            List<Vector4> watervertices = new List<Vector4>();
            Vector4 blue = new Vector4(0, 0, 1, 1);

            for (int chunkY = 0; chunkY < 16; chunkY++)
            {
                for (int chunkX = 0; chunkX < 16; chunkX++)
                {
                    //_info.MapChunks[x, y].LiquidInfo.Heights[0];

                    var chunk = _info.MapChunks[chunkX, chunkY];
                    if(chunk.LiquidInfo.LayerCount == 0)
                        continue;

                    for (int y = chunk.LiquidInfo.Y; y < chunk.LiquidInfo.Y + chunk.LiquidInfo.Height; y++)
                    {
                        for (int x = chunk.LiquidInfo.X; x < chunk.LiquidInfo.X + chunk.LiquidInfo.Width; x++)
                        {
                            watervertices.AddRange(new[]
                            {
                                new Vector4(chunk.OuterPositions[x + 1, y + 1].X, chunk.LiquidInfo.Heights[x, y], chunk.OuterPositions[x + 1, y + 1].Z, 1), blue,
                                new Vector4(chunk.OuterPositions[x + 1, y].X, chunk.LiquidInfo.Heights[x, y], chunk.OuterPositions[x + 1, y].Z, 1), blue,
                                new Vector4(chunk.OuterPositions[x, y].X, chunk.LiquidInfo.Heights[x, y], chunk.OuterPositions[x, y].Z, 1), blue,
                                new Vector4(chunk.OuterPositions[x, y + 1].X, chunk.LiquidInfo.Heights[x, y], chunk.OuterPositions[x, y + 1].Z, 1), blue,
                                new Vector4(chunk.OuterPositions[x + 1, y + 1].X, chunk.LiquidInfo.Heights[x, y], chunk.OuterPositions[x + 1, y + 1].Z, 1), blue,
                                new Vector4(chunk.OuterPositions[x, y].X, chunk.LiquidInfo.Heights[x, y], chunk.OuterPositions[x, y].Z, 1), blue
                            });
                        }
                    }
                }
            }

            Buffer water = new Buffer(device, 1000000 * Utilities.SizeOf<Vector4>() * 2, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, Utilities.SizeOf<Vector4>() * 2);
            DataStream stream;
            device.ImmediateContext.MapSubresource(water, MapMode.WriteNoOverwrite, MapFlags.None, out stream);

            stream.WriteRange(watervertices.ToArray());

            device.ImmediateContext.UnmapSubresource(water, 0);

            device.ImmediateContext.InputAssembler.SetVertexBuffers(3, new SharpDX.Direct3D11.VertexBufferBinding(water, Utilities.SizeOf<Vector4>() * 2, 0));

            waterverticescount = watervertices.Count / 2;

        }

        class MODF : AdtChunk
        {
            private MODFEntry[] _entries;
            public MODF(long position, AdtInfo info) : base(position, info)
            {
                info.File.Seek(position, SeekOrigin.Begin);

                var header = info.File.ReadStruct<ChunkHeader>();
                var num = header.Size / Utilities.SizeOf<MODFEntry>();

                _entries = new MODFEntry[num];

                for (int i = 0; i < num; i++)
                {
                    _entries[i] = info.File.ReadStruct<MODFEntry>();
                }
            }

            public MODFEntry this[int i]
            {
                get { return _entries[i]; }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MODFEntry
        {
            private int mwidEntry;
            private int uniqueId;
            private Vector3 _position;
            private Vector3 _rotation;
            private Vector3 lowerBounds;
            private Vector3 upperBounds;
            private short flags;
            private short doodadSet;
            private short nameSet;
            private short padding;

            public Vector3 Position
            {
                get
                {
                    float d = 1600 * 32 / 3f;
                    Vector3 v = new Vector3(_position.X - d, _position.Y, -(_position.Z - d));
                    return v;
                }
            }

            public Vector3 Rotation
            {
                get { return _rotation; }
            }
        }

        public long GetChunkPosition(string id)
        {
            File.Seek(0, SeekOrigin.Begin);
            var fcc = id.ToArray().Reverse();

            while (true)
            {
                var header = File.ReadStruct<ChunkHeader>();
                if (header.Size == 0)
                    break;

                if (header.Id.SequenceEqual(fcc))
                    return File.Position - 8;

                File.Seek(header.Size, SeekOrigin.Current);
            }

            return 0;
        }
    }

    internal class MDDF : AdtChunk, IEnumerable<MDDFEntry>
    {
        private readonly MDDFEntry[] _entries;

        public MDDF(long position, AdtInfo info)
            : base(position, info)
        {
            info.File.Seek(position, SeekOrigin.Begin);

            var header = info.File.ReadStruct<ChunkHeader>();
            var num = header.Size / Marshal.SizeOf(new MDDFEntry());
            _entries = new MDDFEntry[num];
            Count = num;

            for (int i = 0; i < num; i++)
            {
                _entries[i] = info.File.ReadStruct<MDDFEntry>();
            }
        }

        public int Count { get; private set; }

        public MDDFEntry this[int i]
        {
            get { return _entries[i]; }
        }

        #region Enumerator

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<MDDFEntry> GetEnumerator()
        {
            return new MDDFEnum(_entries.ToList());
        }

        internal class MDDFEnum : IEnumerator<MDDFEntry>
        {
            private readonly MDDFEntry[] _entries;
            private int _position = -1;

            public MDDFEnum(List<MDDFEntry> entries)
            {
                _entries = entries.ToArray();
            }

            public bool MoveNext()
            {
                _position++;
                return _position < _entries.Length;
            }

            public void Reset()
            {
                _position = -1;
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            MDDFEntry IEnumerator<MDDFEntry>.Current
            {
                get { return Current; }
            }

            public MDDFEntry Current
            {
                get { return _entries[_position]; }
            }

            public void Dispose()
            {
            }
        }

        #endregion
    }

    internal class MCNK : AdtChunk
    {
        private Vector4 color = new Vector4(0f, 1f, 0f, 1f);
        private Vector4 color2 = new Vector4(0f, .5f, 0f, 1f);
        private ShaderResourceView[ ] _textures;
        public MCLY mcly;
        private int vertexCount;
        //public MH2O water;
        public MH2OBlock LiquidInfo;

        public int StartIndex { get; set; }

        public void Render(Device device)
        {
            for (int i = 0; i < 4; i++)
            {
                device.ImmediateContext.PixelShader.SetShaderResource(i, _textures[i < mcly.NumLayers ? mcly.Layers[i].TextureId : 0]);
            }

            //if(_hasAlphaMap)
            device.ImmediateContext.PixelShader.SetShaderResource(4, mcal.maps);

            device.ImmediateContext.Draw(vertexCount, StartIndex);
        }

        const float step = 0.0625f;

        #region Terrain vertices, textured or colored

        public Vector4[ ] TerrainVerticesTextured
        {
            get
            {
                var vertices = new List<Vector4>();
                var holes = mcnkInfo.Holes;

                for (var y = 0; y < 8; y++)
                {
                    for (var x = 0; x < 8; x++)
                    {
                        if ((holes >> (y * 8 + x) & 1) == 1)
                        {
                            continue;
                        }

                        vertices.AddRange(new[ ]
                        {
                            new Vector4(OuterPositions[x + 1, y], 1), new Vector4(1, 0, _outerUV[x + 1, y].X, _outerUV[x + 1, y].Y),
                            new Vector4(OuterPositions[x, y], 1), new Vector4(0, 0, _outerUV[x, y].X, _outerUV[x, y].Y),
                            new Vector4(MiddlePositions[x, y], 1), new Vector4(.5f, .5f, _middleUV[x, y].X, _middleUV[x, y].Y),

                            new Vector4(OuterPositions[x + 1, y + 1], 1), new Vector4(1, 1, _outerUV[x + 1, y + 1].X, _outerUV[x + 1, y + 1].Y),
                            new Vector4(OuterPositions[x + 1, y], 1), new Vector4(1, 0, _outerUV[x + 1, y].X, _outerUV[x + 1, y].Y),
                            new Vector4(MiddlePositions[x, y], 1), new Vector4(.5f, .5f, _middleUV[x, y].X, _middleUV[x, y].Y),

                            new Vector4(OuterPositions[x, y + 1], 1), new Vector4(0, 1, _outerUV[x, y + 1].X, _outerUV[x, y + 1].Y),
                            new Vector4(OuterPositions[x + 1, y + 1], 1), new Vector4(1, 1, _outerUV[x + 1, y + 1].X, _outerUV[x + 1, y + 1].Y),
                            new Vector4(MiddlePositions[x, y], 1), new Vector4(.5f, .5f, _middleUV[x, y].X, _middleUV[x, y].Y),

                            new Vector4(OuterPositions[x, y], 1), new Vector4(0, 0, _outerUV[x, y].X, _outerUV[x, y].Y),
                            new Vector4(OuterPositions[x, y + 1], 1), new Vector4(0, 1, _outerUV[x, y + 1].X, _outerUV[x, y + 1].Y),
                            new Vector4(MiddlePositions[x, y], 1), new Vector4(.5f, .5f, _middleUV[x, y].X, _middleUV[x, y].Y)
                        });
                    }
                }

                vertexCount = vertices.Count / 2;

                if (vertexCount < 768)
                {
                    for (int i = 0; i < 768 - vertexCount; i++)
                    {
                        vertices.Add(Vector4.Zero);
                        vertices.Add(Vector4.Zero);
                    }
                }

                return vertices.ToArray();
            }
        }

        public Vector4[ ] TerrainVerticesColored
        {
            get
            {
                var vertices = new List<Vector4>();
                var holes = mcnkInfo.Holes;

                for (var y = 0; y < 8; y++)
                {
                    for (var x = 0; x < 8; x++)
                    {
                        if ((holes >> (y * 8 + x) & 1) == 1)
                        {
                            continue;
                        }

                        vertices.AddRange(new[ ]
                        {
                            new Vector4(OuterPositions[x + 1, y], 1f),
                            new Vector4(OuterPositions[x, y], 1f),
                            new Vector4(MiddlePositions[x, y], 1f),

                            new Vector4(OuterPositions[x + 1, y + 1], 1f),
                            new Vector4(OuterPositions[x + 1, y], 1f),
                            new Vector4(MiddlePositions[x, y], 1f),


                            new Vector4(OuterPositions[x, y + 1], 1f),
                            new Vector4(OuterPositions[x + 1, y + 1], 1f),
                            new Vector4(MiddlePositions[x, y], 1f),

                            new Vector4(OuterPositions[x, y], 1f),
                            new Vector4(OuterPositions[x, y + 1], 1f),
                            new Vector4(MiddlePositions[x, y], 1f)
                        });
                    }
                }

                vertexCount = vertices.Count;

                if (vertexCount < 768)
                {
                    for (int i = 0; i < 768 - vertexCount; i++)
                    {
                        vertices.Add(Vector4.Zero);
                    }
                }

                return vertices.ToArray();
            }
        }

        #endregion

        /*public int[ ] TerrainIndices
        {
            get
            {
                List<int> indices = new List<int>();

                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        indices.AddRange(new[ ]
                        {
                            x, x + 1, x + 4,
                            x + 1, x + 2, x + 4,
                            x + 2, x + 3, x + 4,
                            x + 3, x, x + 4
                        });
                    }
                    
                }

                return indices.ToArray();
            }
        }*/

        public Vector3[,] OuterPositions;
        public Vector3[,] MiddlePositions;

        private Vector2[,] _outerUV;
        private Vector2[,] _middleUV;

        private MCNKInfo mcnkInfo;

        public MCAL mcal;
        public MCVT HeightChunk;

        public bool HasAlphaMap = false;

        public MCNK(long offset, AdtInfo info)
            : base(offset, info)
        {
            info.File.Seek(offset + 8, SeekOrigin.Begin);
            mcnkInfo = info.File.ReadStruct<MCNKInfo>();
            HeightChunk = new MCVT(mcnkInfo.HeightOffset + Position, info);

            //var mcrf = new MCRF(mcnkInfo.RefOffset + Position, info);

            mcly = new MCLY(mcnkInfo.LayerOffset + Position, info);

            if (mcnkInfo.AlphaSize - 8 != 0)
            {
                HasAlphaMap = true;

                mcal = new MCAL(mcnkInfo.AlphaOffset + Position, info, mcly.Layers);
            }

            info.File.Seek(mcnkInfo.ReferencesOffset + Position + 8, SeekOrigin.Begin);
            for (int i = 0; i < mcnkInfo.DoodadReferencesCount; i++)
            {
                 info.Doodads.Add(info.File.ReadInt32());
            }

            for (int i = 0; i < mcnkInfo.MapObjReferencesCount; i++)
            {
                info.Wmos.Add(info.File.ReadInt32());
            }

            StartIndex = (mcnkInfo.IndexX * 16 + mcnkInfo.IndexY) * 768;

            _textures = info.Textures;

            OuterPositions = new Vector3[9,9];
            MiddlePositions = new Vector3[8,8];

            _outerUV = new Vector2[9, 9];
            _middleUV = new Vector2[8, 8];

            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    OuterPositions[x, y].X = x * 25 / 6f;
                    OuterPositions[x, y].Y = HeightChunk.Heights[(8 - y) * 17 + x];
                    OuterPositions[x, y].Z = y * 25 / 6f - 1600 / 48f;
                    OuterPositions[x, y] += mcnkInfo.Position;

                    /*_outerUV[x, y].X = x * 1 / 128f + 1 / 16f * mcnkInfo.IndexX;
                    _outerUV[x, y].Y = y * 1 / 128f + 1 / 16f * (15 - mcnkInfo.IndexY);*/

                    _outerUV[x, y].X = x * 1 / 8f;
                    _outerUV[x, y].Y = y * 1 / 8f;
                }
            }

            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    MiddlePositions[x, y].X = x * 25 / 6f + 25 / 12f;
                    MiddlePositions[x, y].Y = HeightChunk.Heights[(7 - y) * 17 + 9 + x];
                    MiddlePositions[x, y].Z = y * 25 / 6f + 25 / 12f - 1600 / 48f;
                    MiddlePositions[x, y] += mcnkInfo.Position;

                    /*_middleUV[x, y].X = x * 1 / 128f + 1 / 256f + 1 / 16f * mcnkInfo.IndexX;
                    _middleUV[x, y].Y = y * 1 / 128f + 1 / 256f + 1 / 16f * (15 - mcnkInfo.IndexY);*/

                    _middleUV[x, y].X = x * 1 / 8f + 1 / 16f;
                    _middleUV[x, y].Y = y * 1 / 8f + 1 / 16f;
                }
            }
        }
    }

    internal class MCRF : AdtChunk
    {
        public MCRF(long offset, AdtInfo info)
            : base(offset, info)
        {
        }
    }

    internal class MCAL : AdtChunk
    {
        public ShaderResourceView maps;
        public byte[ ][ ] alpha = { new byte[64 * 64], new byte[64 * 64], new byte[64 * 64] };
        public byte[] alpha2 = new byte[4096 * 3];

        public MCAL(long offset, AdtInfo info, MCLYLayer[] layers)
            : base(offset, info)
        {
            info.File.Seek(offset, SeekOrigin.Begin);
            var header = info.File.ReadStruct<ChunkHeader>();

            /*if(header.Size == 0)
                return;*/
            int numLayers = 0;

            foreach (var mclyLayer in layers)
            {
                if ((mclyLayer.Flags & 0x100) > 1)
                    numLayers++;
            }

            var size = header.Size / numLayers;
            
            for (int i = 0; i < numLayers; i++)
            {
                switch (size)
                {
                    case 2048:

                        byte b = 0;
                        for (int y = 0; y < 64; y++)
                        {
                            for (int x = 0; x < 64; x++)
                            {
                                if (x % 2 == 0)
                                {
                                    b = info.File.ReadBytes(1)[0];
                                    alpha[i][(63 - y) * 64 + x] = (byte)((b & 0xF) * 17);
                                }
                                else
                                {
                                    alpha[i][(63 - y) * 64 + x] = (byte)((b >> 4) * 17);
                                }
                            }
                        }

                        break;
                    case 4096:
                        throw new NotImplementedException();
                        //break;
                    default:
                        throw new NotImplementedException();
                        //break;
                }
            }

            DDSHeader dds = new DDSHeader
            {
                Size = 124,
                Flags = DDSD.Caps | DDSD.Height | DDSD.Width | DDSD.PixelFormat,
                Caps = DDSCAPS.Texture,
                Height = 64,
                Width = 64,
                PixelFormat = new DDSPixelFormat
                {
                    Size = 32,
                    Flags = DDPF.Rgb,
                    RGBBitCount = 24,
                    RBitMask = 0x00FF0000,
                    GBitMask = 0x0000FF00,
                    BBitMask = 0x000000FF
                }
            };

            var file = new BinaryWriter(new MemoryStream());

            var buffer = new byte[Marshal.SizeOf(dds)];
            GCHandle h = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            Marshal.StructureToPtr(dds, h.AddrOfPinnedObject(), false);
            h.Free();

            file.Write((int)Magic.DDS);
            file.Write(buffer);

            for (int i = 0; i < 64 * 64; i++)
            {
                file.Write(new[ ] { alpha[0][i], alpha[1][i], alpha[2][i] });

                //alpha2[i * 3 + 3] = byte.MaxValue;
                alpha2[i * 3] = alpha[0][i];
                alpha2[i * 3 + 1] = alpha[1][i];
                alpha2[i * 3 + 2] = alpha[2][i];
            }

            file.Seek(0, SeekOrigin.Begin);

            var bytes = new BinaryReader(file.BaseStream).ReadBytes((int)file.BaseStream.Length);
            maps = new ShaderResourceView(info.Device, Resource.FromMemory<Texture2D>(info.Device, bytes));
        }

        /*private Texture2D ToTexture(Device device, float[ ] alpha)
        {
            return null;
        }*/
    }

    internal class MCLY : AdtChunk
    {
        public int NumLayers { get; private set; }
        public MCLYLayer[] Layers { get; private set; }

        public MCLY(long offset, AdtInfo info)
            : base(offset, info)
        {
            info.File.Seek(offset, SeekOrigin.Begin);
            var header = info.File.ReadStruct<ChunkHeader>();

            NumLayers = header.Size / 16;
            Layers = new MCLYLayer[NumLayers];

            for (int i = 0; i < NumLayers; i++)
            {
                Layers[i] = info.File.ReadStruct<MCLYLayer>();
            }
        }
    }

    class MH2O : AdtChunk
    {
        public MH2OBlock[] chunks = new MH2OBlock[256];

        public MH2O(long position, AdtInfo info)
            : base(position, info)
        {
            info.File.Seek(position, SeekOrigin.Begin);
            var header = info.File.ReadStruct<ChunkHeader>();

            Blocks = new MH2OBlock[16, 16];

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
                        t.Y = (byte)(8 - hej.Height - hej.Y);
                        t.Level1 = hej.level1;
                        t.Level2 = hej.level2;
                        t.Width = hej.Width;
                        t.Height = hej.Height;
                        t.LayerCount = layer.LayerCount;

                        hej.HeightmapOffset += (int)position + 8;
                        hej.MaskOffset += (int)position + 8;

                        info.File.Seek(hej.HeightmapOffset, SeekOrigin.Begin);
                        if (hej.HeightmapOffset == 54246)
                            Console.WriteLine(hej.HeightmapOffset);
                        t.Heights = new float[8, 8];
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
    class MTEX : AdtChunk
    {
        private List<ShaderResourceView> _textures = new List<ShaderResourceView>();

        public MTEX(long offset, AdtInfo info)
            : base(offset, info)
        {
            _info.File.Seek(offset, SeekOrigin.Begin);
            var header = _info.File.ReadStruct<ChunkHeader>();

            string[] textures = _info.File.ReadString(header.Size - 1).Split(new[] { '\0' });

            foreach (var texture in textures)
            {
                var file = new MpqFile(MpqArchive.Open(texture));
                var blp = new BLP(file);
                _textures.Add(new ShaderResourceView(info.Device, Resource.FromMemory<Texture2D>(info.Device, blp.ToDDS())));
            }
        }

        public ShaderResourceView[] Textures
        {
            get { return _textures.ToArray(); }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MDDFEntry
    {
        public int mmidEntry;
        public int uniqueId;
        private Vector3 _position;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        private float[] _rotation;
        private short _scale;
        public short flags;

        public Vector3 position
        {
            get
            {
                float step = 1600 / 3f * 32;
                var ret = new Vector3(_position[0] - step, _position[1], -(_position[2] - step));
                return ret;
            }
        }

        public Vector3 rotation
        {
            get
            {
                return new Vector3(_rotation);
            }
        }

        public float Scale { get { return _scale / 1024f; } }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MCLYLayer
    {
        public int TextureId;
        public int Flags;
        public int Offset;
        public int Effect;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MCNKInfo
    {
        public int Flags;
        public int IndexX;
        public int IndexY;
        public int LayersCount;
        public int DoodadReferencesCount;
        public int HeightOffset;
        public int NormalOffset;
        public int LayerOffset;
        public int ReferencesOffset;
        public int AlphaOffset;
        public int AlphaSize;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        private readonly int[] _unknown;
        public int MapObjReferencesCount;
        private readonly int _holes;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        private int[ ] pad2;
        private Vector3 _position;

        public Vector3 Position
        {
            get { return new Vector3(-_position.Y, _position.Z, _position.X); }
        }

        public ulong Holes
        {
            get
            {
                ulong ret = 0;
                for (int y = 0; y < 4; y++)
                {
                    for (int x = 0; x < 4; x++)
                    {
                        if ((_holes >> (y * 4 + x) & 1) == 1)
                        {
                            ret |= (ulong) 3 << ((3 - y) * 16 + x * 2);
                            ret |= (ulong) 3 << ((3 - y) * 16 + 8 + x * 2);
                        }
                    }
                }
                return ret;
            }
        }
    }

    internal class MCVT : AdtChunk
    {
        public float[] Heights = new float[145];

        public MCVT(long offset, AdtInfo info) : base(offset, info)
        {
            info.File.Seek(offset+8, SeekOrigin.Begin);

            for (int i = 0; i < 145; i++)
            {
                Heights[i] = info.File.ReadSingle();
            }
        }
    }

    class AdtChunk
    {
        public long Position;
        protected AdtInfo _info;

        public AdtChunk(long offset, AdtInfo info)
        {
            _info = info;
            Position = offset;
        }
    }

    class MCIN
    {
        private long _offset;
        private MCINInfo2 _info = new MCINInfo2();
        private AdtInfo _adtInfo;
        public MCINInfo[] Information;

        public MCIN(long offset, AdtInfo info)
        {
            _offset = offset;
            info.File.Seek(offset+8, SeekOrigin.Begin);

            _adtInfo = info;

            _info = info.File.ReadStruct<MCINInfo2>();

            Information = new MCINInfo[256];

            for (int i = 0; i < 256; i++)
            {
                Information[i] = info.File.ReadStruct<MCINInfo>();
            }
        }

        public MCNK this[int x, int y]
        {
            get { return new MCNK(_info.Entries[y * 16 + x].Offset, _adtInfo); }
        }

    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ChunkHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] Id;
        public int Size;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MCINInfo2
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public MCINEntry[] Entries;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MCINInfo
    {
        public uint McnkOffset;
        public uint McnkSize;
        /*public uint Flags;
        public uint AsyncId;*/
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        private readonly int[] _unused;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MCINEntry
    {
        public uint Offset;
        public uint Size;
        public uint Flags;
        public uint Id;
    }
}