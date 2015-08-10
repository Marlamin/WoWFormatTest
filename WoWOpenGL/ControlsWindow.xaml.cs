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
        public static float amb_1;
        public static float amb_2;
        public static float amb_3;
        public static float amb_4;
        public static bool diff_1;
        public static bool diff_2;
        public static bool diff_3;
        public static float diff_4;
        public static float pos_1;
        public static float pos_2;
        public static float pos_3;
        public static float pos_4;
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

        //private void amb1_ValueChanged(object sender, TextChangedEventArgs e)
        //{
        //    amb_1 = 0.0f;
        //    float.TryParse(amb1.Text, out amb_1);
        //}

        //private void amb2_ValueChanged(object sender, TextChangedEventArgs e)
        //{
        //    amb_2 = 0.0f;
        //    float.TryParse(amb2.Text, out amb_2);
        //}

        //private void amb3_ValueChanged(object sender, TextChangedEventArgs e)
        //{
        //    amb_3 = 0.0f;
        //    float.TryParse(amb3.Text, out amb_3);
        //}

        //private void amb4_ValueChanged(object sender, TextChangedEventArgs e)
        //{
        //    amb_4 = 0.0f;
        //    float.TryParse(amb4.Text, out amb_4);
        //}

        //private void diff1_ValueChanged(object sender, RoutedEventArgs e)
        //{
        //    diff_1 = diff1.IsChecked.Value;
        //}

        //private void diff2_ValueChanged(object sender, RoutedEventArgs e)
        //{
        //    diff_2 = diff2.IsChecked.Value;
        //}

        //private void diff3_ValueChanged(object sender, RoutedEventArgs e)
        //{
        //    diff_3 = diff3.IsChecked.Value;
        //}

        //private void diff4_ValueChanged(object sender, RoutedEventArgs e)
        //{
        //  //  diff_4 = (float)Math.Round(diff4.Value, 0);
        //}

        //private void pos1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    pos_1 = (float)Math.Round(pos1.Value, 0) / 100;
        //}

        //private void pos2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    pos_2 = (float)Math.Round(pos2.Value, 0) / 100;
        //}

        //private void pos3_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    pos_3 = (float)Math.Round(pos3.Value, 0) / 100;
        //}

        //private void pos4_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    pos_4 = (float)Math.Round(pos4.Value, 0) / 100;
        //}
    }
}
