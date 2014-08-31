using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        private volatile bool fCancelMapLoading = false;
        private bool fLoading = false;

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
            rect.Name = _SelectedMapName.Replace("'", string.Empty).Replace(" ", string.Empty) + x.ToString("D2") + "_" + y.ToString("D2"); //leading zeros just like adts, this breaks when the mapname has special characters (zg)D:
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

        private void ModelListBox_Loaded(object sender, RoutedEventArgs e)
        {
            var basedir = ConfigurationManager.AppSettings["basedir"];
            //List<string> M2s = Directory.EnumerateFiles(basedir, "*.m2", SearchOption.AllDirectories).ToList();
            List<string> M2s = new List<String>();
            M2s.Add(@"Creature\Serpent\Serpent.M2");
            M2s.Add(@"Creature\Deathwing\Deathwing.M2");
            M2s.Add(@"Creature\Anduin\Anduin.M2");
            M2s.Add(@"Creature\Arthas\Arthas.M2");
            M2s.Add(@"Creature\Etherial\Etherial.M2");
            M2s.Add(@"Creature\Arakkoa2\Arakkoa2.m2");
            M2s.Add(@"Creature\Garrosh\Garrosh.M2");
            M2s.Add(@"Item\ObjectComponents\Weapon\Sword_1H_PVPPandariaS2_C_01.M2");
            M2s.Add(@"World\Expansion05\Doodads\IronHorde\6ih_ironhorde_scaffolding13.M2");
            M2s.Add(@"World\WMO\transports\Icebreaker\Transport_Icebreaker_ship_stationary.wmo");
            M2s.Add(@"World\WMO\Azeroth\Buildings\TownHall\TownHall.wmo");
            M2s.Add(@"World\WMO\transports\passengership\transportship_A.wmo");
            M2s.Add(@"World\WMO\Azeroth\Buildings\AltarOfStorms\AltarOfStorms.wmo");
            M2s.Add(@"World\WMO\Northrend\Dalaran\ND_Dalaran.wmo");
            M2s.Add(@"World\WMO\Northrend\HowlingFjord\RadioTower\RadioTower.wmo");
            M2s.Add(@"World\WMO\Outland\DarkPortal\DarkPortal.wmo");
            M2s.Add(@"World\WMO\transports\Alliance_Battleship\Transport_Alliance_Battleship.wmo");
            M2s.Add(@"World\WMO\Draenor\TanaanJungle\6TJ_DarkPortal_Broken.wmo");
            if (File.Exists("listfile.txt"))
            {
                string line;
                StreamReader file = new System.IO.StreamReader("listfile.txt");
                while ((line = file.ReadLine()) != null)
                {
                    if (line.EndsWith(".m2", StringComparison.OrdinalIgnoreCase) || (line.EndsWith(".wmo", StringComparison.OrdinalIgnoreCase) && !line.Contains("_0") && !line.Contains("_1")))
                    {
                        M2s.Add(line);
                    }
                }
            }

            for (int i = 0; i < M2s.Count; i++)
            {
                M2s[i] = M2s[i].Replace(basedir, string.Empty);
            }
            //ModelListBox.Items.Clear();
            ModelListBox.DataContext = M2s;
        }

        private void ModelListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ListBoxItem item = ModelListBox.SelectedValue as ListBoxItem;

            if (item == null)//Let's assume its a string
            {
                rRender = new Render(ModelListBox.SelectedValue.ToString());
                rModelRenderWindow.Renderer = rRender;
                rModelRenderWindow.Focus();
            }
            else
            {
                rRender = new Render(item.Content.ToString());
                rModelRenderWindow.Renderer = rRender;
                rModelRenderWindow.Focus();
            }
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
            this.Close();
        }
    }
}