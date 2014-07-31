using SharpDX;
using SharpDX.WPF.Cameras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWRenderLib.Cameras
{
    public class WoWModelViewerCamera : BaseCamera
    {
        public override void HandleMouseWheel(System.Windows.UIElement ui, System.Windows.Input.MouseWheelEventArgs e)
        {
            float zoomModifier = 0.3f;
            Position = new Vector3(Position.X + (e.Delta > 0 ? -zoomModifier : zoomModifier), Position.Y + (e.Delta > 0 ? -zoomModifier : zoomModifier), Position.Z + (e.Delta > 0 ? zoomModifier : -zoomModifier));
            LookAt = new Vector3(LookAt.X + (e.Delta > 0 ? -zoomModifier : zoomModifier), LookAt.Y + (e.Delta > 0 ? -zoomModifier : zoomModifier), LookAt.Z + (e.Delta > 0 ? zoomModifier : -zoomModifier));

            UpdateView();
        }
    }
}