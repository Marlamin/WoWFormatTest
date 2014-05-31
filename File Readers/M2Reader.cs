using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWFormatTest
{
    class M2Reader
    {
        private List<String> blpFiles;
        public void LoadM2(string filename)
        {
            filename = filename.Replace("MDX", "M2").Replace("MDL", "M2");

            var basedir = ConfigurationManager.AppSettings["basedir"];

            blpFiles = new List<string>();
            /*
            FileStream m2 = File.Open(basedir + filename, FileMode.Open);
            BinaryReader bin = new BinaryReader(m2);
            
            long position = 0;

            //M2's aren't chunked and have a stringblock at the end instead of right in the middle. Fun times ahead!

            while (position < m2.Length)
            {
                position++;
            }
            m2.Close();
            */
        }
    }
}
