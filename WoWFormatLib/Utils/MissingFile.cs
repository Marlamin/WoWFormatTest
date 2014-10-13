using System;
using System.IO;

namespace WoWFormatLib.Utils
{
    public class MissingFile
    {
        public MissingFile(string filename)
        {
            //I have no idea what I'm doing
            //Console.WriteLine("Missing file: " + filename);
            using (StreamWriter sw = File.AppendText("missingfiles.txt"))
            {
                sw.WriteLine(filename);
            }
        }
    }
}