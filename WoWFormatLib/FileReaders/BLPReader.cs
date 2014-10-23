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
                return;
            }
            else
            {
                using (var blp = new BlpFile(CASC.OpenFile(filename)))
                {
                    bmp = blp.GetBitmap(0);
                }
            }
        }
    }
}