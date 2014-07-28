using System.ComponentModel;
using System.Windows;

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