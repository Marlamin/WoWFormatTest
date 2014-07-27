using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWFormatLib;
using WoWFormatLib.FileReaders;

namespace WoWJSONDump
{
    class Program
    {
        static void Main(string[] args)
        {
            M2Reader reader = new M2Reader("Z:\\18566_full\\");
            reader.LoadM2(@"Creature\Deathwing\Deathwing.M2");

            for (int i = 0; i < reader.model.vertices.Count(); i++)
            {
                Console.WriteLine("v " + reader.model.vertices[i].position.X + " " + reader.model.vertices[i].position.Z * -1 + " " + reader.model.vertices[i].position.Y + " ");
            }

            for (int i = 0; i < reader.model.vertices.Count(); i++)
            {
                Console.WriteLine("vn " + reader.model.vertices[i].normal.X + " " + reader.model.vertices[i].normal.Z * -1 + " " + reader.model.vertices[i].normal.Y + " ");
            }

            for (int i = 0; i < reader.model.skins[0].triangles.Count(); i++)
            {
                Console.WriteLine("f " + (reader.model.skins[0].triangles[i].pt1 + 1) + " " + (reader.model.skins[0].triangles[i].pt2 + 1) + " " + (reader.model.skins[0].triangles[i].pt3 + 1));
            }
        }
    }
}
