// This code was written for the OpenTK library and has been released
// to the Public Domain.
// It is provided "as is" without express or implied warranty of any kind.

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics.OpenGL;


namespace Examples.Tutorial
{
    /// <summary>
    /// Demonstrates simple OpenGL Texturing.
    /// </summary>

    public class Textures : GameWindow
    {
        int texture;

        public Textures() : base(800, 600) { }

        #region OnLoad

        /// <summary>
        /// Setup OpenGL and load resources here.
        /// </summary>
        /// <param name="e">Not used.</param>
        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(Color.MidnightBlue);
            GL.Enable(EnableCap.Texture2D);

            //string filename = args[1]; // This should be a ".wdl" file

            string filename = "Draenor/Draenor.wdl";
            GL.Enable(EnableCap.Texture2D);
            using (BinaryReader f = new BinaryReader(File.Open(filename, FileMode.Open)))
            {
                int pos = 0, size = 0;
                string fourCC = "";

                // This array holds absolute (not relative) file positions for places inside MARE chunk, determined from MAOF chunk
                Int32[,] ofsbuf = new Int32[64, 64];

                // File reading
                while (pos < f.BaseStream.Length)
                {
                    //fourCC = f.ReadInt32().ToString();
                    char[] header = new char[4];
                    header = f.ReadChars(4);
                    fourCC = new string(header);
                    //Console.WriteLine(fourCC);

                    size = f.ReadInt32();
                    pos += size + 8;

                    // MAOF (MapAreaFileOffsets) parsing
                    if (fourCC == "FOAM")
                    {
                        for (int i = 0; i < 64; ++i)
                            for (int j = 0; j < 64; ++j)
                                ofsbuf[j, i] = f.ReadInt32();
                    }

                    // MARE (MapAreaRelativeElevations??) parsing
                    if (fourCC == "ERAM")
                    {
                        UInt32[,] texbuf = new UInt32[1024, 1024];
                        Int16[,] tilebuf = new Int16[17, 17];

                        for (int i = 0; i < 64; ++i)
                        {
                            for (int j = 0; j < 64; ++j)
                            {
                                if (ofsbuf[i, j] > 0)
                                {
                                    // Set file position
                                    f.BaseStream.Position = ofsbuf[i, j] + 8;

                                    // Read array from file into array in memory (there is probably a more technical way to do this, but who cares)
                                    for (int x = 0; x < 17; ++x)
                                        for (int y = 0; y < 17; ++y)
                                            tilebuf[x, y] = f.ReadInt16();

                                    // ORIGINAL AUTHOR COMMENTS START HERE
                                    // make minimap
                                    // for a 512x512 minimap texture and 64x64 tiles, one tile is 8x8 pixels
                                    for (int z = 0; z < 16; ++z)
                                    {
                                        for (int x = 0; x < 16; ++x)
                                        {
                                            Int16 hval = tilebuf[z, x]; // (Author) for now
                                            //Console.WriteLine(hval);
                                            // make rgb from height value
                                            byte r, g, b;
                                            if (hval <= 0)
                                            {
                                                // water = blue
                                                if (hval < -511) hval = -511;
                                                hval /= -2;
                                                r = g = 0;
                                                b = (byte)(255 - hval);
                                            }
                                            else
                                            {
                                                // above water = should apply a palette :(
                                                /*
                                                float fh = hval / 1600.0f;
                                                if (fh > 1.0f) fh = 1.0f;
                                                unsigned char c = (unsigned char) (fh * 255.0f);
                                                r = g = b = c;
                                                */

                                                // green: 20, 149, 7    0-600
                                                // brown: 137, 84, 21  600-1200
                                                // gray: 96, 96, 96    1200-1600
                                                // white: 255, 255, 255
                                                byte r1, r2, g1, g2, b1, b2;
                                                float t;
                                                if (hval < 600)
                                                {
                                                    r1 = 20;
                                                    r2 = 137;
                                                    g1 = 149;
                                                    g2 = 84;
                                                    b1 = 7;
                                                    b2 = 21;
                                                    t = hval / 600.0f;
                                                }
                                                else if (hval < 1200)
                                                {
                                                    r2 = 96;
                                                    r1 = 137;
                                                    g2 = 96;
                                                    g1 = 84;
                                                    b2 = 96;
                                                    b1 = 21;
                                                    t = (hval - 600) / 600.0f;
                                                }
                                                else /* if (hval < 1600) */
                                                {
                                                    r1 = 96;
                                                    r2 = 255;
                                                    g1 = 96;
                                                    g2 = 255;
                                                    b1 = 96;
                                                    b2 = 255;
                                                    if (hval >= 1600) hval = 1599;
                                                    t = (hval - 1200) / 600.0f;
                                                }

                                                //! \todo  add a regular palette here

                                                r = (byte)(r2 * t + r1 * (1.0f - t));
                                                g = (byte)(g2 * t + g1 * (1.0f - t));
                                                b = (byte)(b2 * t + b1 * (1.0f - t));
                                                // ORIGINAL AUTHOR COMMENTS END HERE
                                            }

                                            texbuf[j * 16 + z, i * 16 + x] = (UInt32)((r) | (g << 8) | (b << 16) | (255 << 24));
                                        }
                                    }
                                }
                            }
                        }

                        int linear = (int)All.Linear;

                        GL.GenTextures(1, out texture);
                        GL.BindTexture(TextureTarget.Texture2D, texture);
                        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, 1024, 1024, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, texbuf);
                        GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, ref linear);
                        GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, ref linear);
                    }

