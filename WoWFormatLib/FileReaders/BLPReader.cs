using System.Drawing;
using System.Drawing.Imaging;

//using SereniaBLPLib;
using System.IO;
using WoWFormatLib.SereniaBLPLib;

namespace WoWFormatLib.FileReaders
{
    public class BLPReader
    {
        private string basedir;
        public Bitmap bmp;

        public BLPReader(string basedir)
        {
            this.basedir = basedir;
        }

        public void LoadBLP(string filename)
        {
            filename = Path.Combine(basedir, filename);
            if (File.Exists(filename))
            {
                using (var blp = new BlpFile(File.Open(filename, FileMode.Open)))
                {
                    bmp = blp.GetBitmap(0);
                }
            }
            else
            {
                new WoWFormatLib.Utils.MissingFile(filename);
            }
        }

        public MemoryStream asBitmapStream()
        {
            MemoryStream bitmapstream = new MemoryStream();
            bmp.Save(bitmapstream, ImageFormat.Bmp);
            return bitmapstream;
        }
    }
}