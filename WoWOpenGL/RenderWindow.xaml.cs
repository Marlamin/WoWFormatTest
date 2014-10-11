using System.ComponentModel;
using System.Windows;

namespace WoWOpenGL
{
    /// <summary>
    /// Interaction logic for RenderWindow.xaml
    /// </summary>
    public partial class RenderWindow : Window
    {
        private string loadmap;
        public static System.Windows.Forms.Integration.WindowsFormsHost winFormControl;
        public RenderWindow(string name)
        {
            Closing += OnWindowClosing;
            Loaded += Window_Loaded;
            InitializeComponent();
            loadmap = name;
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            MainWindow mw = new MainWindow();
            mw.Show();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            winFormControl = wfContainer;
            new RenderTerrain(loadmap);
        }
    }
}