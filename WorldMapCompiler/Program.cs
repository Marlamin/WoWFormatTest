using System.Drawing;
using System.IO;
using WoWFormatLib.SereniaBLPLib;

namespace WorldMapCompiler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var mapName = "Drustvar";
            var i = 1;
            var x = 6;
            var y = 4;
            var areaName = "NWIsland";

            var res_x = x * 256;
            var res_y = y * 256;

            var bmp = new Bitmap(res_x, res_y);
            var g = Graphics.FromImage(bmp);

            for (var cur_y = 0; cur_y < y; cur_y++)
            {
                for (var cur_x = 0; cur_x < x; cur_x++)
                {
                    var stream = new MemoryStream(File.ReadAllBytes(@"D:\WoW\BLTE\automaps\Interface\WorldMap\" + mapName + "\\" + areaName + i + ".blp"));
                    var blp = new BlpFile(stream);
                    g.DrawImage(blp.GetBitmap(0), cur_x * 256, cur_y * 256, new Rectangle(0, 0, 256, 256), GraphicsUnit.Pixel);
                    i++;
                }
            }

            g.Dispose();
            if (!Directory.Exists("done")) { Directory.CreateDirectory("done"); }
            bmp.Save("done/" + areaName + ".png");
        }
    }
}
