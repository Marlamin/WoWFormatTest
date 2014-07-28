using System;
using System.Linq;

namespace WoWFormatLib.Utils
{
    public class BlizzHeader
    {
        private char[] header;
        private UInt32 size;

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

        public void Flip()
        {
            header = header.Reverse().ToArray();
        }

        public bool Is(String name)
        {
            return string.Join(string.Empty, header).Equals(name, StringComparison.InvariantCulture);
        }

        public override String ToString()
        {
            return new String(header);
        }
    }
}