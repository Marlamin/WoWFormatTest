using System;
using System.IO;
using System.Linq;
using WoWFormatLib.Structs.ADT;
using WoWFormatLib.Utils;

namespace WoWFormatLib.FileReaders
{
    public class LODADTReader
    {
        public LODADT lodadt;
        public void LoadLODADT(string filename)
        {
            using (var adt = CASC.cascHandler.OpenFile(filename))
            using (var bin = new BinaryReader(adt))
            {
                long position = 0;

                while (position < adt.Length)
                {
                    adt.Position = position;

                    var chunkName = new string(bin.ReadChars(4).Reverse().ToArray());
                    var chunkSize = bin.ReadUInt32();

                    position = adt.Position + chunkSize;

                    switch (chunkName)
                    {
                        case "MVER":
                            break;
                        case "MLHD": // Header
                            break;
                        case "MLVH": // Vertex Heights
                            lodadt.heights = ReadMLVHChunk(chunkSize, bin);
                            break;
                        case "MLVI": // Vertex Indices
                            lodadt.indices = ReadMLVIChunk(chunkSize, bin);
                            break;
                        case "MLLL": // LOD Levels
                            lodadt.lodLevels = ReadMLLLChunk(chunkSize, bin);
                            break;
                        case "MLND": // Quad tree stuff (?)
                            lodadt.quadTree = ReadMLNDChunk(chunkSize, bin);
                            break;
                        case "MLSI": // "Skirt" indices (?)
                            lodadt.skirtIndices = ReadMLSIChunk(chunkSize, bin);
                            break;
                        /* Model.blob */
                        case "MBMH":
                        case "MBBB":
                        case "MBMI":
                        case "MBNV":
                        case "MBMB":
                        /* Liquids */
                        case "MLLD":
                        case "MLLN":
                        case "MLLI":
                        case "MLLV":
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
