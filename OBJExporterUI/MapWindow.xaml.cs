using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;
using WoWFormatLib.FileReaders;
using WoWFormatLib.Utils;

namespace OBJExporterUI
{
    /// <summary>
    /// Interaction logic for MapWindow.xaml
    /// </summary>
    public partial class MapWindow : Window
    {
        private string _map;
        private volatile bool fCancelMapLoading = false;
        Point m_start;
        Vector m_startOffset;
        private int min_x = 64;
        private int min_y = 64;
        private int max_x = 0;
        private int max_y = 0;

        public MapWindow(string map)
        {
            _map = map;
            InitializeComponent();
            LoadMap(_map);
        }

        private void LoadMap(string map)
        {
            WDTGrid.Children.Clear();

            var wdt = new WDTReader();
            if (CASC.cascHandler.FileExists(System.IO.Path.Combine(@"world\maps\", map, map + ".wdt")))
            {
                Stopwatch _SW = new Stopwatch();
                BackgroundWorker _BackgroundWorker = new BackgroundWorker();
                _BackgroundWorker.WorkerReportsProgress = true;

                _BackgroundWorker.DoWork += new DoWorkEventHandler(
                    (object o, DoWorkEventArgs args) =>
                    {
                        _SW.Start();
                        BackgroundWorker _Worker = o as BackgroundWorker;
                        wdt.LoadWDT(System.IO.Path.Combine(@"world\maps\", map, map + ".wdt"));
                        List<int[]> tiles = wdt.GetTiles();

                        if(tiles.Count == 0)
                        {
                            return;
                        }

                        for (int i = 0; i < tiles.Count; i++)
                        {
                            if (map == "Kalimdor")
                            {
                                if (tiles[i][0] < 5 && tiles[i][1] < 5)
                                {
                                    // Filter out GM island
                                    continue;
                                }
                            }

                            if (tiles[i][0] < min_x) { min_x = tiles[i][0]; }
                            if (tiles[i][1] < min_y) { min_y = tiles[i][1]; }

                            if (tiles[i][0] > max_x) { max_x = tiles[i][0]; }
                            if (tiles[i][1] > max_y) { max_y = tiles[i][1]; }
                        }

                        for (int i = 0; i < tiles.Count; i++)
                        {
                            if (map == "Kalimdor")
                            {
                                // Filter out GM island
                                if (tiles[i][0] < 5 && tiles[i][1] < 5)
                                {
                                    continue;
                                }
                            }

                            if (fCancelMapLoading)
                                break;

                            Action _LoadTileAction = delegate () { LoadTile(tiles[i]); };
                            this.Dispatcher.Invoke(_LoadTileAction);
                            _Worker.ReportProgress((i * 100) / tiles.Count, "Loading map..");
                        }

                        _Worker.ReportProgress(100, "Map loaded.");
                    });

                _BackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(
                    (object o, ProgressChangedEventArgs args) =>
                    {
                        progressBar.Value = args.ProgressPercentage;
                        progressLabel.Content = (string)args.UserState;
                    });

                _BackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
                (object sender, RunWorkerCompletedEventArgs args) =>
                {
                    fCancelMapLoading = false;
                    if (max_x == 0 && min_x == 64)
                    {
                        Close();
                        return;
                    }
                    Width = (max_x - min_x) * (WDTGrid.Width / 64) + 64;
                    Height = (max_y - min_y) * (WDTGrid.Height / 64) + 64;
                    progressBar.Visibility = Visibility.Hidden;
                    progressLabel.Visibility = Visibility.Hidden;
                    WDTGrid.Visibility = Visibility.Visible;
                    _SW.Stop();
                });

                _BackgroundWorker.RunWorkerAsync();
            }
        }

        private void LoadTile(int[] tile)
        {
            var x = tile[0];
            var y = tile[1];
            string _SelectedMapName = _map;
            Rectangle rect = new Rectangle();
            rect.Name = _SelectedMapName.Replace("'", string.Empty).Replace(" ", string.Empty) + "_" + x.ToString("D2") + "_" + y.ToString("D2"); //leading zeros just like adts (TODO: NOT REALLY), this breaks when the mapname has special characters (zg)D: 
            rect.Width = WDTGrid.Width / 64;
            rect.Height = WDTGrid.Height / 64;
            rect.VerticalAlignment = VerticalAlignment.Top;
            rect.HorizontalAlignment = HorizontalAlignment.Left;
            rect.MouseEnter += Rect_MouseEnter;
            rect.MouseLeave += Rect_MouseLeave;
            if (CASC.cascHandler.FileExists(System.IO.Path.Combine(@"world\minimaps\" + _SelectedMapName + "\\map" + x.ToString("D2") + "_" + y.ToString("D2") + ".blp")))
            {
                rect.MouseLeftButtonDown += new MouseButtonEventHandler(Rectangle_Mousedown);
                var xmargin = (x * rect.Width) - (min_x * rect.Width);
                var ymargin = (y * rect.Height) - (min_y * rect.Height);
                rect.Margin = new Thickness(xmargin, ymargin, 0, 0);

                var blp = new BLPReader();
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
                var xmargin = (x * rect.Width) - (min_x * rect.Width);
                var ymargin = (y * rect.Height) - (min_y * rect.Height);
                rect.Margin = new Thickness(xmargin, ymargin, 0, 0);
                rect.Fill = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            }
            WDTGrid.Children.Add(rect);
        }

        private void Rect_MouseLeave(object sender, MouseEventArgs e)
        {
            var source = (Rectangle)sender;
            source.StrokeThickness = 0;
        }

        private void Rect_MouseEnter(object sender, MouseEventArgs e)
        {
            var source = (Rectangle)sender;
            source.Stroke = Brushes.Red;
            source.StrokeThickness = 2;
        }

        private void Rectangle_Mousedown(object sender, RoutedEventArgs e)
        {
            fCancelMapLoading = true;
            string name = Convert.ToString(e.Source.GetType().GetProperty("Name").GetValue(e.Source, null));
            Console.WriteLine("Detected mouse event on " + name + "!");
            MainWindow.SelectTile(name.Replace(_map + "_", string.Empty));
        }

        private void WDTGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            m_start = e.GetPosition(mapWindow);
            m_startOffset = new Vector(tt.X, tt.Y);
            WDTGrid.CaptureMouse();
        }

        private void WDTGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            WDTGrid.ReleaseMouseCapture();
        }

        private void WDTGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (WDTGrid.IsMouseCaptured)
            {
                Vector offset = Point.Subtract(e.GetPosition(mapWindow), m_start);

                tt.X = m_startOffset.X + (offset.X / ts.ScaleX);
                tt.Y = m_startOffset.Y + (offset.Y / ts.ScaleY);
            }
        }

        private void WDTGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Console.WriteLine(e.Delta);
            if(e.Delta == 120)
            {
                ts.ScaleX = ts.ScaleX + 0.1;
                ts.ScaleY = ts.ScaleY + 0.1;
            }
            else if(e.Delta == -120)
            {
                ts.ScaleX = ts.ScaleX - 0.1;
                ts.ScaleY = ts.ScaleY - 0.1;
            }
        }

        private void mapWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
