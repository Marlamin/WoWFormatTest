using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
//using SereniaBLPLib;
using System.IO;
using WoWFormatLib.SereniaBLPLib;

namespace WoWFormatLib.FileReaders
{
    public class BLPReader
    {
        public Bitmap bmp;
        private string basedir;
        public bool useCASC;

        public BLPReader(string basedir)
        {
            this.basedir = basedir;
        }

        public MemoryStream asBitmapStream()
        {
            MemoryStream bitmapstream = new MemoryStream();
            bmp.Save(bitmapstream, ImageFormat.Bmp);
            return bitmapstream;
        }

        public void LoadBLP(string filename)
        {
            string fullpath = Path.Combine(basedir, filename);
            if (File.Exists(fullpath))
            {
                using (var blp = new BlpFile(File.Open(fullpath, FileMode.Open)))
                {
                    bmp = blp.GetBitmap(0);
                }
            }
            else
            {
                if (useCASC)
                {
                    Utils.CASC.DownloadFile(filename);
                    fullpath = Path.Combine("data", filename);
                }

                if (!File.Exists(fullpath))
                {
                    new WoWFormatLib.Utils.MissingFile(filename);
                }
                else
                {
                    using (var blp = new BlpFile(File.Open(fullpath, FileMode.Open)))
                    {
                        bmp = blp.GetBitmap(0);
                    }
                }
            }
        }

        public void LoadBLP(string[] filenames)
        {
            if (filenames.Length == 0)
                return;

            LoadBLP(filenames[0]);

            using (var canvas = Graphics.FromImage(bmp))
            {
                canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;

                for (int i = 1; i < filenames.Length; i++)
                {
                    if (!File.Exists(filenames[i]))
                        continue;

                    using (var blp = new BlpFile(File.Open(filenames[i], FileMode.Open)))
                    {
                        bmp = blp.GetBitmap(0);
                        canvas.DrawImage(bmp, 0, 0);
                    }
                }
                canvas.Save();
            }
        }
    }
}