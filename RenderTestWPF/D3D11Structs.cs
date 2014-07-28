using SharpDX;
using System.Runtime.InteropServices;

namespace RenderTestWPF
{
    /// <summary>
    ///
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Projections
    {
        public Matrix World;
        public Matrix View;
        public Matrix Projection;
    }
}