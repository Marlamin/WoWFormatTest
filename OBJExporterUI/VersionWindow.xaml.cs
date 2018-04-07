using System.Diagnostics;
using System.Windows;

namespace OBJExporterUI
{
    /// <summary>
    /// Interaction logic for VersionWindow.xaml
    /// </summary>
    public partial class VersionWindow : Window
    {
        public VersionWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //FileVersionInfo warptensLibVersion = FileVersionInfo.GetVersionInfo(@"DBFilesClient.NET.dll");
            VersionLabel.Content = "OBJ Exporter version: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void WebsiteButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://marlam.in/obj/");
        }

        private void GHButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/Marlamin/WoWFormatTest/issues");
        }
    }
}
