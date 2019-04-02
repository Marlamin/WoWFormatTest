using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using WoWFormatLib.SereniaBLPLib;
using WoWFormatLib.Utils;

namespace WoWFormatLib.FileReaders
{
    public class BLPReader
    {
        public Bitmap bmp;

        public MemoryStream asBitmapStream()
        {
            var bitmapstream = new MemoryStream();
            bmp.Save(bitmapstream, ImageFormat.Bmp);
            return bitmapstream;
        }

        public void LoadBLP(uint fileDataID)
        {
            using (var blp = new BlpFile(CASC.OpenFile(fileDataID)))
            {
                bmp = blp.GetBitmap(0);
            }
        }

        public void LoadBLP(string filename)
        {
            using (var blp = new BlpFile(CASC.OpenFile(filename)))
            {
                bmp = blp.GetBitmap(0);
            }
        }

        public void LoadBLP(Stream file)
        {
            using (var blp = new BlpFile(file))
            {
                bmp = blp.GetBitmap(0);
            }
        }
    }
}
