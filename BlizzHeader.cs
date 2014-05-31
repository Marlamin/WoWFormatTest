using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWFormatTest
{
    class BlizzHeader
    {
        private char[] header;
        UInt32 size;

        public BlizzHeader()
        {
            header = new char[4];
            size = 0;
        }

        public BlizzHeader(char[] h, UInt32 s)
        {
            if (h.Length != 4) { throw new Exception("Header should be exactly 4 chars"); }
            header = h;
            size = s;
        }

        public void Flip()
        {
            char t;

            t = header[0];
            header[0] = header[3];
            header[3] = t;

            t = header[1];
            header[1] = header[2];
            header[2] = t;
        }

        public bool Is(String name)
        {
            if (name.Length == 4){
                if (header[0] == name[0] && header[1] == name[1] && header[2] == name[2] && header[3] == name[3]){
                    return true;
                }else{
                    return false;
                }
            }else{
                return false;
            }
        }

        public override String ToString() { return new String(header); }

        public char[] Header
        {
            get { return header; }
            set { header = value; }
        }

        public UInt32 Size
        {
            get { return size; }
            set { size = value; }
        }
    }
}
