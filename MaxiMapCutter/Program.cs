using System;
using System.IO;
using NetVips;

namespace MaxiMapCutter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                throw new Exception("Not enough arguments, need inpng, outdir, maxzoom");
            }

            var inpng = args[0];
            var outdir = args[1];
            var maxzoom = int.Parse(args[2]);

            if (!Directory.Exists(args[1]))
            {
                Directory.CreateDirectory(args[1]);
            }

            var image = Image.NewFromFile(args[0]);

            for (var zoom = maxzoom; zoom > 1; zoom--) {

                Console.WriteLine(zoom);

                if(zoom != maxzoom)
                {
                    image = image.Resize(0.5, "VIPS_KERNEL_NEAREST");
                }

                var width = image.Width;
                var height = image.Height;

                // Always make sure that the image is dividable by 256
                if (width % 256 != 0)
                {
                    width = (width - (width % 256) + 256);
                }

                if (height % 256 != 0)
                {
                    height = (height - (height % 256) + 256);
                }

                image = image.Gravity("VIPS_COMPASS_DIRECTION_NORTH_WEST", width, height);

                var w = 0;
                for (var x = 0; x < width; x += 256){
                    var h = 0;
                    for (var y = 0; y < height; y += 256){
                        image.ExtractArea(x, y, 256, 256).WriteToFile(Path.Combine(outdir, "z" + zoom + "x" + w + "y" + h + ".png"));
                        h++;
                    }
                    w++;
                }
            }
        }
    }
}
