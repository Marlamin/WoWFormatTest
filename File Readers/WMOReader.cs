using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WoWFormatTest
{
    class WMOReader
    {
        public void LoadWMO(string basedir, string filename)
        {
            if(File.Exists(basedir + filename)){
                Console.WriteLine("     WMO " + filename + " exists");
            }else{
                Console.WriteLine("     WMO " + filename + "does not exist");
            }
        }
    }
}
