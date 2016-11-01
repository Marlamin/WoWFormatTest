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

namespace OBJExporterUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string outdir;
        private readonly BackgroundWorker worker = new BackgroundWorker();
        private readonly BackgroundWorker exportworker = new BackgroundWorker();
        private readonly BackgroundWorkerEx cascworker = new BackgroundWorkerEx();
        private readonly BackgroundWorkerEx fileworker = new BackgroundWorkerEx();

        private bool showM2 = true;
        private bool showWMO = true;

        private bool mapsLoaded = false;
        private bool texturesLoaded = false;

        private List<string> models;
        private List<string> textures;

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
            
            exportworker.DoWork += exportworker_DoWork;
            exportworker.RunWorkerCompleted += exportworker_RunWorkerCompleted;
            exportworker.ProgressChanged += worker_ProgressChanged;
            exportworker.WorkerReportsProgress = true;

            cascworker.DoWork += cascworker_DoWork;
            cascworker.RunWorkerCompleted += cascworker_RunWorkerCompleted;
            cascworker.ProgressChanged += worker_ProgressChanged;
            cascworker.WorkerReportsProgress = true;

            fileworker.DoWork += fileworker_DoWork;
            fileworker.RunWorkerCompleted += fileworker_RunWorkerCompleted;
            fileworker.ProgressChanged += fileworker_ProgressChanged;
            fileworker.WorkerReportsProgress = true;
        }

        private void cascworker_DoWork(object sender, DoWorkEventArgs e)
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

        private void previewButton_Click(object sender, RoutedEventArgs e)
        {
            using (PreviewWindow tw = new PreviewWindow((string)modelListBox.SelectedItem))
            {
                tw.Run(30.0, 60.0);
            }
        }

        private void exportButton_Click(object sender, RoutedEventArgs e)
        {
            if ((string) exportButton.Content == "Crawl maptile for models")
            {
                var filterSplit = filterTextBox.Text.Remove(0, 8).Split('_');
                var filename = "world\\maps\\" + filterSplit[0] + "\\" + filterSplit[0] + "_" + filterSplit[1] + "_" + filterSplit[2] + ".adt";

                fileworker.RunWorkerAsync(filename);
            }else
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

        private void fileworker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            modelListBox.DataContext = (List<string>) e.UserState;
        }

        private void fileworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            exportButton.Content = "Export model to OBJ!";
        }

        private void fileworker_DoWork(object sender, DoWorkEventArgs e)
        {
            var results = new List<string>();
            var remaining = new List<string>();
            var progress = 0;

            remaining.Add((string) e.Argument);

            while(remaining.Count > 0)
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

        private void FilterBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<string> filtered = new List<string>();

            var selectedTab = (TabItem) tabs.SelectedItem;
            if((string)selectedTab.Header == "Textures")
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
            else
            {
                if (filterTextBox.Text.StartsWith("maptile:"))
                {
                    var filterSplit = filterTextBox.Text.Remove(0, 8).Split('_');
                    if (filterSplit.Length == 3)
                    {
                        exportButton.Content = "Crawl maptile for models";

                        if (CASC.FileExists("world/maps/" + filterSplit[0] + "/" + filterSplit[0] + "_" + filterSplit[1] + "_" + filterSplit[2] + ".adt"))
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

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            outdir = ConfigurationManager.AppSettings["outdir"];

            cascworker.RunWorkerAsync();
        }

        private void cascworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.WorkerReportsProgress = true;

            models = new List<string>();
            textures = new List<string>();

            progressBar.Visibility = Visibility.Visible;

            worker.RunWorkerAsync();
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            loadingImage.Visibility = Visibility.Hidden;
            tabs.Visibility = Visibility.Visible;
            progressBar.Visibility = Visibility.Hidden;
            loadingLabel.Visibility = Visibility.Hidden;
            modelListBox.Visibility = Visibility.Visible;
            filterTextBox.Visibility = Visibility.Visible;
            exportButton.Visibility = Visibility.Visible;
            previewButton.Visibility = Visibility.Visible;
            wmoCheckBox.Visibility = Visibility.Visible;
            m2CheckBox.Visibility = Visibility.Visible;

            modelListBox.DataContext = models;
            textureListBox.DataContext = textures;
        }

        private void exportworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            exportButton.IsEnabled = true;
            progressBar.Visibility = Visibility.Hidden;
            loadingLabel.Visibility = Visibility.Hidden;
            wmoCheckBox.IsEnabled = true;
            m2CheckBox.IsEnabled = true;
            modelListBox.IsEnabled = true;
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var state = (string)e.UserState;

            if (!string.IsNullOrEmpty(state))
            {
                loadingLabel.Content = state;
            }
            
            progressBar.Value = e.ProgressPercentage;
        }

        private void exportworker_DoWork(object sender, DoWorkEventArgs e)
        {
            var selectedFiles = (System.Collections.IList) e.Argument;

            foreach (string selectedFile in selectedFiles)
            {
                if (!CASC.FileExists(selectedFile)) { continue; }
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
                    Exporters.OBJ.ADTExporter.exportADT(selectedFile);
                }
            }
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            worker.ReportProgress(0, "Loading listfile..");
            List<string> linelist = new List<string>();

            if (CASC.FileExists("dbfilesclient/filedatacomplete.dbc"))
            {
                var reader = new DBCReader<FileDataRecord>("dbfilesclient/filedatacomplete.dbc");

                if (reader.recordCount > 0)
                {
                    worker.ReportProgress(50, "Loading complete listfile..");

                    for (int i = 0; i < reader.recordCount; i++)
                    {
                        if (CASC.cascHandler.FileExists(reader[i].ID))
                        {
                            linelist.Add(reader[i].FileName + reader[i].FilePath);
                        }
                    }
                }
            }
            
            if(linelist.Count() == 0)
            {
                // Fall back

                if (!File.Exists("listfile.txt"))
                {
                    throw new Exception("Listfile not found. Unable to continue.");
                }

                worker.ReportProgress(50, "Loading listfile from disk..");

                foreach(var line in File.ReadAllLines("listfile.txt"))
                {
                    if (CASC.FileExists(line))
                    {
                        linelist.Add(line);
                    }
                }
            }else
            {
                Console.WriteLine("Linelist count" + linelist.Count());
            }

            worker.ReportProgress(0, "Sorting listfile..");

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
                if (showWMO && lines[i].EndsWith(".wmo")) {
                    if (!unwanted.Contains(lines[i].Substring(lines[i].Length - 8, 8)) && !lines[i].EndsWith("lod.wmo") && !lines[i].EndsWith("lod1.wmo") && !lines[i].EndsWith("lod2.wmo") && !lines[i].EndsWith("lod3.wmo")) {
                        if (!models.Contains(lines[i])) { models.Add(lines[i]); }
                    }
                }

                if (showM2 && lines[i].EndsWith(".m2")) {
                    //if (!lines[i].StartsWith("alternate") && !lines[i].StartsWith("camera")) {
                       models.Add(lines[i]);
                    //}
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
        }

        private void CheckBoxChanged(object sender, RoutedEventArgs e)
        {
            if (exportButton == null) { return; }
            if(m2CheckBox == null) { return; }

            if ((bool) m2CheckBox.IsChecked) { showM2 = true; } else { showM2 = false; }
            if ((bool) wmoCheckBox.IsChecked) { showWMO = true; } else { showWMO = false; }

            progressBar.Visibility = Visibility.Visible;
            loadingLabel.Visibility = Visibility.Visible;
            previewButton.Visibility = Visibility.Hidden;
            exportButton.Visibility = Visibility.Hidden;
            modelListBox.Visibility = Visibility.Hidden;
            filterTextBox.Visibility = Visibility.Hidden;
            wmoCheckBox.Visibility = Visibility.Hidden;
            m2CheckBox.Visibility = Visibility.Hidden;

            models = new List<string>();
            textures = new List<string>();
            worker.RunWorkerAsync();
        }

        private void modelListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(modelListBox.SelectedItems.Count == 1)
            {
                previewButton.IsEnabled = true;
            }
            else
            {
                previewButton.IsEnabled = false;
            }
        }

        private void MapsTab_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!mapsLoaded)
            {
                try
                {
                    mapListBox.DisplayMemberPath = "Value";
                    var mapsData = new DBFilesClient.NET.Storage<MapEntry>(CASC.OpenFile(@"DBFilesClient/Map.db2"));
                    foreach (var mapEntry in mapsData)
                    {
                        mapListBox.Items.Add(new KeyValuePair<string, string>(mapEntry.Value.directory, mapEntry.Value.mapname_lang));
                    }

                    mapsLoaded = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occured: " + ex.Message);
                }
            }
        }

        private void TexturesTab_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!texturesLoaded)
            {
                modelListBox.DataContext = textures;
                texturesLoaded = true;
            }
        }

        private void exportTextureButton_Click(object sender, RoutedEventArgs e)
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

        private void textureListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
            catch(Exception blpException)
            {
                Console.WriteLine(blpException.Message);
            }
        }
    }
}
