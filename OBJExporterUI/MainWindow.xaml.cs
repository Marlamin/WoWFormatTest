using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WoWFormatLib.DBC;
using WoWFormatLib.Utils;
using CASCExplorer;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.Net;
using System.IO.Compression;
using Microsoft.VisualBasic.FileIO;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Input;

namespace OBJExporterUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BackgroundWorker worker = new BackgroundWorker();
        private readonly BackgroundWorker exportworker = new BackgroundWorker();
        private readonly BackgroundWorkerEx cascworker = new BackgroundWorkerEx();
        private readonly BackgroundWorkerEx fileworker = new BackgroundWorkerEx();
        private readonly BackgroundWorkerEx tileworker = new BackgroundWorkerEx();

        private bool showM2 = true;
        private bool showWMO = true;

        private bool mapsLoaded = false;
        private bool texturesLoaded = false;

        private List<string> models;
        private List<string> textures;

        private Dictionary<int, NiceMapEntry> mapNames = new Dictionary<int, NiceMapEntry>();
        private List<string> mapFilters = new List<string>();

        private string outdir;

        private static ListBox tileBox;

        private PreviewControl previewControl;

        public MainWindow()
        {
            if (bool.Parse(ConfigurationManager.AppSettings["firstrun"]) == true)
            {
                var cfgWindow = new ConfigurationWindow();
                cfgWindow.ShowDialog();

                ConfigurationManager.RefreshSection("appSettings");
            }

            if (bool.Parse(ConfigurationManager.AppSettings["firstrun"]) == true)
            {
                Close();
            }

            InitializeComponent();

            tileBox = tileListBox;

            Title = "Marlamin's OBJ Exporter " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            previewControl = new PreviewControl(renderCanvas);
            CompositionTarget.Rendering += previewControl.CompositionTarget_Rendering;
            wfHost.Initialized += previewControl.WindowsFormsHost_Initialized;

            exportworker.DoWork += Exportworker_DoWork;
            exportworker.RunWorkerCompleted += Exportworker_RunWorkerCompleted;
            exportworker.ProgressChanged += Worker_ProgressChanged;
            exportworker.WorkerReportsProgress = true;

            cascworker.DoWork += CASCworker_DoWork;
            cascworker.RunWorkerCompleted += CASCworker_RunWorkerCompleted;
            cascworker.ProgressChanged += Worker_ProgressChanged;
            cascworker.WorkerReportsProgress = true;

            fileworker.DoWork += Fileworker_DoWork;
            fileworker.RunWorkerCompleted += Fileworker_RunWorkerCompleted;
            fileworker.ProgressChanged += Fileworker_ProgressChanged;
            fileworker.WorkerReportsProgress = true;
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            outdir = ConfigurationManager.AppSettings["outdir"];
            cascworker.RunWorkerAsync();
        }

        /* Generic UI */
        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            previewControl.LoadModel((string)modelListBox.SelectedItem);
        }
        private void FilterBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<string> filtered = new List<string>();

            var selectedTab = (TabItem)tabs.SelectedItem;
            if ((string)selectedTab.Header == "Textures")
            {
                for (int i = 0; i < textures.Count(); i++)
                {
                    if (textures[i].IndexOf(filterTextBox.Text, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                    {
                        filtered.Add(textures[i]);
                    }
                }

                textureListBox.DataContext = filtered;
            }
            else if ((string)selectedTab.Header == "Maps")
            {
                UpdateMapListView();
            }
            else
            {
                if (filterTextBox.Text.StartsWith("maptile:"))
                {
                    var filterSplit = filterTextBox.Text.Remove(0, 8).Split('_');
                    if (filterSplit.Length == 3)
                    {
                        exportButton.Content = "Crawl maptile for models";

                        if (CASC.cascHandler.FileExists("world/maps/" + filterSplit[0] + "/" + filterSplit[0] + "_" + filterSplit[1] + "_" + filterSplit[2] + ".adt"))
                        {
                            exportButton.IsEnabled = true;
                        }
                        else
                        {
                            exportButton.IsEnabled = false;
                        }
                    }
                }
                else
                {
                    exportButton.Content = "Export model to OBJ!";
                }

                for (int i = 0; i < models.Count(); i++)
                {
                    if (models[i].IndexOf(filterTextBox.Text, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                    {
                        filtered.Add(models[i]);
                    }
                }

                modelListBox.DataContext = filtered;
            }

        }
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if ((string)exportButton.Content == "Crawl maptile for models")
            {
                var filterSplit = filterTextBox.Text.Remove(0, 8).Split('_');
                var filename = "world\\maps\\" + filterSplit[0] + "\\" + filterSplit[0] + "_" + filterSplit[1] + "_" + filterSplit[2] + ".adt";

                fileworker.RunWorkerAsync(filename);
            }
            else
            {
                progressBar.Value = 0;
                progressBar.Visibility = Visibility.Visible;
                loadingLabel.Content = "";
                loadingLabel.Visibility = Visibility.Visible;
                wmoCheckBox.IsEnabled = false;
                m2CheckBox.IsEnabled = false;
                exportButton.IsEnabled = false;
                modelListBox.IsEnabled = false;

                exportworker.RunWorkerAsync(modelListBox.SelectedItems);
            }
        }

        /* Workers */
        private void CASCworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.WorkerReportsProgress = true;

            models = new List<string>();
            textures = new List<string>();

            progressBar.Visibility = Visibility.Visible;

            worker.RunWorkerAsync();

            UpdateMapListView();

            MainMenu.IsEnabled = true;
        }
        private void CASCworker_DoWork(object sender, DoWorkEventArgs e)
        {
            var basedir = ConfigurationManager.AppSettings["basedir"];
            if (Directory.Exists(basedir))
            {
                if (File.Exists(Path.Combine(basedir, ".build.info")))
                {
                    cascworker.ReportProgress(0, "Loading WoW from disk..");
                    CASC.InitCasc(cascworker, basedir, ConfigurationManager.AppSettings["program"]);
                }
                else
                {
                    throw new Exception("Unable to find World of Warcraft client!");
                }
            }
            else
            {
                cascworker.ReportProgress(0, "Loading WoW from web..");
                CASC.InitCasc(cascworker, null, ConfigurationManager.AppSettings["program"]);
            }
        }

        private void Fileworker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            modelListBox.DataContext = (List<string>)e.UserState;
        }
        private void Fileworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            exportButton.Content = "Export model to OBJ!";
        }
        private void Fileworker_DoWork(object sender, DoWorkEventArgs e)
        {
            var results = new List<string>();
            var remaining = new List<string>();
            var progress = 0;

            remaining.Add((string)e.Argument);

            while (remaining.Count > 0)
            {
                var filename = remaining[0];
                if (filename.EndsWith(".wmo"))
                {
                    var wmo = new WoWFormatLib.FileReaders.WMOReader();
                    wmo.LoadWMO(filename);


                    // Loop through filenames from WMO
                }
                else if (filename.EndsWith(".adt"))
                {
                    var adt = new WoWFormatLib.FileReaders.ADTReader();
                    adt.LoadADT(filename);

                    foreach (var entry in adt.adtfile.objects.wmoNames.filenames)
                    {
                        results.Add(entry.ToLower());
                    }

                    foreach (var entry in adt.adtfile.objects.m2Names.filenames)
                    {
                        results.Add(entry.ToLower());
                    }
                }

                remaining.Remove(filename);
            }

            fileworker.ReportProgress(progress, results);
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            loadingImage.Visibility = Visibility.Hidden;
            tabs.Visibility = Visibility.Visible;
            modelListBox.Visibility = Visibility.Visible;
            filterTextBox.Visibility = Visibility.Visible;
            filterTextLabel.Visibility = Visibility.Visible;
            exportButton.Visibility = Visibility.Visible;
            wmoCheckBox.Visibility = Visibility.Visible;
            m2CheckBox.Visibility = Visibility.Visible;

            progressBar.Value = 100;
            loadingLabel.Content = "Done";

            MenuListfile.IsEnabled = true;

            modelListBox.DataContext = models;
            textureListBox.DataContext = textures;

            previewControl.LoadModel("world/arttest/boxtest/xyz.m2");
#if DEBUG
            //previewControl.BakeTexture("world\\maps\\draenor\\draenor_35_24.adt", "draenor_35_24.png");
#endif
        }
        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var state = (string)e.UserState;

            if (!string.IsNullOrEmpty(state))
            {
                loadingLabel.Content = state;
            }

            progressBar.Value = e.ProgressPercentage;
        }
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            worker.ReportProgress(0, "Loading listfile..");

            List<string> linelist = new List<string>();

            if (!File.Exists("listfile.txt"))
            {
                worker.ReportProgress(20, "Downloading listfile..");
                UpdateListfile();
            }

            worker.ReportProgress(50, "Loading listfile from disk..");

            foreach (var line in File.ReadAllLines("listfile.txt"))
            {
                if (CASC.cascHandler.FileExists(line))
                {
                    linelist.Add(line.ToLower());
                }
            }

            worker.ReportProgress(70, "Sorting listfile..");

            linelist.Sort();

            string[] lines = linelist.ToArray();

            linelist = null;

            List<string> unwantedExtensions = new List<String>();
            for (int u = 0; u < 512; u++)
            {
                unwantedExtensions.Add("_" + u.ToString().PadLeft(3, '0') + ".wmo");
            }

            string[] unwanted = unwantedExtensions.ToArray();

            for (int i = 0; i < lines.Count(); i++)
            {
                if (showWMO && lines[i].EndsWith(".wmo"))
                {
                    if (!unwanted.Contains(lines[i].Substring(lines[i].Length - 8, 8)) && !lines[i].EndsWith("lod.wmo") && !lines[i].EndsWith("lod1.wmo") && !lines[i].EndsWith("lod2.wmo") && !lines[i].EndsWith("lod3.wmo"))
                    {
                        models.Add(lines[i]);
                    }
                }

                if (showM2 && lines[i].EndsWith(".m2"))
                {
                    models.Add(lines[i]);
                }

                if (lines[i].EndsWith(".blp"))
                {
                    textures.Add(lines[i]);
                }

                if (i % 1000 == 0)
                {
                    var progress = (i * 100) / lines.Count();
                    worker.ReportProgress(progress, "Filtering listfile..");
                }
            }

            lines = null;
        }

        private void Exportworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            exportButton.IsEnabled = true;
            progressBar.Value = 100;
            loadingLabel.Content = "Done";
            wmoCheckBox.IsEnabled = true;
            m2CheckBox.IsEnabled = true;
            modelListBox.IsEnabled = true;

            /* ADT specific UI */
            exportTileButton.IsEnabled = true;
            mapListBox.IsEnabled = true;
            tileListBox.IsEnabled = true;
        }
        private void Exportworker_DoWork(object sender, DoWorkEventArgs e)
        {
            var selectedFiles = (System.Collections.IList)e.Argument;

            foreach (string selectedFile in selectedFiles)
            {
                if (!CASC.cascHandler.FileExists(selectedFile)) { continue; }
                if (selectedFile.EndsWith(".wmo"))
                {
                    Exporters.OBJ.WMOExporter.exportWMO(selectedFile, exportworker);
                }
                else if (selectedFile.EndsWith(".m2"))
                {
                    Exporters.OBJ.M2Exporter.exportM2(selectedFile, exportworker);
                }
                else if (selectedFile.EndsWith(".adt"))
                {
                    Exporters.OBJ.ADTExporter.exportADT(selectedFile, exportworker);
                }
                else if (selectedFile.EndsWith(".blp"))
                {
                    try
                    {
                        var blp = new WoWFormatLib.FileReaders.BLPReader();
                        blp.LoadBLP(selectedFile);

                        var bmp = blp.bmp;

                        if (!Directory.Exists(Path.Combine(outdir, Path.GetDirectoryName(selectedFile))))
                        {
                            Directory.CreateDirectory(Path.Combine(outdir, Path.GetDirectoryName(selectedFile)));
                        }

                        bmp.Save(Path.Combine(outdir, Path.GetDirectoryName(selectedFile), Path.GetFileNameWithoutExtension(selectedFile)) + ".png");
                    }
                    catch (Exception blpException)
                    {
                        Console.WriteLine(blpException.Message);
                    }
                }
            }
        }

        /* Model tab */
        private void ModelListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (modelListBox.SelectedItems.Count == 1)
            {
                previewControl.LoadModel((string)modelListBox.SelectedItem);
            }
        }
        private void ModelCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            if (exportButton == null) { return; }
            if (m2CheckBox == null) { return; }

            if ((bool)m2CheckBox.IsChecked) { showM2 = true; } else { showM2 = false; }
            if ((bool)wmoCheckBox.IsChecked) { showWMO = true; } else { showWMO = false; }

            progressBar.Visibility = Visibility.Visible;
            loadingLabel.Visibility = Visibility.Visible;
            exportButton.Visibility = Visibility.Hidden;
            modelListBox.Visibility = Visibility.Hidden;
            filterTextBox.Visibility = Visibility.Hidden;
            filterTextLabel.Visibility = Visibility.Hidden;
            wmoCheckBox.Visibility = Visibility.Hidden;
            m2CheckBox.Visibility = Visibility.Hidden;

            models = new List<string>();
            textures = new List<string>();
            worker.RunWorkerAsync();
        }

        /* Texture tab */
        private void TexturesTab_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!texturesLoaded)
            {
                textureListBox.DataContext = textures;
                texturesLoaded = true;
            }
        }
        private void TextureListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var file = (string)textureListBox.SelectedItem;
            try
            {
                var blp = new WoWFormatLib.FileReaders.BLPReader();
                blp.LoadBLP(file);

                var bmp = blp.bmp;

                using (var memory = new MemoryStream())
                {
                    bmp.Save(memory, ImageFormat.Png);

                    memory.Position = 0;

                    var bitmapImage = new BitmapImage();

                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();

                    blpImage.Source = bitmapImage;
                }
            }
            catch (Exception blpException)
            {
                Console.WriteLine(blpException.Message);
            }
        }
        private void ExportTextureButton_Click(object sender, RoutedEventArgs e)
        {
            progressBar.Value = 0;
            progressBar.Visibility = Visibility.Visible;
            loadingLabel.Content = "";
            loadingLabel.Visibility = Visibility.Visible;
            wmoCheckBox.IsEnabled = false;
            m2CheckBox.IsEnabled = false;
            exportButton.IsEnabled = false;
            modelListBox.IsEnabled = false;

            exportworker.RunWorkerAsync(textureListBox.SelectedItems);
        }

        /* Map tab */
        private void ExportTileButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedMap = (MapListItem)mapListBox.SelectedItem;
            var selectedTile = (string)tileListBox.SelectedItem;

            Console.WriteLine(selectedMap.Name + ", " + selectedMap.Internal + ", " + selectedTile);

            progressBar.Value = 0;
            progressBar.Visibility = Visibility.Visible;
            loadingLabel.Content = "";
            loadingLabel.Visibility = Visibility.Visible;
            wmoCheckBox.IsEnabled = false;
            m2CheckBox.IsEnabled = false;
            exportButton.IsEnabled = false;
            modelListBox.IsEnabled = false;

            /* ADT specific UI */
            exportTileButton.IsEnabled = false;
            mapListBox.IsEnabled = false;
            tileListBox.IsEnabled = false;

            var file = "world/maps/" + selectedMap.Internal.ToLower() + "/" + selectedMap.Internal.ToLower() + "_" + selectedTile + ".adt";

            var tempList = new List<string>();
            tempList.Add(file);

            exportworker.RunWorkerAsync(tempList);

            // Hackfix because I can't seem to get GL and backgroundworkers due to work well together due to threading, will freeze everything
            var mapname = file.Replace("world/maps/", "").Substring(0, file.Replace("world/maps/", "").IndexOf("/"));
            var coord = file.Replace("world/maps/" + mapname + "/" + mapname, "").Replace(".adt", "").Split('_');

            var centerx = int.Parse(coord[1]);
            var centery = int.Parse(coord[2]);
            previewControl.BakeTexture(file.Replace("/", "\\"), Path.Combine(outdir, Path.GetDirectoryName(file), "mat" + centery.ToString() + centerx.ToString() + ".png"));
        }
        private void MapListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            tileListBox.Items.Clear();

            if (mapListBox.HasItems)
            {
                var selectedItem = (MapListItem)mapListBox.SelectedItem;

                var wdt = "world\\maps\\" + selectedItem.Internal + "\\" + selectedItem.Internal + ".wdt";

                if (CASC.cascHandler.FileExists(wdt))
                {
                    var reader = new WoWFormatLib.FileReaders.WDTReader();
                    reader.LoadWDT(wdt);
                    for (var i = 0; i < reader.tiles.Count; i++)
                    {
                        tileListBox.Items.Add(reader.tiles[i][0].ToString().PadLeft(2, '0') + "_" + reader.tiles[i][1].ToString().PadLeft(2, '0'));
                    }
                }
            }
        }
        private void TileListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var file = (string)tileListBox.SelectedItem;

                var selectedItem = (MapListItem)mapListBox.SelectedItem;

                var minimapFile = "world\\minimaps\\" + selectedItem.Internal + "\\map" + file + ".blp";

                if (!CASC.cascHandler.FileExists(minimapFile))
                {
                    minimapFile = @"interface\icons\inv_misc_questionmark.blp";
                }

                var blp = new WoWFormatLib.FileReaders.BLPReader();
                blp.LoadBLP(minimapFile);

                var bmp = blp.bmp;

                using (var memory = new MemoryStream())
                {
                    bmp.Save(memory, ImageFormat.Png);

                    memory.Position = 0;

                    var bitmapImage = new BitmapImage();

                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();

                    tileImage.Source = bitmapImage;
                }

            }
            catch (Exception blpException)
            {
                Console.WriteLine(blpException.Message);
            }
        }
        private void MapCheckBox_Click(object sender, RoutedEventArgs e)
        {
            var source = (CheckBox)sender;
            if (string.IsNullOrEmpty((string)source.Content))
            {
                // Expansion filter
                //Console.WriteLine("Checkbox event on " + source.Name);

                if ((bool)source.IsChecked)
                {
                    if (!mapFilters.Contains(source.Name))
                    {
                        mapFilters.Add(source.Name);
                    }
                }
                else
                {
                    mapFilters.Remove(source.Name);
                }
            }
            else
            {
                // Category filter
                //Console.WriteLine("Checkbox event on " + source.Content);

                if ((bool)source.IsChecked)
                {
                    if (!mapFilters.Contains((string)source.Content))
                    {
                        mapFilters.Add((string)source.Content);
                    }
                }
                else
                {
                    mapFilters.Remove((string)source.Content);
                }
            }

            if (mapsLoaded)
            {
                UpdateMapListView();
            }
        }
        private void MapViewerButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = (MapListItem)mapListBox.SelectedItem;
            if (selectedItem == null) return;

            var selectedTile = tileListBox.SelectedItem;
            if (selectedTile == null) return;

            var tiles = selectedTile.ToString().Split('_');
            var x = int.Parse(tiles[0]);
            var y = int.Parse(tiles[1]);

            var adtFile = "world\\maps\\" + selectedItem.Internal + "\\" + selectedItem.Internal + "_" + x + "_" + y + ".adt";

            previewControl.LoadModel(adtFile);
        }
        private void ExportCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["exportEverything"].Value = exportEverything.IsChecked.ToString();
            config.Save(ConfigurationSaveMode.Full);
        }
        private void TileViewerButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = (MapListItem)mapListBox.SelectedItem;
            if (selectedItem == null) return;

            var mw = new MapWindow(selectedItem.Internal);
            mw.Show();

        }
        private void BakeTextureButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = (MapListItem)mapListBox.SelectedItem;
            if (selectedItem == null) return;

            var selectedTile = tileListBox.SelectedItem;
            if (selectedTile == null) return;

            var tiles = selectedTile.ToString().Split('_');
            var x = int.Parse(tiles[0]);
            var y = int.Parse(tiles[1]);

            var adtFile = "world\\maps\\" + selectedItem.Internal + "\\" + selectedItem.Internal + "_" + x + "_" + y + ".adt";

            if (CASC.cascHandler.FileExists(adtFile))
            {
                bakeTextureButton.IsEnabled = false;

                previewControl.BakeTexture(adtFile, selectedItem.Internal + "_" + x + "_" + y + ".png");

                bakeTextureButton.IsEnabled = true;
            }
        }
        public static void SelectTile(string tile)
        {
            tileBox.SelectedValue = tile;
        }
        public class MapListItem
        {
            public string Type { get; set; }
            public string Name { get; set; }
            public string Internal { get; set; }
            public string Image { get; set; }
        }
        public class NiceMapEntry
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string Internal { get; set; }
            public string Type { get; set; }
            public string Expansion { get; set; }
        }

        /* Menu */
        private void MenuMapNames_Click(object sender, RoutedEventArgs e)
        {
            UpdateMapList();
        }
        private void MenuPreferences_Click(object sender, RoutedEventArgs e)
        {
            var cfgWindow = new ConfigurationWindow(true);
            cfgWindow.ShowDialog();

            ConfigurationManager.RefreshSection("appSettings");
        }
        private void MenuListfile_Click(object sender, RoutedEventArgs e)
        {
            MenuListfile.IsEnabled = false;

            progressBar.Visibility = Visibility.Visible;
            loadingLabel.Visibility = Visibility.Visible;
            exportButton.Visibility = Visibility.Hidden;
            modelListBox.Visibility = Visibility.Hidden;
            filterTextBox.Visibility = Visibility.Hidden;
            filterTextLabel.Visibility = Visibility.Hidden;
            wmoCheckBox.Visibility = Visibility.Hidden;
            m2CheckBox.Visibility = Visibility.Hidden;
            tabs.Visibility = Visibility.Hidden;

            UpdateListfile();

            models.Clear();
            textures.Clear();

            worker.RunWorkerAsync();
        }
        private void MenuVersion_Click(object sender, RoutedEventArgs e)
        {
            var vwindow = new VersionWindow();
            vwindow.Show();
        }
        private void MenuQuit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /* Utilities */
        private void UpdateListfile()
        {
            using (var client = new WebClient())
            using (var stream = new MemoryStream())
            {
                client.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";
                var responseStream = new GZipStream(client.OpenRead(ConfigurationManager.AppSettings["listfileurl"]), CompressionMode.Decompress);
                responseStream.CopyTo(stream);
                File.WriteAllBytes("listfile.txt", stream.ToArray());
                responseStream.Close();
                responseStream.Dispose();
            }
        }
        private void UpdateMapList()
        {
            using (var client = new WebClient())
            using (var stream = new MemoryStream())
            {
                var responseStream = client.OpenRead("https://docs.google.com/spreadsheets/d/1yYSHjWTX0l751QscolQpFNWjwdKLbD_rzviZ_XqTPfk/export?exportFormat=csv&gid=0");
                responseStream.CopyTo(stream);
                File.WriteAllBytes("mapnames.csv", stream.ToArray());
                responseStream.Close();
                responseStream.Dispose();
            }
        }
        private void UpdateMapListView()
        {
            if (!File.Exists("mapnames.csv"))
            {
                UpdateMapList();
            }

            if (File.Exists("mapnames.csv") && mapNames.Count == 0)
            {
                using (TextFieldParser parser = new TextFieldParser("mapnames.csv"))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();
                        if (fields[0] != "ID")
                        {
                            mapNames.Add(int.Parse(fields[0]), new NiceMapEntry { ID = fields[0], Name = fields[4], Internal = fields[2], Type = fields[3], Expansion = fields[5] });
                        }
                    }
                }
            }

            mapListBox.DisplayMemberPath = "Value";
            mapListBox.Items.Clear();

            // try
            //{
            CASC.cascHandler.OpenFile(@"DBFilesClient/Map.db2").ExtractToFile("DBFilesClient", "Map.db2");
            var mapsData = new DBFilesClient.NET.Storage<MapEntry72>(@"DBFilesClient/Map.db2");

            foreach (var mapEntry in mapsData)
            {
                if (CASC.cascHandler.FileExists("World/Maps/" + mapEntry.Value.directory + "/" + mapEntry.Value.directory + ".wdt"))
                {
                    var mapItem = new MapListItem { Internal = mapEntry.Value.directory };

                    if (mapNames.ContainsKey(mapEntry.Key))
                    {
                        mapItem.Name = mapNames[mapEntry.Key].Name;
                        mapItem.Type = mapNames[mapEntry.Key].Type;
                        var expansionID = ExpansionNameToID(mapNames[mapEntry.Key].Expansion);
                        mapItem.Image = "pack://application:,,,/Resources/wow" + expansionID + ".png";

                        if (!mapFilters.Contains("wow" + expansionID) || !mapFilters.Contains(mapItem.Type))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        mapItem.Name = mapEntry.Value.mapname_lang;
                        mapItem.Type = "UNKNOWN";
                        mapItem.Image = "pack://application:,,,/Resources/wow7.png";
                    }

                    if (string.IsNullOrEmpty(filterTextBox.Text) || (mapEntry.Value.directory.IndexOf(filterTextBox.Text, 0, StringComparison.CurrentCultureIgnoreCase) != -1 || mapEntry.Value.mapname_lang.IndexOf(filterTextBox.Text, 0, StringComparison.CurrentCultureIgnoreCase) != -1))
                    {
                        mapListBox.Items.Add(mapItem);
                    }
                }
            }

            /*}
            catch (Exception ex)
            {
                Console.WriteLine("An error occured during DBC reading.. falling back to CSV!" + ex.Message);
                foreach (var map in mapNames)
                {
                    if (CASC.FileExists("World/Maps/" + map.Value.Internal + "/" + map.Value.Internal + ".wdt"))
                    {
                        mapListBox.Items.Add(new MapListItem { Name = map.Value.Name, Internal = map.Value.Internal, Type = map.Value.Type });
                    }
                }
            }*/

            mapsLoaded = true;
        }
        private int ExpansionNameToID(string name)
        {
            switch (name)
            {
                case "Vanilla":
                    return 1;
                case "Burning Crusade":
                    return 2;
                case "Wrath of the Lich King":
                    return 3;
                case "Cataclysm":
                    return 4;
                case "Mists of Pandaria":
                    return 5;
                case "Warlords of Draenor":
                    return 6;
                case "Legion":
                    return 7;
                default:
                    return 1;
            }
        }
    }
}
