using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;

namespace WoWFormatTest
{
    class WMOReader
    {
        public void LoadWMO(string filename)
        {
            string basedir = ConfigurationManager.AppSettings["basedir"];

            FileStream wdt = File.Open(basedir + filename, FileMode.Open);
            BinaryReader bin = new BinaryReader(wdt);
            BlizzHeader chunk;

            long position = 0;
            while (position < wdt.Length)
            {
                wdt.Position = position;
                chunk = new BlizzHeader(bin.ReadChars(4), bin.ReadUInt32());
                chunk.Flip();
                position = wdt.Position + chunk.Size;

                if (chunk.Is("MVER")) { if (bin.ReadUInt32() != 17) { throw new Exception("Unsupported WMO version!"); } continue; }
                if (chunk.Is("MOHD")) { continue; }
                if (chunk.Is("MOTX")) { continue; }
                if (chunk.Is("MOMT")) { continue; }
                if (chunk.Is("MOGN")) { continue; }
                if (chunk.Is("MOGI")) { continue; }
                if (chunk.Is("MOSB")) { continue; }
                if (chunk.Is("MOPV")) { continue; }
                if (chunk.Is("MOPT")) { continue; }
                if (chunk.Is("MOPR")) { continue; }
                if (chunk.Is("MOVV")) { continue; }
                if (chunk.Is("MOVB")) { continue; }
                if (chunk.Is("MOLT")) { continue; }
                if (chunk.Is("MODS")) { continue; }
                if (chunk.Is("MODN")) { continue; }
                if (chunk.Is("MODD")) { continue; }
                if (chunk.Is("MFOG")) { continue; }
                if (chunk.Is("MCVP")) { continue; }

                throw new Exception(String.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunk.ToString(), position.ToString(), filename));
            }

            wdt.Close();
        }
    }
}
