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
        public static ProgressBar cascProgressBar;
        public static Label cascProgressDesc;
        private volatile bool fCancelMapLoading = false;
        public static int curlogentry = 0;
        public static bool useCASC = false;
        public static bool CASCinitialized = false;
        public static bool mapsTabLoaded = false;
        public static bool mouseOverRenderArea = false;
        public static Window controls;
        private static List<string> models = new List<String>();
        public MainWindow()
        {
            InitializeComponent();
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
            pbLoadMap.Value = 0d;

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
                            _Worker.ReportProgress((i * 100) / tiles.Count);
                        }

                        _Worker.ReportProgress(100);
                    });

                _BackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(
                    (object o, ProgressChangedEventArgs args) =>
                    {
                        pbLoadMap.Value = args.ProgressPercentage;
                    });

                _BackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
                (object sender, RunWorkerCompletedEventArgs args) =>
                {
                    fCancelMapLoading = false;
                    _SW.Stop();
                    Console.WriteLine("Loading {0} took {1} seconds", _SelectedMapName, _SW.Elapsed.TotalSeconds, _SW.ElapsedMilliseconds);
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

            using (TerrainWindow tw = new TerrainWindow(name))
            {
                tw.Run(30.0, 60.0);
            }
        }

        /* MODEL STUFF */
        private void ModelListBox_Loaded(object sender, RoutedEventArgs e)
        {
            models.Add(@"character\human\male\humanmale_hd.m2");
            models.Add(@"character\troll\male\trollmale_hd.m2");
            models.Add(@"creature\serpent\serpent.m2");
            models.Add(@"creature\deathwing\deathwing.m2");
            models.Add(@"creature\anduin\anduin.m2");
            models.Add(@"creature\arthas\arthas.m2");
            models.Add(@"creature\etherial\etherial.m2");
            models.Add(@"creature\arakkoa2\arakkoa2.m2");
            models.Add(@"creature\garrosh\garrosh.m2");
            models.Add(@"item\objectcomponents\weapon\sword_1h_garrison_a_01.m2");
            models.Add(@"world\expansion05\doodads\ironhorde\6ih_ironhorde_scaffolding13.m2");
            models.Add(@"environments\stars\cavernsoftimesky.m2");
            models.Add(@"world\wmo\kalimdor\ogrimmar\ogrimmar.wmo");
            models.Add(@"world\wmo\azeroth\buildings\stormwind\stormwind2.wmo");
            models.Add(@"world\wmo\draenor\ironhorde\6ih_ironhorde_tower01.wmo");
            models.Add(@"world\wmo\transports\icebreaker\transport_icebreaker_ship_stationary.wmo");
            models.Add(@"world\wmo\azeroth\buildings\townhall\townhall.wmo");
            models.Add(@"world\wmo\transports\passengership\transportship_a.wmo");
            models.Add(@"world\wmo\azeroth\buildings\altarofstorms\altarofstorms.wmo");
            models.Add(@"world\wmo\northrend\dalaran\nd_dalaran.wmo");
            models.Add(@"world\wmo\northrend\howlingfjord\radiotower\radiotower.wmo");
            models.Add(@"world\wmo\outland\darkportal\darkportal.wmo");
            models.Add(@"world\wmo\transports\alliance_battleship\transport_alliance_battleship.wmo");
            models.Add(@"world\wmo\draenor\tanaanjungle\6tj_darkportal_broken.wmo");

            if (File.Exists("listfile.txt"))
            {
                string[] lines = File.ReadAllLines("listfile.txt");

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
                    if (!CASC.FileExists(lines[i])) { continue; }
                    if (lines[i].EndsWith(".m2"))
                    {
                        if (!lines[i].StartsWith("alternate") && !lines[i].StartsWith("camera") && !lines[i].StartsWith("spells"))
                        {
                            models.Add(lines[i]);
                        }
                    }
                    else if (lines[i].EndsWith(".wmo"))
                    {
                        if (!unwanted.Contains(lines[i].Substring(lines[i].Length - 8, 8)) && !lines[i].EndsWith("lod.wmo"))
                        {
                            models.Add(lines[i]);
                        }
                    }
                }
            }

            ModelListBox.DataContext = models;
        }

        private void ModelListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void ModelListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBoxItem item = ModelListBox.SelectedValue as ListBoxItem;

            if (item == null)//Let's assume its a string
            {
                if (ModelListBox.SelectedValue != null)
                {
                    new Render(ModelListBox.SelectedValue.ToString());
                }
            }
            else
            {
                new Render(item.Content.ToString());
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

            winFormControl = wfContainer;
            cascProgressBar = CASCprogress;
            cascProgressDesc = CASCdesc;
            useCASC = true;
            if (!CASCinitialized)
            {
                SwitchToCASC();
            }
        }

        private void SwitchToCASC()
        {
            Console.WriteLine("Intializing CASC filesystem..");
            ModelListBox.Visibility = Visibility.Hidden;
            contentTypeLoading.Visibility = Visibility.Visible;
            CASCdesc.Visibility = Visibility.Visible;
            CASCprogress.Visibility = Visibility.Visible;
            FilterBox.Visibility = Visibility.Hidden;
            

            if (ConfigurationManager.AppSettings["basedir"] != "" && Directory.Exists(ConfigurationManager.AppSettings["basedir"]))
            {
                Console.WriteLine("Using basedir " + ConfigurationManager.AppSettings["basedir"] + " to load..");
                CASC.InitCasc(null, ConfigurationManager.AppSettings["basedir"], "wow_beta");
            }
            else
            {
                CASC.InitCasc();
            }

            Console.WriteLine("CASC filesystem initialized.");
            Console.WriteLine("Generating listfile..");
            List<string> files = new List<String>();
            //models = CASC.GenerateListfile(); // Let's ship listfile instead now!
            ModelListBox.DataContext = models;
            ModelListBox.Items.SortDescriptions.Add(new SortDescription("", ListSortDirection.Ascending));
            Console.WriteLine("Listfile generated!");
            CASCinitialized = true;
            Console.WriteLine("BUILD: " + CASC.cascHandler.Config.BuildName);
            FilterBox.Visibility = Visibility.Visible;
            CASCdesc.Visibility = Visibility.Hidden;
            CASCprogress.Visibility = Visibility.Hidden;
            contentTypeLoading.Visibility = Visibility.Collapsed;
            ModelListBox.Visibility = Visibility.Visible;
            MapsTab.Visibility = Visibility.Visible;
           // using (TerrainWindow tw = new TerrainWindow("draenor_30_31"))
           // {
           //     tw.Run(30.0, 60.0);
           // }
            // new Render(@"world\wmo\draenor\orc\6Oc_orcclans_housesmall.wmo");
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

        private void glControl_MouseEnter(object sender, MouseEventArgs e)
        {
            Console.WriteLine("Mouse entered!");
            mouseOverRenderArea = true;
        }

        private void glControl_MouseLeave(object sender, MouseEventArgs e)
        {
            Console.WriteLine("Mouse left!");
            mouseOverRenderArea = false;
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