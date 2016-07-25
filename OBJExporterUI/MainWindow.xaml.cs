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

        private bool showADT = false;
        private bool showM2 = false;
        private bool showWMO = true;

        private bool exportOBJ = true;
        private bool exportDAE = false;

        private List<String> files;

        public MainWindow()
        {
            if (bool.Parse(ConfigurationManager.AppSettings["firstrun"]) == true)
            {
                var cfgWindow = new ConfigurationWindow();
                cfgWindow.ShowDialog();

                ConfigurationManager.RefreshSection("appSettings");
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

        private void button_Click(object sender, RoutedEventArgs e)
        {
            progressBar.Value = 0;
            progressBar.Visibility = Visibility.Visible;
            loadingLabel.Content = "";
            loadingLabel.Visibility = Visibility.Visible;
            adtCheckBox.IsEnabled = false;
            wmoCheckBox.IsEnabled = false;
            m2CheckBox.IsEnabled = false;
            exportButton.IsEnabled = false;
            modelListBox.IsEnabled = false;

            exportworker.RunWorkerAsync(modelListBox.SelectedItems);
        }

        private void FilterBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<String> filtered = new List<String>();

            for (int i = 0; i < files.Count(); i++)
            {
                if (files[i].IndexOf(filterTextBox.Text, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                {
                    filtered.Add(files[i]);
                }
            }

            modelListBox.DataContext = filtered;
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

            files = new List<String>();

            loadingImage.Visibility = Visibility.Hidden;
            progressBar.Visibility = Visibility.Visible;

            worker.RunWorkerAsync();
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar.Visibility = Visibility.Hidden;
            loadingLabel.Visibility = Visibility.Hidden;
            modelListBox.Visibility = Visibility.Visible;
            filterTextBox.Visibility = Visibility.Visible;
            exportButton.Visibility = Visibility.Visible;
            wmoCheckBox.Visibility = Visibility.Visible;
            m2CheckBox.Visibility = Visibility.Visible;
            adtCheckBox.Visibility = Visibility.Visible;
            objCheckBox.Visibility = Visibility.Visible;
            //daeCheckBox.Visibility = Visibility.Visible;

            modelListBox.DataContext = files;
        }

        private void exportworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            exportButton.IsEnabled = true;
            progressBar.Visibility = Visibility.Hidden;
            loadingLabel.Visibility = Visibility.Hidden;
            adtCheckBox.IsEnabled = true;
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
                if (showADT && lines[i].EndsWith(".adt")) {
                    if(!lines[i].EndsWith("obj0.adt") && !lines[i].EndsWith("obj1.adt") && !lines[i].EndsWith("tex0.adt") && !lines[i].EndsWith("tex1.adt") && !lines[i].EndsWith("_lod.adt"))
                    {
                        if (!files.Contains(lines[i])) { files.Add(lines[i]); }
                    }
                }

                if (showWMO && lines[i].EndsWith(".wmo")) {
                    if (!unwanted.Contains(lines[i].Substring(lines[i].Length - 8, 8)) && !lines[i].EndsWith("lod.wmo") && !lines[i].EndsWith("lod1.wmo") && !lines[i].EndsWith("lod2   .wmo")) {
                        if (!files.Contains(lines[i])) { files.Add(lines[i]); }
                    }
                }

                if (showM2 && lines[i].EndsWith(".m2")) {
                    if (!lines[i].StartsWith("alternate") && !lines[i].StartsWith("camera")) {
                        if (!files.Contains(lines[i])) { files.Add(lines[i]); }
                    }
                }

                if (i % 100 == 0)
                {
                    var progress = (i * 100) / lines.Count();
                    worker.ReportProgress(progress, "Filtering listfile..");
                }
            }
        }

        private void CheckBoxChanged(object sender, RoutedEventArgs e)
        {
            if(m2CheckBox == null) { return; }
            if ((bool) adtCheckBox.IsChecked) { showADT = true; } else { showADT = false; }
            if ((bool) m2CheckBox.IsChecked) { showM2 = true; } else { showM2 = false; }
            if ((bool) wmoCheckBox.IsChecked) { showWMO = true; } else { showWMO = false; }

            progressBar.Visibility = Visibility.Visible;
            loadingLabel.Visibility = Visibility.Visible;
            exportButton.Visibility = Visibility.Hidden;
            modelListBox.Visibility = Visibility.Hidden;
            filterTextBox.Visibility = Visibility.Hidden;
            wmoCheckBox.Visibility = Visibility.Hidden;
            m2CheckBox.Visibility = Visibility.Hidden;
            adtCheckBox.Visibility = Visibility.Hidden;

            files = new List<String>();

            worker.RunWorkerAsync();
        }
    }
}
