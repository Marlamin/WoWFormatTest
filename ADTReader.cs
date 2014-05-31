using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WoWFormatTest
{
    class ADTReader
    {
        public void LoadADT(string basedir, string mapname, int x, int y)
        {
            Console.WriteLine("Loading " + y + "_" + x + " ADT for map " + mapname);
            string filename = basedir + "World\\Maps\\" + mapname + "\\" + mapname + "_" + y + "_" + x; // x and y are flipped
            FileStream adt = File.Open(filename + ".adt", FileMode.Open);
            FileStream adtobj0 = File.Open(filename + "_obj0.adt", FileMode.Open);
            FileStream adtobj1 = File.Open(filename + "_obj1.adt", FileMode.Open);
            FileStream adttex0 = File.Open(filename + "_tex0.adt", FileMode.Open);
            FileStream adttex1 = File.Open(filename + "_tex1.adt", FileMode.Open);
            BinaryReader bin = new BinaryReader(adt);
            BlizzHeader chunk;
            long position = 0;
            while (position < adt.Length)
            {
                adt.Position = position;
                chunk = new BlizzHeader(bin.ReadChars(4), bin.ReadUInt32());
                chunk.Flip();
                position = adt.Position + chunk.Size;

                if (chunk.Is("MVER")){ if (bin.ReadUInt32() != 18){throw new Exception("Unsupported ADT version!");} continue; }
                if (chunk.Is("MHDR")){ continue; }
                if (chunk.Is("MH2O")){ continue; }
                if (chunk.Is("MCNK")){ continue; }
                if (chunk.Is("MFBO")){ continue; }

                //model.blob stuff
                if (chunk.Is("MBMH")){ continue; }
                if (chunk.Is("MBBB")){ continue; }
                if (chunk.Is("MBMI")){ continue; }
                if (chunk.Is("MBNV")){ continue; }

                throw new Exception(String.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunk.ToString(), position.ToString(), filename));
            }

            bin = new BinaryReader(adtobj0);
            position = 0;
            Console.WriteLine("Loading " + y + "_" + x + " OBJ0 ADT for map " + mapname);

            while (position < adtobj0.Length)
            {
                adtobj0.Position = position;
                chunk = new BlizzHeader(bin.ReadChars(4), bin.ReadUInt32());
                chunk.Flip();
                position = adtobj0.Position + chunk.Size;

                if (chunk.Is("MVER")){ if (bin.ReadUInt32() != 18) { throw new Exception("Unsupported ADT version!"); } continue; }
                if (chunk.Is("MMDX")) { continue; }
                if (chunk.Is("MMID")) { continue; }
                if (chunk.Is("MWMO")) { continue; }
                if (chunk.Is("MWID")) { continue; }
                if (chunk.Is("MDDF")) { continue; }
                if (chunk.Is("MODF")) { continue; }
                if (chunk.Is("MCNK")) { continue; }

                throw new Exception(String.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunk.ToString(), position.ToString(), filename));
            }

            bin = new BinaryReader(adtobj1);
            position = 0;
            Console.WriteLine("Loading " + y + "_" + x + " OBJ1 ADT for map " + mapname);

            while (position < adtobj1.Length)
            {
                adtobj1.Position = position;
                chunk = new BlizzHeader(bin.ReadChars(4), bin.ReadUInt32());
                chunk.Flip();
                position = adtobj1.Position + chunk.Size;

                if (chunk.Is("MVER")){ if (bin.ReadUInt32() != 18) { throw new Exception("Unsupported ADT version!"); } continue; }
                if (chunk.Is("MMDX")) { continue; }
                if (chunk.Is("MMID")) { continue; }
                if (chunk.Is("MWMO")) { continue; }
                if (chunk.Is("MWID")) { continue; }
                if (chunk.Is("MDDF")) { continue; }
                if (chunk.Is("MODF")) { continue; }
                if (chunk.Is("MCNK")) { continue; }

                throw new Exception(String.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunk.ToString(), position.ToString(), filename));
            }

            bin = new BinaryReader(adttex0);
            position = 0;
            Console.WriteLine("Loading " + y + "_" + x + " TEX0 ADT for map " + mapname);

            while (position < adttex0.Length)
            {
                adttex0.Position = position;
                chunk = new BlizzHeader(bin.ReadChars(4), bin.ReadUInt32());
                chunk.Flip();
                position = adttex0.Position + chunk.Size;

                if (chunk.Is("MVER")) { if (bin.ReadUInt32() != 18) { throw new Exception("Unsupported ADT version!"); } continue; }
                if (chunk.Is("MAMP")) { continue; }
                if (chunk.Is("MTEX")) { continue; }
                if (chunk.Is("MCNK")) { continue; }
                if (chunk.Is("MTXP")) { continue; }

                throw new Exception(String.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunk.ToString(), position.ToString(), filename));
            }

            bin = new BinaryReader(adttex1);
            position = 0;
            Console.WriteLine("Loading " + y + "_" + x + " TEX1 ADT for map " + mapname);

            while (position < adttex1.Length)
            {
                adttex1.Position = position;
                chunk = new BlizzHeader(bin.ReadChars(4), bin.ReadUInt32());
                chunk.Flip();
                position = adttex1.Position + chunk.Size;

                if (chunk.Is("MVER")) { if (bin.ReadUInt32() != 18) { throw new Exception("Unsupported ADT version!"); } continue; }
                if (chunk.Is("MAMP")) { continue; }
                if (chunk.Is("MTEX")) { continue; }
                if (chunk.Is("MCNK")) { continue; }
                if (chunk.Is("MTXP")) { continue; }

                throw new Exception(String.Format("{2} Found unknown header at offset {1} \"{0}\" while we should've already read them all!", chunk.ToString(), position.ToString(), filename));
            }

        }
        
    }
}
