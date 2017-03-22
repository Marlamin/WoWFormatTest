using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
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

namespace OBJExporterUI
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : Window
    {
        private bool wowFound = false;
        private string wowLoc;

        public ConfigurationWindow()
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Blizzard Entertainment\\World of Warcraft"))
                {
                    if (key != null)
                    {
                        var obj = key.GetValue("InstallPath");
                        if (obj != null)
                        {
                            wowLoc = (string)obj;
                            wowFound = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to get WoW install path from registry. Falling back to online mode! Error: " + ex.Message);
            }

            InitializeComponent();

            if (wowFound)
            {
                basedirLabel.Content = wowLoc;
                localMode.IsChecked = true;
                onlineMode.IsChecked = false;
            }
            else
            {
                onlineMode.IsChecked = true;
                localMode.IsChecked = false;
            }
        }

        private void mode_Checked(object sender, RoutedEventArgs e)
        {
            if(basedirLabel == null) { return; }
            if ((bool) onlineMode.IsChecked)
            {
                programSelect.Visibility = Visibility.Visible;
                programLabel.Visibility = Visibility.Visible;
                onlineLabel.Visibility = Visibility.Visible;
                localLabel.Visibility = Visibility.Hidden;
                basedirBrowse.Visibility = Visibility.Hidden;
                basedirLabel.Visibility = Visibility.Hidden;
            }
            else
            {
                programSelect.Visibility = Visibility.Hidden;
                programLabel.Visibility = Visibility.Hidden;
                onlineLabel.Visibility = Visibility.Hidden;
                localLabel.Visibility = Visibility.Visible;
                basedirBrowse.Visibility = Visibility.Visible;
                basedirLabel.Visibility = Visibility.Visible;
            }
        }

        private void basedirBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result.ToString() == "OK")
            {
                if (File.Exists(System.IO.Path.Combine(dialog.SelectedPath, ".build.info")))
                {
                    basedirLabel.Content = dialog.SelectedPath;
                }
                else
                {
                    basedirLabel.Content = "Could not find a WoW client there!";
                }
            }
        }

        private void programSelect_Loaded(object sender, RoutedEventArgs e)
        {
            programSelect.Items.Add(new KeyValuePair<string, string>("Live/Retail", "wow"));
            programSelect.Items.Add(new KeyValuePair<string, string>("Public Test Realm (PTR)", "wowt"));
            programSelect.Items.Add(new KeyValuePair<string, string>("Beta", "wow_beta"));
            programSelect.DisplayMemberPath = "Key";
            programSelect.SelectedIndex = 0;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            var error = false;

            if ((bool)onlineMode.IsChecked)
            {
                // Online mode

                if (programSelect.SelectedValue == null) {
                    error = true;
                }
                else
                {
                    config.AppSettings.Settings["basedir"].Value = "";
                    config.AppSettings.Settings["program"].Value = ((KeyValuePair<string, string>)programSelect.SelectedValue).Value;
                    config.AppSettings.Settings["firstrun"].Value = "false";
                }
            }
            else
            {
                // Local mode

                if ((string)basedirLabel.Content == "No WoW directory set" || (string)basedirLabel.Content == "Could not find a WoW client there!")
                {
                    error = true;
                }
                else
                {
                    config.AppSettings.Settings["basedir"].Value = (string)basedirLabel.Content;
                    config.AppSettings.Settings["program"].Value = "";
                    config.AppSettings.Settings["firstrun"].Value = "false";
                }
            }

            if((string) outdirLabel.Content != "No export directory set, using application folder")
            {
                if (Directory.Exists((string) outdirLabel.Content))
                {
                    config.AppSettings.Settings["outdir"].Value = (string) outdirLabel.Content;
                }
                else
                {
                    error = true;
                }
            }

            if (!error)
            {
                config.Save(ConfigurationSaveMode.Full);
                System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            }
        }

        private void outdirBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result.ToString() == "OK")
            {
                outdirLabel.Content = dialog.SelectedPath;
            }
        }
    }
}