                    // MAHO (MapAreaHOles) parsing (not implemented by author)
                    if (fourCC == "AHOM")
                    {
                        /*
                        After each MARE chunk there follows a MAHO (MapAreaHOles) chunk. It may be left out if the data is supposed to be 0 all the time.
                        It's an array of 16 shorts. Each short is a bitmask. If the bit is not set, there is a hole at this position.
                        */
                    }

                    f.BaseStream.Position = pos;
                }
            }
        }

        #endregion

        #region OnUnload

        protected override void OnUnload(EventArgs e)
        {
            GL.DeleteTextures(1, ref texture);
        }

        #endregion

        #region OnResize

        /// <summary>
        /// Respond to resize events here.
        /// </summary>
        /// <param name="e">Contains information on the new GameWindow size.</param>
        /// <remarks>There is no need to call the base implementation.</remarks>
        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            GL.Ortho(0.0, 1.0, 0.0, 1.0, 0.0, 4.0);
           // GL.Rotate(90.0f, 0.0, 1.0, 0.0);
        }

        #endregion

        #region OnUpdateFrame

        /// <summary>
        /// Add your game logic here.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        /// <remarks>There is no need to call the base implementation.</remarks>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (Keyboard[OpenTK.Input.Key.Escape])
                this.Exit();
        }

        #endregion

        #region OnRenderFrame

        /// <summary>
        /// Add your game rendering code here.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        /// <remarks>There is no need to call the base implementation.</remarks>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.BindTexture(TextureTarget.Texture2D, texture);

            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(0f, 1f); GL.Vertex2(0f, 0f);
            GL.TexCoord2(1f, 1f); GL.Vertex2(1f, 0f);
            GL.TexCoord2(1f, 0f); GL.Vertex2(1f, 1f);
            GL.TexCoord2(0f, 0f); GL.Vertex2(0f, 1f);

            GL.End();

            SwapBuffers();
        }

        #endregion

        #region public static void Main()

        /// <summary>
        /// Entry point of this example.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            using (Textures example = new Textures())
            {
                // Get the title and category  of this example using reflection.
                example.Title = "WDL thingamajig";
                example.Run(30.0, 0.0);
            }
        }

        #endregion
    }
}