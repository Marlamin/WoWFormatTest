using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;

namespace WoWFormatTest
{
    class WMOReader
    {
        public void LoadWMO(string filename)
        {
            string basedir = ConfigurationManager.AppSettings["basedir"];

            if(File.Exists(Path.Combine(basedir + filename))){
                Console.WriteLine("     WMO " + filename + " exists");
            }else{
                Console.WriteLine("     WMO " + filename + "does not exist");
            }
        }
    }
}
