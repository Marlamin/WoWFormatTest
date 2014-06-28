using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWFormatLib.Utils
{
    public class MissingFile
    {
        public MissingFile(string filename)
        {
            //I have no idea what I'm doing
            using (StreamWriter sw = File.AppendText("missingfiles.txt"))
            {
                sw.WriteLine(filename);
            }
        }
    }
}
