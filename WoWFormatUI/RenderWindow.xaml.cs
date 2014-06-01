using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using WoWFormatLib;

namespace WoWFormatUI
{
    /// <summary>
    /// Interaction logic for RenderWindow.xaml
    /// </summary>
    public partial class RenderWindow : Window
    {
        public RenderWindow(string name)
        {
            Closing += OnWindowClosing;
            InitializeComponent();
            RenderLabel.Content = name;
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            MainWindow mw = new MainWindow();
            mw.Show();
        }
    }
}
