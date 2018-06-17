using System;
using System.IO;

namespace MinimapTranslator
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach(var line in File.ReadAllLines("Minimaps/Textures/Minimap/md5translate.trs"))
            {
                if (line.Substring(0, 4) == "dir:" || line.Substring(0, 3) == "WMO")
                    continue;

                var exploded = line.Split('\t');
                Directory.CreateDirectory(Path.Combine("output", Path.GetDirectoryName(exploded[0])));
                File.Copy("Minimaps/Textures/Minimap/" + exploded[1], Path.Combine("output", exploded[0]));
            }
        }
    }
}
