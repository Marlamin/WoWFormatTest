using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWFormatLib.FileReaders;
using WoWFormatLib.Utils;

namespace MinimapCompiler
{
    internal class WMO
    {
        public WMO(){}

        public void Compile(uint fileDataID)
        {
            if (!Listfile.fdidToNameMap.TryGetValue(fileDataID, out var filename))
            {
                throw new Exception("FileDataID " + fileDataID + " is not named in listfile!");
            }

            var wmoreader = new WMOReader();
            var wmo = wmoreader.LoadWMO(fileDataID);

            Console.WriteLine("Compiling minimaps for WMO " + fileDataID);

            if (wmo.group == null || wmo.group.Count() == 0)
            {
                Console.WriteLine("WMO " + fileDataID + " has no groups! Skipping..");
                return;
            }

            int wmo_minx = 999999999;
            int wmo_maxx = 0;
            int wmo_miny = 999999999;
            int wmo_maxy = 0;
            int numtiles = 0;
            //Determine min max offset
            for (int i = 0; i < wmo.group.Count(); i++)
            {
                string groupid = i.ToString().PadLeft(3, '0');

                double drawx1 = wmo.groupInfo[i].boundingBox1.X * 2;
                double drawy1 = wmo.groupInfo[i].boundingBox1.Y * 2;

                double drawx2 = wmo.groupInfo[i].boundingBox2.X * 2;
                double drawy2 = wmo.groupInfo[i].boundingBox2.Y * 2;

                if (drawx1 < wmo_minx) { wmo_minx = (int)drawx1; }
                if (drawx1 > wmo_maxx) { wmo_maxx = (int)drawx1; }

                if (drawy1 < wmo_miny) { wmo_miny = (int)drawy1; }
                if (drawy1 > wmo_maxy) { wmo_maxy = (int)drawy1; }

                if (drawx2 < wmo_minx) { wmo_minx = (int)drawx2; }
                if (drawx2 > wmo_maxx) { wmo_maxx = (int)drawx2; }

                if (drawy2 < wmo_miny) { wmo_minx = (int)drawy2; }
                if (drawy2 > wmo_maxy) { wmo_maxx = (int)drawy2; }
            }

            int wmoresx = 0;
            int wmoresy = 0;
            //Determine image height
            for (int i = 0; i < wmo.group.Count(); i++)
            {
                string groupid = i.ToString().PadLeft(3, '0');

                double drawx1 = wmo.groupInfo[i].boundingBox1.X * 2;
                double drawy1 = wmo.groupInfo[i].boundingBox1.Y * 2;

                double drawx2 = wmo.groupInfo[i].boundingBox2.X * 2;
                double drawy2 = wmo.groupInfo[i].boundingBox2.Y * 2;

                int greenx1 = (int)drawx1 + Math.Abs(wmo_minx);
                int greeny1 = (int)drawy1 + Math.Abs(wmo_miny);

                int greenx2 = (int)drawx2 + Math.Abs(wmo_minx);
                int greeny2 = (int)drawy2 + Math.Abs(wmo_miny);

                if (greenx2 > wmoresx) { wmoresx = greenx2; }
                if (greeny2 > wmoresy) { wmoresy = greeny2; }
            }

            if (wmoresx == 0 || wmoresy == 0)
            {
                Console.WriteLine("WMO " + fileDataID + " has invalid calculated resolution (" + wmoresx + "x" + wmoresy + ")");
                return;
            }

            Bitmap wmobmp = new Bitmap(wmoresx, wmoresy);
            Graphics wmog = Graphics.FromImage(wmobmp);

            /*
            string wmodirname = Path.GetDirectoryName(wmoname.Replace("World" + Path.DirectorySeparatorChar, String.Empty));

            if (!Directory.Exists(Path.Combine(basedir, "World", "Minimaps", wmodirname + Path.DirectorySeparatorChar)))
            {
                Console.WriteLine("WMO has no minimaps directory (" + Path.Combine(basedir, "World", "Minimaps", wmodirname + Path.DirectorySeparatorChar) + "). Skipping..");
                return;
            }
            */

            for (int i = 0; i < wmo.group.Count(); i++)
            {
                string groupid = i.ToString().PadLeft(3, '0');

                double drawx1 = wmo.groupInfo[i].boundingBox1.X * 2;
                double drawy1 = wmo.groupInfo[i].boundingBox1.Y * 2;

                double drawx2 = wmo.groupInfo[i].boundingBox2.X * 2;
                double drawy2 = wmo.groupInfo[i].boundingBox2.Y * 2;

                int greenx1 = (int)drawx1 + Math.Abs(wmo_minx);
                int greeny1 = (int)drawy1 + Math.Abs(wmo_miny);

                int greenx2 = (int)drawx2 + Math.Abs(wmo_minx);
                int greeny2 = (int)drawy2 + Math.Abs(wmo_miny);

                try
                {
                    var minimapbmp = CompileGroup(filename, groupid);

                    if (minimapbmp.Width > 1)
                    {
                        wmog.DrawImage(minimapbmp, greenx1,
                            (wmoresy - (greeny1 + (greeny2 - greeny1)) + (greeny2 - greeny1)),
                            new Rectangle(0, minimapbmp.Height, (greenx2 - greenx1), -(greeny2 - greeny1)),
                            GraphicsUnit.Pixel);
                    }

                    numtiles++;
                    minimapbmp.Dispose();
                }
                catch (FileNotFoundException e)
                {
                    return;
                }
            }

            wmog.Dispose();

            if (numtiles > 0) //check if it even compiled anything
            {
                Directory.CreateDirectory("done" + Path.DirectorySeparatorChar + "WMO" + Path.DirectorySeparatorChar + Path.GetDirectoryName(fileDataID.ToString()));
                wmobmp.Save("done" + Path.DirectorySeparatorChar + "WMO" + Path.DirectorySeparatorChar + Path.Combine(fileDataID.ToString()) + ".png");
            }
            else
            {
                Console.WriteLine("WMO has no minimaps. Skipping..");
            }
        }

