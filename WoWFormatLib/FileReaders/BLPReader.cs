using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SereniaBLPLib;
using System.IO;

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
                    bmp = blp.getBitmap(0);
                }
            }
            else
            {
                new Exception("BLP file " + filename + " does not exist!");
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
