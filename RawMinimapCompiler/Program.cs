using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawMinimapCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            var maps = Directory.GetDirectories(Environment.CurrentDirectory, "maps/*", SearchOption.TopDirectoryOnly);
            foreach (string mappath in maps)
            {
                var mapname = Path.GetFileName(mappath);
                Console.WriteLine("Processing " + mapname);
                var tiles = Directory.GetFiles(mappath, "*.blp", SearchOption.TopDirectoryOnly);

                var min_x = 64;
                var max_x = 0;
                var min_y = 64;
                var max_y = 0;

                foreach (string tile in tiles)
                {
                    var file = Path.GetFileNameWithoutExtension(tile).Replace("map", "");
                    var coords = file.Split('_');
                    if (int.Parse(coords[0]) > max_x) { max_x = int.Parse(coords[0]); }
                    if (int.Parse(coords[1]) > max_y) { max_y = int.Parse(coords[1]); }

                    if (int.Parse(coords[0]) < min_x) { min_x = int.Parse(coords[0]); }
                    if (int.Parse(coords[1]) < min_y) { min_y = int.Parse(coords[1]); }
                }

                Console.WriteLine("[" + mapname + "] MIN: (" + min_x + " " + min_y + ") MAX: (" + max_x + " " + max_y + ")");
            }
            Console.ReadLine();
        }
    }
}
