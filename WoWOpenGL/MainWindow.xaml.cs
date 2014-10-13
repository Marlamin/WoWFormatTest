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
using System.Threading;
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

namespace WoWOpenGL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static GLControl glc;
        public static System.Windows.Forms.Integration.WindowsFormsHost winFormControl;
        public static ListView debugList;
        public static ProgressBar cascProgressBar;
        public static Label cascProgressDesc;
        private volatile bool fCancelMapLoading = false;
        private bool loaded = false;
        public static int curlogentry = 0;
        public static bool useCASC = false;
        public static bool CASCinitialized = false;
        public static AsyncAction bgAction;
        public MainWindow()
        {
            InitializeComponent();
            var basedir = ConfigurationManager.AppSettings["basedir"];
            var reader = new MapReader(basedir);

            Dictionary<int, string> maps = reader.GetMaps();
            foreach (KeyValuePair<int, string> map in maps)
            {
                MapListBox.Items.Add(map);
            }
            MapListBox.DisplayMemberPath = "Value";
        }

        /* MAP STUFF */
        private delegate void LoadMapDelegate(string basedir, WDTReader wdt);

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadMap();
        }

        private void LoadMap()
        {
            if (MapListBox.SelectedValue == null)
                return;

            var basedir = ConfigurationManager.AppSettings["basedir"];
            string _SelectedMapName = ((KeyValuePair<int, string>)MapListBox.SelectedValue).Value;
            WDTGrid.Children.Clear();
            pbLoadMap.Value = 0d;

            var wdt = new WDTReader(basedir);
            if (File.Exists(System.IO.Path.Combine(basedir, "World\\Maps\\", _SelectedMapName, _SelectedMapName + ".wdt")))
            {
                Stopwatch _SW = new Stopwatch();
                BackgroundWorker _BackgroundWorker = new BackgroundWorker();
                _BackgroundWorker.WorkerReportsProgress = true;

                _BackgroundWorker.DoWork += new DoWorkEventHandler(
                    (object o, DoWorkEventArgs args) =>
                    {
                        _SW.Start();
                        BackgroundWorker _Worker = o as BackgroundWorker;
                        wdt.LoadWDT(System.IO.Path.Combine("World\\Maps\\", _SelectedMapName, _SelectedMapName + ".wdt"));
                        List<int[]> tiles = wdt.getTiles();

                        for (int i = 0; i < tiles.Count; i++)
                        {
                            if (fCancelMapLoading)
                                break;

                            Action _LoadTileAction = delegate() { LoadTile(basedir, tiles[i]); };
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

        private void LoadTile(string basedir, int[] tile)
        {
            var x = tile[0];
            var y = tile[1];
            string _SelectedMapName = ((KeyValuePair<int, string>)MapListBox.SelectedValue).Value;
            Rectangle rect = new Rectangle();
            rect.Name = _SelectedMapName.Replace("'", string.Empty).Replace(" ", string.Empty) + "_" + x.ToString("D2") + "_" + y.ToString("D2"); //leading zeros just like adts, this breaks when the mapname has special characters (zg)D:
            rect.Width = WDTGrid.Width / 64;
            rect.Height = WDTGrid.Height / 64;
            rect.VerticalAlignment = VerticalAlignment.Top;
            rect.HorizontalAlignment = HorizontalAlignment.Left;

            if (File.Exists(basedir + "World\\Minimaps\\" + _SelectedMapName + "\\map" + x.ToString("D2") + "_" + y.ToString("D2") + ".blp"))
            {
                rect.MouseLeftButtonDown += new MouseButtonEventHandler(Rectangle_Mousedown);
                var xmargin = x * rect.Width;
                var ymargin = y * rect.Height;
                rect.Margin = new Thickness(xmargin, ymargin, 0, 0);
                var blp = new BLPReader(basedir);

                //Kalimdor takes a few seconds to load, and takes up about ~4xxMB of memory after its loaded, this can be much improved
                blp.LoadBLP("World\\Minimaps\\" + _SelectedMapName + "\\map" + x.ToString("D2") + "_" + y.ToString("D2") + ".blp");
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
                Console.WriteLine(basedir + "World\\Minimaps\\" + _SelectedMapName + "\\map" + x.ToString("D2") + "_" + y.ToString("D2") + ".blp");
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
            
            var rw = new RenderWindow(name);
            rw.Show();
            this.Hide();
        }

        /* MODEL STUFF */
        private void ModelListBox_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> models = new List<String>();
            models.Add(@"Creature\Serpent\Serpent.M2");
            models.Add(@"Creature\Deathwing\Deathwing.M2");
            models.Add(@"Creature\Anduin\Anduin.M2");
            models.Add(@"Creature\Arthas\Arthas.M2");
            models.Add(@"Creature\Etherial\Etherial.M2");
            models.Add(@"Creature\Arakkoa2\Arakkoa2.m2");
            models.Add(@"Creature\Garrosh\Garrosh.M2");
            models.Add(@"Item\ObjectComponents\Weapon\sword_1h_garrison_a_01.m2");
            models.Add(@"World\Expansion05\Doodads\IronHorde\6ih_ironhorde_scaffolding13.M2");
            models.Add(@"World\WMO\Kalimdor\Ogrimmar\Ogrimmar.wmo");
            models.Add(@"World\WMO\Azeroth\Buildings\StormWind\Stormwind2.wmo");
            models.Add(@"World\WMO\Draenor\IronHorde\6ih_ironhorde_tower01.wmo");
            models.Add(@"World\WMO\transports\Icebreaker\Transport_Icebreaker_ship_stationary.wmo");
            models.Add(@"World\WMO\Azeroth\Buildings\TownHall\TownHall.wmo");
            models.Add(@"World\WMO\transports\passengership\transportship_A.wmo");
            models.Add(@"World\WMO\Azeroth\Buildings\AltarOfStorms\AltarOfStorms.wmo");
            models.Add(@"World\WMO\Northrend\Dalaran\ND_Dalaran.wmo");
            models.Add(@"World\WMO\Northrend\HowlingFjord\RadioTower\RadioTower.wmo");
            models.Add(@"World\WMO\Outland\DarkPortal\DarkPortal.wmo");
            models.Add(@"World\WMO\transports\Alliance_Battleship\Transport_Alliance_Battleship.wmo");
            models.Add(@"World\WMO\Draenor\TanaanJungle\6TJ_DarkPortal_Broken.wmo");

            if (File.Exists("listfile.txt"))
            {
                string line;
                StreamReader file = new System.IO.StreamReader("listfile.txt");
                while ((line = file.ReadLine()) != null)
                {
                    if (line.EndsWith(".m2", StringComparison.OrdinalIgnoreCase) || line.EndsWith(".wmo", StringComparison.OrdinalIgnoreCase))
                    {
                        models.Add(line);
                    }
                }
            }

            ModelListBox.DataContext = models;
        }

        private void ModelListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void ModelListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ListBoxItem item = ModelListBox.SelectedValue as ListBoxItem;

            if (item == null)//Let's assume its a string
            {
                new Render(ModelListBox.SelectedValue.ToString());
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
            debugList = DebugListBox;
            winFormControl = wfContainer;
            cascProgressBar = CASCprogress;
            cascProgressDesc = CASCdesc;
        }

        private void contentTypeOnline_Checked(object sender, RoutedEventArgs e)
        {
            useCASC = true;
            if (!CASCinitialized)
            {
                SwitchToCASC();
            }
            else
            {
                ModelListBox.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void contentTypeLocal_Checked(object sender, RoutedEventArgs e)
        {
            useCASC = false;
            if (ConfigurationManager.AppSettings["basedir"].Count() == 0)
            {
                ModelListBox.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void CascProgress()
        {
            BackgroundWorker bw = new BackgroundWorker();
            MainWindow.cascProgressBar.Visibility = System.Windows.Visibility.Visible;
            MainWindow.cascProgressDesc.Visibility = System.Windows.Visibility.Visible;

            bw.DoWork += (sender, args) =>
            {
                string prevDesc = "";
                while (CASCinitialized == false)
                {
                    Console.WriteLine(WoWFormatLib.Utils.CASC.progressNum);
                    if (prevDesc != WoWFormatLib.Utils.CASC.progressDesc)
                    {
                        MainWindow.cascProgressDesc.Content = WoWFormatLib.Utils.CASC.progressDesc; 
                    }
                    System.Threading.Thread.Sleep(100);
                }
            };
            
            bw.RunWorkerCompleted += (sender, args) =>
            {
                MainWindow.cascProgressBar.Visibility = System.Windows.Visibility.Hidden;
                MainWindow.cascProgressDesc.Visibility = System.Windows.Visibility.Hidden;
            };
            
            bw.RunWorkerAsync();
        }
        private async void SwitchToCASC()
        {
            contentTypeLocal.Visibility = System.Windows.Visibility.Collapsed;
            contentTypeOnline.Visibility = System.Windows.Visibility.Collapsed;
            ModelListBox.Visibility = System.Windows.Visibility.Hidden;
            contentTypeLoading.Visibility = System.Windows.Visibility.Visible;
            CASCdesc.Visibility = System.Windows.Visibility.Visible;
            CASCprogress.Visibility = System.Windows.Visibility.Visible;

            bgAction = new AsyncAction(() => WoWFormatLib.Utils.CASC.InitCasc(bgAction));
            bgAction.ProgressChanged += new EventHandler<AsyncActionProgressChangedEventArgs>(bgAction_ProgressChanged);

            try
            {
                await bgAction.DoAction();
            }
            catch
            {

            }

            CASCinitialized = true;
            CASCdesc.Visibility = System.Windows.Visibility.Hidden;
            CASCprogress.Visibility = System.Windows.Visibility.Hidden;
            contentTypeLoading.Visibility = System.Windows.Visibility.Collapsed;
            contentTypeLocal.Visibility = System.Windows.Visibility.Visible;
            ModelListBox.Visibility = System.Windows.Visibility.Visible;
            contentTypeOnline.Visibility = System.Windows.Visibility.Visible;
        }

        private void bgAction_ProgressChanged(object sender, AsyncActionProgressChangedEventArgs progress)
        {
            CASCprogress.Value = progress.Progress;
            if (progress.UserData != null) { CASCdesc.Content = progress.UserData; }
        }
    }
}