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
                        case "MLHD": // Header
                        case "MLVH": // Vertex Heights
                        case "MLVI": // Vertex Indices
                            break;
                        case "MLLL": // LOD Levels
                            lodadt.lodLevels = ReadMLLLChunk(chunkSize, bin);
                            break;
                        case "MLND": // Quad tree stuff (?)
                        case "MLSI": // "Skirt" indices (?)
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
    }
}
