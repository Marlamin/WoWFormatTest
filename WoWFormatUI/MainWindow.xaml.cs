using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Windows;
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

        private delegate void LoadMapDelegate(string basedir, WDTReader wdt);

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

        private void Rectangle_Mousedown(object sender, RoutedEventArgs e)
        {
            fCancelMapLoading = true;
            string name = Convert.ToString(e.Source.GetType().GetProperty("Name").GetValue(e.Source, null));
            Console.WriteLine("Detected mouse event on " + name + "!");
            var rw = new RenderWindow(name);
            rw.Show();
            this.Close();
        }

        private void rbSortName_Checked(object sender, RoutedEventArgs e)
        {
            MapListBox.Items.SortDescriptions.Clear();
            MapListBox.Items.SortDescriptions.Add(new SortDescription("Value", ListSortDirection.Ascending));
        }

        private void rbSortMapId_Checked(object sender, RoutedEventArgs e)
        {
            MapListBox.Items.SortDescriptions.Clear();
            MapListBox.Items.SortDescriptions.Add(new SortDescription("Key", ListSortDirection.Ascending));
        }
    }
}