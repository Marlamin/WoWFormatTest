using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using WoWFormatLib.SereniaBLPLib;
using WoWFormatLib.Utils;

namespace WoWFormatLib.FileReaders
{
    public class BLPReader
    {
        public Bitmap bmp;

        public BLPReader()
        {
        }

        public MemoryStream asBitmapStream()
        {
            MemoryStream bitmapstream = new MemoryStream();
            bmp.Save(bitmapstream, ImageFormat.Bmp);
            return bitmapstream;
        }

        public void LoadBLP(string filename)
        {
            if (!CASC.FileExists(filename))
            {
                new WoWFormatLib.Utils.MissingFile(filename);

                // @TODO Quick fix to get texture working when it doesn't exist. Happened because Blizzard accidentally referenced a texture on their shares instead of in files.
                using (var blp = new BlpFile(CASC.OpenFile(@"World\Expansion05\Doodads\IronHorde\Ember_Offset_Streak.blp")))
                {
                    bmp = blp.GetBitmap(0);
                }
            }
            else
            {
                using (var blp = new BlpFile(CASC.OpenFile(filename)))
                {
                    bmp = blp.GetBitmap(0);
                }
            }
        }

        public void LoadBLP(Stream file) { 
            using (var blp = new BlpFile(file))
            {
                bmp = blp.GetBitmap(0);
            }
       }
    }
}