        private Bitmap CompileGroup(string filename, string groupid)
        {
            //Console.WriteLine("  group " + groupid);
            int min_x = 64;
            int min_y = 64;

            int max_x = 0;
            int max_y = 0;

            int x = 0;
            int y = 0;

            var wmoonlyname = Path.GetFileNameWithoutExtension(filename);
            var wmodir = filename.Replace("world/", "world/minimaps/").Replace(wmoonlyname + ".wmo", "");
            var filePaths = new List<(uint FileDataID, string GroupFileName)>();
            uint lastpath = 0;

            var hasMinimaps = false;
            for (int cur_x = 0; cur_x < 64; cur_x++)
            {
                for (int cur_y = 0; cur_y < 64; cur_y++)
                {
                    string wmogroupfilename = wmodir + wmoonlyname + "_" + groupid + "_" + cur_x.ToString().PadLeft(2, '0') + "_" + cur_y.ToString().PadLeft(2, '0') + ".blp";
                    if (wmogroupfilename.Contains("000_00_00"))
                    {
                        //Console.WriteLine("CHECKING " + wmogroupfilename);
                    }

                    if (Listfile.nameToFDIDMap.TryGetValue(wmogroupfilename, out var groupFileDataID))
                    {
                        if (CASC.FileExists(groupFileDataID))
                        {
                            //Console.WriteLine(wmogroupfilename + " exists!");
                            filePaths.Add((groupFileDataID, wmogroupfilename));
                            hasMinimaps = true;
                        }
                    }
                    else
                    {
                        //Console.WriteLine("Group filename " + wmogroupfilename + " could not be found in the listfile");
                    }
                }
            }

            foreach ((uint fdid, string path) in filePaths)
            {
                Console.WriteLine(path);
                x = int.Parse(path.Substring(path.Length - 9, 2));
                y = int.Parse(path.Substring(path.Length - 6, 2));

                if (x > max_x) { max_x = x; }
                if (y > max_y) { max_y = y; }

                if (x < min_x) { min_x = x; }
                if (y < min_y) { min_y = y; }

                // Console.WriteLine("[" + groupid + "] MIN: " + min_x + " " + min_y);
                // Console.WriteLine("[" + groupid + "] MAX: " + max_x + " " + max_y);

                lastpath = fdid;
            }

            var res_x = 0;
            var res_y = 0;

            if (min_x == 0 && max_x == 0 && min_y == 0 && max_y == 0)
            {
                var blpreader = new BLPReader();
                blpreader.LoadBLP(lastpath);
                res_x = blpreader.bmp.Width;
                res_y = blpreader.bmp.Height;
            }
            else
            {
                res_x = (((max_x - min_x) * 256) + 256);
                res_y = (((max_y - min_y) * 256) + 256);
            }

            //Console.WriteLine("[" + groupid + "] " + "Creating new image of " + res_x + "x" + res_y + " for " + wmoname);

            if (res_x < 0) { res_x = 1; }
            if (res_y < 0) { res_y = 1; }

            if(!hasMinimaps)
                throw new FileNotFoundException("No minimaps found for this WMO");

            var bmp = new Bitmap(res_x, res_y);
            var g = Graphics.FromImage(bmp);

            foreach ((uint fdid, string path) in filePaths)
            {
                x = int.Parse(path.Substring(path.Length - 9, 2));
                y = int.Parse(path.Substring(path.Length - 6, 2));
                var blpreader = new BLPReader();
                blpreader.LoadBLP(fdid);
                //  Console.WriteLine("BLP Width: " + blpreader.bmp.Width);
                //  Console.WriteLine("BLP Height: " + blpreader.bmp.Height);
                var draw_x = (x - min_x) * 256;
                var draw_y = (max_y - (y - min_y)) * 256;
                //Console.WriteLine("Drawing tile at " + draw_x + " & " + draw_y);
                g.DrawImage(blpreader.bmp, draw_x, draw_y, new Rectangle(0, 0, blpreader.bmp.Width, blpreader.bmp.Height), GraphicsUnit.Pixel);
            }

            g.Dispose();
            return bmp;
        }
    }
}
