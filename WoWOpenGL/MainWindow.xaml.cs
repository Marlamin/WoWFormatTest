using CASCExplorer;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Threading;
using WoWFormatLib.DBC;
using WoWFormatLib.FileReaders;
using WoWFormatLib.Utils;

namespace WoWOpenGL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static GLControl glc;
        public static System.Windows.Forms.Integration.WindowsFormsHost winFormControl;
        public static TextBox filterBox;
        public static ListBox modelListBox;
        public static ProgressBar cascProgressBar;
        public static Label cascProgressDesc;
        public static TabItem mapsTab;

        private volatile bool fCancelMapLoading = false;

        public static int curlogentry = 0;

        public static bool useCASC = false;
        public static bool CASCinitialized = false;
        public static bool mapsTabLoaded = false;

        public static Window controls;

        private static List<string> models = new List<String>();

        private BackgroundWorkerEx cascWorker = new BackgroundWorkerEx();
        private BackgroundWorker listfileWorker = new BackgroundWorker();
        private BackgroundWorker renderWorker = new BackgroundWorker();

        public MainWindow()
        {
            InitializeComponent();

            cascWorker.DoWork += cascWorker_DoWork;
            cascWorker.RunWorkerCompleted += cascWorker_RunWorkerCompleted;
            cascWorker.ProgressChanged += worker_ProgressChanged;
            cascWorker.WorkerReportsProgress = true;

            listfileWorker.DoWork += ListfileWorker_DoWork;
            listfileWorker.RunWorkerCompleted += ListfileWorker_RunWorkerCompleted;
            listfileWorker.ProgressChanged += worker_ProgressChanged;
            listfileWorker.WorkerReportsProgress = true;

            renderWorker.RunWorkerCompleted += RenderWorker_RunWorkerCompleted;
            renderWorker.ProgressChanged += worker_ProgressChanged;
            renderWorker.WorkerReportsProgress = true;

            filterBox = FilterBox;
            modelListBox = ModelListBox;
            mapsTab = MapsTab;
        }

        private void RenderWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar.Value = 100;
            progressLabel.Content = "Done.";
        }

        private void ListfileWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            FilterBox.Visibility = Visibility.Visible;
            tabs.Visibility = Visibility.Visible;
            ModelListBox.Visibility = Visibility.Visible;
            MapsTab.Visibility = Visibility.Visible;

            progressBar.Value = 100;
            progressLabel.Content = "Done.";

            ModelListBox.DataContext = models;

            winFormControl = wfContainer;
        }

        private void ListfileWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            listfileWorker.ReportProgress(0, "Loading listfile..");
            List<string> linelist = new List<string>();
            
            (CASC.cascHandler.Root as WowRootHandler)?.LoadFileDataComplete(CASC.cascHandler);
            
            foreach(var filename in CASCFile.FileNames)
            {
                linelist.Add(filename.Value);
            }

            if (linelist.Count() == 0)
            {
                // Fall back

                if (!File.Exists("listfile.txt"))
                {
                    throw new Exception("Listfile not found. Unable to continue.");
                }

                listfileWorker.ReportProgress(50, "Loading listfile from disk..");

                foreach(var line in File.ReadAllLines("listfile.txt"))
                {
                    if (CASC.FileExists(line))
                    {
                        linelist.Add(line);
                    }

                }
                linelist.AddRange(File.ReadAllLines("listfile.txt"));
            }

            listfileWorker.ReportProgress(0, "Sorting listfile..");

            linelist.Sort();

            string[] lines = linelist.ToArray();

            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].ToLower();
            }

            List<string> unwantedExtensions = new List<String>();
            for (int u = 0; u < 512; u++)
            {
                unwantedExtensions.Add("_" + u.ToString().PadLeft(3, '0') + ".wmo");
            }

            string[] unwanted = unwantedExtensions.ToArray();

            for (int i = 0; i < lines.Count(); i++)
            {
                var line = lines[i];
                if (line.EndsWith(".wmo"))
                {
                    if (!unwanted.Contains(line.Substring(lines[i].Length - 8, 8)) && !line.EndsWith("lod.wmo") && !line.EndsWith("lod1.wmo") && !line.EndsWith("lod2.wmo"))
                    {
                        if (!models.Contains(line)) { models.Add(line); }
                    }
                }

                if (line.EndsWith(".m2"))
                {
                    if (!line.StartsWith("alternate") && !line.StartsWith("camera"))
                    {
                        if (!models.Contains(line)) { models.Add(line); }
                    }
                }

                if (i % 100 == 0)
                {
                    var progress = (i * 100) / lines.Count();
                    listfileWorker.ReportProgress(progress, "Filtering listfile..");
                }
            }
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var state = (string)e.UserState;

            if (!string.IsNullOrEmpty(state))
            {
                progressLabel.Content = state;
            }

            progressBar.Value = e.ProgressPercentage;
        }

        private void cascWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            listfileWorker.RunWorkerAsync();
        }

        /* MAP STUFF */
        private delegate void LoadMapDelegate(WDTReader wdt);

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadMap();
        }

        private void LoadMap()
        {
            if (MapListBox.SelectedValue == null)
                return;

            string _SelectedMapName = ((KeyValuePair<int, string>)MapListBox.SelectedValue).Value;
            WDTGrid.Children.Clear();

            progressBar.Visibility = Visibility.Visible;
            progressLabel.Visibility = Visibility.Visible;

            progressBar.Value = 0;
            progressLabel.Content = "Loading minimap..";

            var wdt = new WDTReader();
            if (CASC.FileExists(System.IO.Path.Combine(@"world\maps\", _SelectedMapName, _SelectedMapName + ".wdt")))
            {
                Stopwatch _SW = new Stopwatch();
                BackgroundWorker _BackgroundWorker = new BackgroundWorker();
                _BackgroundWorker.WorkerReportsProgress = true;

                _BackgroundWorker.DoWork += new DoWorkEventHandler(
                    (object o, DoWorkEventArgs args) =>
                    {
                        _SW.Start();
                        BackgroundWorker _Worker = o as BackgroundWorker;
                        wdt.LoadWDT(System.IO.Path.Combine(@"world\maps\", _SelectedMapName, _SelectedMapName + ".wdt"));
                        List<int[]> tiles = wdt.getTiles();

                        for (int i = 0; i < tiles.Count; i++)
                        {
                            if (fCancelMapLoading)
                                break;

                            Action _LoadTileAction = delegate() { LoadTile(tiles[i]); };
                            this.Dispatcher.Invoke(_LoadTileAction);
                            _Worker.ReportProgress((i * 100) / tiles.Count, "Loading minimap..");
                        }

                        _Worker.ReportProgress(100, "Minimap loaded.");
                    });

                _BackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(
                    (object o, ProgressChangedEventArgs args) =>
                    {
                        progressBar.Value = args.ProgressPercentage;
                        progressLabel.Content = (string) args.UserState;
                    });

                _BackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
                (object sender, RunWorkerCompletedEventArgs args) =>
                {
                    fCancelMapLoading = false;
                    _SW.Stop();
                });

                _BackgroundWorker.RunWorkerAsync();
            }
        }

        private void LoadTile(int[] tile)
        {
            var x = tile[0];
            var y = tile[1];
            string _SelectedMapName = ((KeyValuePair<int, string>)MapListBox.SelectedValue).Value;
            Rectangle rect = new Rectangle();
            rect.Name = _SelectedMapName.Replace("'", string.Empty).Replace(" ", string.Empty) + "_" + x.ToString("D2") + "_" + y.ToString("D2"); //leading zeros just like adts (TODO: NOT REALLY), this breaks when the mapname has special characters (zg)D: 
            rect.Width = WDTGrid.Width / 64;
            rect.Height = WDTGrid.Height / 64;
            rect.VerticalAlignment = VerticalAlignment.Top;
            rect.HorizontalAlignment = HorizontalAlignment.Left;

            if (CASC.FileExists(System.IO.Path.Combine(@"world\minimaps\" + _SelectedMapName + "\\map" + x.ToString("D2") + "_" + y.ToString("D2") + ".blp")))
            {
                rect.MouseLeftButtonDown += new MouseButtonEventHandler(Rectangle_Mousedown);
                var xmargin = x * rect.Width;
                var ymargin = y * rect.Height;
                rect.Margin = new Thickness(xmargin, ymargin, 0, 0);
                var blp = new BLPReader();

                //Kalimdor takes a few seconds to load, and takes up about ~4xxMB of memory after its loaded, this can be much improved
                blp.LoadBLP(@"world\minimaps\" + _SelectedMapName + "\\map" + x.ToString("D2") + "_" + y.ToString("D2") + ".blp");
                BitmapImage bitmapImage = new BitmapImage();
                using (MemoryStream bitmap = blp.asBitmapStream())
                {
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = bitmap;
                    bitmapImage.DecodePixelHeight = Convert.ToInt32(rect.Width);
                    bitmapImage.DecodePixelWidth = Convert.ToInt32(rect.Height);
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                }
                ImageBrush imgBrush = new ImageBrush(bitmapImage);
                rect.Fill = imgBrush;
            }
            else
            {
                rect.Fill = new SolidColorBrush(Color.FromRgb(0, 111, 0));
                Console.WriteLine(@"world\minimaps\" + _SelectedMapName + "\\map" + x.ToString("D2") + "_" + y.ToString("D2") + ".blp");
            }
            WDTGrid.Children.Add(rect);
        }

        private void rbSortMapId_Checked(object sender, RoutedEventArgs e)
        {
            MapListBox.Items.SortDescriptions.Clear();
            MapListBox.Items.SortDescriptions.Add(new SortDescription("Key", ListSortDirection.Ascending));
        }

        private void rbSortName_Checked(object sender, RoutedEventArgs e)
        {
            MapListBox.Items.SortDescriptions.Clear();
            MapListBox.Items.SortDescriptions.Add(new SortDescription("Value", ListSortDirection.Ascending));
        }

        private void Rectangle_Mousedown(object sender, RoutedEventArgs e)
        {
            fCancelMapLoading = true;
            string name = Convert.ToString(e.Source.GetType().GetProperty("Name").GetValue(e.Source, null));
            Console.WriteLine("Detected mouse event on " + name + "!");

            using (TerrainWindow tw = new TerrainWindow(name, renderWorker))
            {
                tw.Run(30.0, 60.0);
            }
        }

        /* MODEL STUFF */

        private void ModelListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void ModelListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBoxItem item = ModelListBox.SelectedValue as ListBoxItem;

            Render rw;

            if (item == null)//Let's assume its a string
            {
                if (ModelListBox.SelectedValue != null)
                {
                    rw = new Render(ModelListBox.SelectedValue.ToString(), renderWorker);
                }
            }
            else
            {
                rw = new Render(item.Content.ToString(), renderWorker);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            controls = new ControlsWindow();
            controls.Show();

            cascProgressBar = progressBar;

            ModelListBox.Visibility = Visibility.Hidden;
            FilterBox.Visibility = Visibility.Hidden;
            tabs.Visibility = Visibility.Hidden;

            cascWorker.RunWorkerAsync();
        }

        private void cascWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (ConfigurationManager.AppSettings["basedir"] != "" && Directory.Exists(ConfigurationManager.AppSettings["basedir"]))
            {
                CASC.InitCasc(cascWorker, ConfigurationManager.AppSettings["basedir"], "wow_beta");
            }
            else
            {
                CASC.InitCasc(cascWorker, null, "wow_beta");
            }

            CASCinitialized = true;
        }

        private void MapsTab_Focused(object sender, RoutedEventArgs e)
        {
            if (mapsTabLoaded)
            {
                return;
            }

            var reader = new DBCReader<MapRecord>(@"dbfilesclient\map.dbc");
            
            for(int i = 0; i < reader.recordCount; i++){
                MapListBox.Items.Add(new KeyValuePair<int, string>(reader[i].ID, reader[i].Directory));
            }

            MapListBox.DisplayMemberPath = "Value";

            mapsTabLoaded = true;
        }

        private void FilterBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<String> filtered = new List<String>();

            for (int i = 0; i < models.Count(); i++)
            {
                if (models[i].IndexOf(FilterBox.Text, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                {
                    filtered.Add(models[i]);
                }
            }
            ModelListBox.DataContext = filtered;
        }
    }
}