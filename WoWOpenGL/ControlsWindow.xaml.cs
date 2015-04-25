using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WoWOpenGL
{
    /// <summary>
    /// Interaction logic for ControlsWindow.xaml
    /// </summary>
    public partial class ControlsWindow : Window
    {
        public static double camSpeed = 50;

        public ControlsWindow()
        {
            InitializeComponent();
        }

        private void CameraSpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CameraSpeedLabel != null) //Sometimes the event fires before label is loaded!
            {
                CameraSpeedLabel.Content = "Camera speed: " + Math.Round(CameraSpeedSlider.Value, 0) + "%";
                camSpeed = Math.Round(CameraSpeedSlider.Value, 0);
            }
        }
    }
}
