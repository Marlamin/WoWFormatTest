using System;
using System.IO;
using WoWFormatLib.Structs.ADT;
using WoWFormatLib.Utils;

namespace WoWFormatLib.FileReaders
{
    public class LODADTReader
    {
        public LODADT lodadt;
        public void LoadLODADT(string filename)
        {
            using (var adt = CASC.OpenFile(filename))
            using (var bin = new BinaryReader(adt))
            {
                long position = 0;

                while (position < adt.Length)
                {
                    adt.Position = position;

                    var chunkName = (ADTChunks)bin.ReadUInt32();
                    var chunkSize = bin.ReadUInt32();

                    position = adt.Position + chunkSize;

                    switch (chunkName)
                    {
                        case ADTChunks.MVER:
                            break;
                        case ADTChunks.MLHD: // Header
                            break;
                        case ADTChunks.MLVH: // Vertex Heights
                            lodadt.heights = ReadMLVHChunk(chunkSize, bin);
                            break;
                        case ADTChunks.MLVI: // Vertex Indices
                            lodadt.indices = ReadMLVIChunk(chunkSize, bin);
                            break;
                        case ADTChunks.MLLL: // LOD Levels
                            lodadt.lodLevels = ReadMLLLChunk(chunkSize, bin);
                            break;
                        case ADTChunks.MLND: // Quad tree stuff (?)
                            lodadt.quadTree = ReadMLNDChunk(chunkSize, bin);
                            break;
                        case ADTChunks.MLSI: // "Skirt" indices (?)
                            lodadt.skirtIndices = ReadMLSIChunk(chunkSize, bin);
                            break;
                        /* Model.blob */
                        case ADTChunks.MBMH:
                        case ADTChunks.MBBB:
                        case ADTChunks.MBMI:
                        case ADTChunks.MBNV:
                        case ADTChunks.MBMB:
                        /* Liquids */
                        case ADTChunks.MLLD:
                        case ADTChunks.MLLN:
                        case ADTChunks.MLLI:
                        case ADTChunks.MLLV:
                            break;
                        default:
                            throw new Exception(string.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunkName, position, filename));
                    }
                }
            }
        }

        private float[] ReadMLVHChunk(uint size, BinaryReader bin)
        {
            var count = size / 4;
            var mlvh = new float[count];

            for (var i = 0; i < count; i++)
            {
                mlvh[i] = bin.ReadSingle();
            }

            return mlvh;

        }
        private short[] ReadMLVIChunk(uint size, BinaryReader bin)
        {
            var count = size / 2;
            var mlvi = new short[count];

            for (var i = 0; i < count; i++)
            {
                mlvi[i] = bin.ReadInt16();
            }

            return mlvi;
        }

        private MLLLEntry[] ReadMLLLChunk(uint size, BinaryReader bin)
        {
            var count = size / 24;
            var mlll = new MLLLEntry[count];

            for (var i = 0; i < count; i++)
            {
                mlll[i] = bin.Read<MLLLEntry>();
            }

            return mlll;
        }

        private MLNDEntry[] ReadMLNDChunk(uint size, BinaryReader bin)
        {
            var count = size / 24;
            var mlnd = new MLNDEntry[count];

            for (var i = 0; i < count; i++)
            {
                mlnd[i] = bin.Read<MLNDEntry>();
            }

            return mlnd;
        }

        private short[] ReadMLSIChunk(uint size, BinaryReader bin)
        {
            var count = size / 2;
            var mlsi = new short[count];

            for (var i = 0; i < count; i++)
            {
                mlsi[i] = bin.ReadInt16();
            }

            return mlsi;
        }
    }
}
