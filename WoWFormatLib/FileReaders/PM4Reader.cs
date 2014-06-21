using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using WoWFormatLib.Utils;

namespace WoWFormatLib.FileReaders
{
    public class PM4Reader
    {
      
        private string basedir;

        public PM4Reader(string basedir)
        {
            this.basedir = basedir;
        }

        public void LoadPM4(string filename)
        {
            using (FileStream pm4Stream = File.Open(basedir + filename, FileMode.Open))
            {
                ReadPM4(filename, pm4Stream);
            }
        }

        private void ReadPM4(string filename, FileStream pm4)
        {
            var bin = new BinaryReader(pm4);
            BlizzHeader chunk;

            long position = 0;
            while (position < pm4.Length)
            {
                pm4.Position = position;
                chunk = new BlizzHeader(bin.ReadChars(4), bin.ReadUInt32());
                chunk.Flip();
                position = pm4.Position + chunk.Size;

                switch (chunk.ToString())
                {

                    case "MVER":
                        UInt32 ver = bin.ReadUInt32();
                        Console.WriteLine("     MVER is " + ver.ToString() + " (size " + chunk.Size.ToString() + ")");
                        continue;
                    case "MCRC":
                        UInt32 crc = bin.ReadUInt32();
                        Console.WriteLine("     MCRC is " + crc.ToString() + " (size " + chunk.Size.ToString() + ")");
                        continue;
                    case "MSHD":
                        uint numuints = chunk.Size / 4;
                        Console.WriteLine("     MSHD contains " + numuints.ToString() + " uint32s" + " (size " + chunk.Size.ToString() + ")");
                        for (int i = 0; i < numuints; i++)
                        {
                            var temp = bin.ReadUInt32();
                            Console.WriteLine("         " + temp);
                        }
                        continue;
                    default:
                        Console.WriteLine("     " + chunk.ToString() + " (size " + chunk.Size.ToString() + ")");
                        break;
                }
            }
        }
    }
}
