using System;
using System.Collections.Generic;
using System.Configuration;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WoWFormatLib.DBC;
using WoWFormatLib.FileReaders;

namespace WoWFormatUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var reader = new MapReader(ConfigurationManager.AppSettings["basedir"]);
            Dictionary<int, string> maps = reader.GetMaps();
            foreach (KeyValuePair<int, string> map in maps)
            {
                MapListBox.Items.Add(map.Value);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (MapListBox.SelectedValue != null)
            {
                Console.WriteLine(MapListBox.SelectedValue.ToString());
            }
        }
    }
}
