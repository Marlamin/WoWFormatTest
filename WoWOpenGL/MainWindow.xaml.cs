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
        private bool loaded = false;
        public static int curlogentry = 0;
        public static bool useCASC = false;
        public static bool CASCinitialized = false;
        public static bool mapsTabLoaded = false;
        public static AsyncAction bgAction;
        public static bool mouseOverRenderArea = false;
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
            if (CASC.FileExists(System.IO.Path.Combine("World\\Maps\\", _SelectedMapName, _SelectedMapName + ".wdt")))
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
            rect.Name = _SelectedMapName.Replace("'", string.Empty).Replace(" ", string.Empty) + "_" + x.ToString("D2") + "_" + y.ToString("D2"); //leading zeros just like adts, this breaks when the mapname has special characters (zg)D:
            rect.Width = WDTGrid.Width / 64;
            rect.Height = WDTGrid.Height / 64;
            rect.VerticalAlignment = VerticalAlignment.Top;
            rect.HorizontalAlignment = HorizontalAlignment.Left;

            if (CASC.FileExists(System.IO.Path.Combine("World\\Minimaps\\" + _SelectedMapName + "\\map" + x.ToString("D2") + "_" + y.ToString("D2") + ".blp")))
            {
                rect.MouseLeftButtonDown += new MouseButtonEventHandler(Rectangle_Mousedown);
                var xmargin = x * rect.Width;
                var ymargin = y * rect.Height;
                rect.Margin = new Thickness(xmargin, ymargin, 0, 0);
                var blp = new BLPReader();

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
                Console.WriteLine("World\\Minimaps\\" + _SelectedMapName + "\\map" + x.ToString("D2") + "_" + y.ToString("D2") + ".blp");
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
            models.Add(@"Character\Human\Male\HumanMale_HD.m2");
            models.Add(@"Character\Troll\Male\TrollMale_HD.m2");
            models.Add(@"Creature\Serpent\Serpent.M2");
            models.Add(@"Creature\Deathwing\Deathwing.M2");
            models.Add(@"Creature\Anduin\Anduin.M2");
            models.Add(@"Creature\Arthas\Arthas.M2");
            models.Add(@"Creature\Etherial\Etherial.M2");
            models.Add(@"Creature\Arakkoa2\Arakkoa2.m2");
            models.Add(@"Creature\Garrosh\Garrosh.M2");
            models.Add(@"Item\ObjectComponents\Weapon\sword_1h_garrison_a_01.m2");
            models.Add(@"World\Expansion05\Doodads\IronHorde\6ih_ironhorde_scaffolding13.M2");
            models.Add(@"Environments\Stars\CavernsOfTimeSky.m2");
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
            
            winFormControl = wfContainer;
            cascProgressBar = CASCprogress;
            cascProgressDesc = CASCdesc;
            useCASC = true;
            if (!CASCinitialized)
            {
                SwitchToCASC();
            }
        }

        private async void SwitchToCASC()
        {
            Console.WriteLine("Intializing CASC filesystem..");
            ModelListBox.Visibility = System.Windows.Visibility.Hidden;
            contentTypeLoading.Visibility = System.Windows.Visibility.Visible;
            CASCdesc.Visibility = System.Windows.Visibility.Visible;
            CASCprogress.Visibility = System.Windows.Visibility.Visible;
            FilterBox.Visibility = System.Windows.Visibility.Hidden;

            if (ConfigurationManager.AppSettings["basedir"] != "" && Directory.Exists(ConfigurationManager.AppSettings["basedir"]))
            {
                bgAction = new AsyncAction(() => WoWFormatLib.Utils.CASC.InitCasc(bgAction, ConfigurationManager.AppSettings["basedir"]));
            }
            else
            {
                bgAction = new AsyncAction(() => WoWFormatLib.Utils.CASC.InitCasc(bgAction));
            }
            
            bgAction.ProgressChanged += new EventHandler<AsyncActionProgressChangedEventArgs>(bgAction_ProgressChanged);

            try
            {
                await bgAction.DoAction();
            }
            catch
            {

            }
            Console.WriteLine("CASC filesystem initialized.");
            Console.WriteLine("Generating listfile..");
            List<string> files = new List<String>();
            models = CASC.GenerateListfile();
            ModelListBox.DataContext = models;
            ModelListBox.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("", System.ComponentModel.ListSortDirection.Ascending));
            Console.WriteLine("Listfile generated!");
            CASCinitialized = true;
            Console.WriteLine("BUILD: " + CASC.cascHandler.Config.BuildName);
            FilterBox.Visibility = System.Windows.Visibility.Visible;
            CASCdesc.Visibility = System.Windows.Visibility.Hidden;
            CASCprogress.Visibility = System.Windows.Visibility.Hidden;
            contentTypeLoading.Visibility = System.Windows.Visibility.Collapsed;
            ModelListBox.Visibility = System.Windows.Visibility.Visible;
            MapsTab.Visibility = System.Windows.Visibility.Visible;
            using (TerrainWindow tw = new TerrainWindow("Draenor_29_25"))
             {
                 tw.Run(30.0, 60.0);
             }
            //new Render("World\\wmo\\Draenor\\Human\\6HU_garrison_townhall_v3.wmo");
        }

        private void bgAction_ProgressChanged(object sender, AsyncActionProgressChangedEventArgs progress)
        {
            CASCprogress.Value = progress.Progress;
            if (progress.UserData != null) { CASCdesc.Content = progress.UserData;}
        }

        private void MapsTab_Focused(object sender, RoutedEventArgs e)
        {
            if (mapsTabLoaded)
            {
                return;
            }

            var reader = new DBCReader<MapRecord>("DBFilesClient\\Map.dbc");
            
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