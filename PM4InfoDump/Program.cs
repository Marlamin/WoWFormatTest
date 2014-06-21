using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WoWFormatLib.FileReaders;

namespace PM4InfoDump
{
    class Program
    {
        static void Main(string[] args)
        {
            string basedir = ConfigurationManager.AppSettings["basedir"];
            DirectoryInfo d = new DirectoryInfo(basedir);

            foreach (var file in d.GetFiles("*"))
            {
                PM4Reader reader = new PM4Reader(basedir);
                Console.WriteLine(file.Name);
                reader.LoadPM4(file.Name);
            }

            Console.ReadLine();
        }

    }
}
