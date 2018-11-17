﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;

namespace QuickOverTool_WPF
{
    /// <summary>
    /// MainWindow.xaml logics.
    /// Mostly button events.
    /// </summary>
    public partial class MainWindow : Window
    {
        // About window and query window
        AboutWindow about = new AboutWindow();
        QueryWindow query = new QueryWindow();
        // Color brushes...
        SolidColorBrush red = new SolidColorBrush(Colors.Red);
        SolidColorBrush green = new SolidColorBrush(Colors.Green);
        SolidColorBrush blue = new SolidColorBrush(Colors.Blue);
        SolidColorBrush gray = new SolidColorBrush(Colors.Gray);
        // DataTool path and PID
        int dataToolPID = -1;
        string sharedPath = Path.GetDirectoryName
            (Assembly.GetEntryAssembly().CodeBase).Substring(6);
        // Mode parameters dictionary
        Dictionary<string, string> modes = new Dictionary<string, string>();

        public MainWindow()
        {
            InitializeComponent();
            PopulateDict();
            ReadConfig();
            CheckGUIUpdate();
            CheckDTUpdate();
            FlushInformation();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            SaveConfig();
            System.Windows.Application.Current.Shutdown();
        }

        // Mode dictionary populator
        private void PopulateDict()
        {
            modes.Add("l_heroes", "list-heroes");
            modes.Add("l_generalUnlocks", "list-general-unlocks");
            modes.Add("l_heroUnlocks", "list-unlocks");
            modes.Add("l_maps", "list-maps");
            modes.Add("l_strings", "dump-strings");
            modes.Add("l_achievements", "list-achievements");
            modes.Add("l_lootboxes", "list-lootbox");
            modes.Add("l_keys", "list-keys");
            modes.Add("e_generalUnlocks", "extract-general");
            modes.Add("e_heroUnlocks", "extract-unlocks");
            modes.Add("e_heroVoice", "extract-hero-voice");
            modes.Add("e_heroConvo", "extract-hero-convo");
            modes.Add("e_maps", "extract-maps");
            modes.Add("e_lootboxes", "extract-lootbox");
            modes.Add("e_npcs", "extract-npcs");
            modes.Add("e_abilities", "extract-abilities");
            modes.Add("e_menv", "extract-map-envs");
            modes.Add("l_highlights", "list-highlights");
            modes.Add("l_subt", "list-subtitles");
            modes.Add("l_subtAudio", "list-subtitles-real");
            modes.Add("l_chat", "list-chat-settings");
        }
        // Read paths from config file
        private void ReadConfig()
        {
            if (String.IsNullOrWhiteSpace(Properties.Settings.Default.overwatchPath))
                Properties.Settings.Default.overwatchPath = "C:\\Program Files (x86)\\Battle.net\\Overwatch";
            if (String.IsNullOrWhiteSpace(Properties.Settings.Default.outputPath))
                Properties.Settings.Default.outputPath = ".\\";
            textBoxOverwatchPath.Text = Properties.Settings.Default.overwatchPath;
            textBoxOutputPath.Text = Properties.Settings.Default.outputPath;
        }
        // Save paths from config file
        private void SaveConfig()
        {
            Properties.Settings.Default.overwatchPath = textBoxOverwatchPath.Text;
            Properties.Settings.Default.outputPath = textBoxOutputPath.Text;
            Properties.Settings.Default.Save();
        }
        // Update Information
        public void FlushInformation()
        {
            // Reset colors of important controls
            textBoxOverwatchPath.BorderBrush = gray;
            textBoxOutputPath.BorderBrush = gray;
            groupBoxModesNew.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#ffd5dfe5"));
            buttonExtractQuery.BorderBrush = gray;
            // Retrieve results from validators
            string[] dataTool = Validation.DataTool(sharedPath);
            string[] overwatch = Validation.Overwatch(textBoxOverwatchPath.Text);
            // Also set the window title - Removed because it replicates per each press of "Start" button 
            // Datatool.exe
            if (dataTool[0] != null)
            {
                labelOverToolExecutable.Foreground = new SolidColorBrush(Colors.Green);
                labelOverToolExecutable.Content = dataTool[0];
            }
            else
            {
                labelOverToolExecutable.Foreground = new SolidColorBrush(Colors.Red);
                labelOverToolExecutable.Content = "Not Found";
            }
            // Ow.keys
            if (dataTool[1] != null)
            {
                labelOverToolIntegrity.Foreground = new SolidColorBrush(Colors.Green);
                labelOverToolIntegrity.Content = "Found";
            }
            else
            {
                labelOverToolIntegrity.Foreground = new SolidColorBrush(Colors.Red);
                labelOverToolIntegrity.Content = "Not Found";
            }
            // Overwatch
            if (overwatch != null)
            {
                labelValidity.Content = "Overwatch is Valid";
                labelValidity.Foreground = new SolidColorBrush(Colors.Green);
                textBoxOverwatchPath.BorderBrush = new SolidColorBrush(Colors.Blue);

                labelOverwatchVersion.Foreground = new SolidColorBrush(Colors.Green);
                labelOverwatchVersion.Content = overwatch[0];
                labelOverwatchBranch.Foreground = new SolidColorBrush(Colors.Green);
                labelOverwatchBranch.Content = overwatch[1];
                // DataTool does not support PTR builds
                if (overwatch[1] == "PTR")
                {
                    AddLog("(Potential) PTR build detected. DataTool is incompatible with PTR.");
                    labelOverwatchBranch.Foreground = new SolidColorBrush(Colors.Red);
                } 
            }
            else
            {
                labelValidity.Content = "Overwatch is Invalid";
                textBoxOverwatchPath.BorderBrush = new SolidColorBrush(Colors.Red);
                labelValidity.Foreground = new SolidColorBrush(Colors.Red);

                labelOverwatchVersion.Foreground = new SolidColorBrush(Colors.Red);
                labelOverwatchVersion.Content = "N/A";
                labelOverwatchBranch.Foreground = new SolidColorBrush(Colors.Red);
                labelOverwatchBranch.Content = "N/A";
            }
        }
        // Log increment
        public delegate void AddLogRuntime(string content);

        public void AddLog(string content)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new AddLogRuntime(AddLog), content);
                return;
            }
            else
            {
                textBoxOutput.AppendText("\n" + content);
                textBoxOutput.ScrollToEnd();
            }
        }
        // Get mode selection
        private string GetRadioButton()
        {
            ComboBoxItem selection = new ComboBoxItem();
            if (radioButtonListMode.IsChecked == true)
            {
                selection = (ComboBoxItem)comboBoxList.SelectedItem;
                return modes[selection.Name];
            }
            else if (radioButtonExtractMode.IsChecked == true)
            {
                selection = (ComboBoxItem)comboBoxExtract.SelectedItem;
                return modes[selection.Name];
            }
            else
            {
                groupBoxModesNew.BorderBrush = new SolidColorBrush(Colors.Red);
                throw new ArgumentException("Mode is not selected; please select a mode.");
            }
        }
        // Select Overwatch path
        private void buttonPath_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            DialogResult folderBrowserResult = folderBrowser.ShowDialog();
            textBoxOverwatchPath.Text = folderBrowser.SelectedPath;
            Validation.Overwatch(textBoxOverwatchPath.Text);
            FlushInformation();
        }
        // Select output path
        private void buttonOutputPath_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            DialogResult folderBrowserResult = folderBrowser.ShowDialog();
            textBoxOutputPath.Text = folderBrowser.SelectedPath;
            AddLog("Output path: " + textBoxOutputPath.Text);
            textBoxOutputPath.BorderBrush = new SolidColorBrush(Colors.Blue);
            return;
        }
        // Start
        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            FlushInformation();
            textBoxOutput.Text = "";    // Clear log output
            string command;
            try
            {
               command = FabricateCmdline();
            }
            catch (ArgumentException x)
            {
                AddLog(x.Message);
                return;
            }
            // Launch
            AddLog("Time now: " + DateTime.Now.ToString());
            AddLog("Cmdline: DataTool.exe" + command);
            AddLog("Output: " + textBoxOutputPath.Text);
            StartUp(command);
        }
        // Get DataTool logging
        private void DataTool_DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Data))
            {
                AddLog(Encoding.UTF8.GetString
                    (Encoding.Default.GetBytes(e.Data)));
            }
        }
        // On DataTool exit
        private void DataTool_Exited(object sender, EventArgs e)
        {
            AddLog(DateTime.Now.ToString() + " - DataTool has exited.");
        }

        private void buttonSaveOutput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                File.WriteAllText(Path.Combine(textBoxOutputPath.Text, "log.txt"), textBoxOutput.Text);
                textBoxOutput.Text = "Log written to " + Path.Combine(textBoxOutputPath.Text, "log.txt");
            }
            catch
            {
                textBoxOutputPath.BorderBrush = new SolidColorBrush(Colors.Red);
                AddLog("Output path invalid or permission denied.");
            }
        }

        private void buttonSaveCmdline_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string[] batFile = {"echo off\n",
                ".\\DataTool.exe" + FabricateCmdline(),
                "\npause\n"};
                File.WriteAllLines(".\\_" + GetRadioButton() + ".bat", batFile);
                AddLog("Written batch file for mode " + GetRadioButton() +
                    " to _" + GetRadioButton() + ".bat.");
            }
            catch
            {
                AddLog("Settings invalid. Please check your mode settings.");
            }
        }

        private void buttonTaskkill_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process proc = Process.GetProcessById(dataToolPID);
                DialogResult prompt = System.Windows.Forms.MessageBox.
                    Show("DataTool is still running. Terminate?", "Confirm", MessageBoxButtons.YesNo);
                if (prompt == System.Windows.Forms.DialogResult.Yes)
                {
                    proc.Kill();
                    AddLog("DataTool is terminated.");
                }
                else if (prompt == System.Windows.Forms.DialogResult.No) return;
            }
            catch
            {
                AddLog("Termination failed; DataTool might not be running.");
                return;
            }
        }

        private void buttonAbout_Click(object sender, RoutedEventArgs e)
        {
            about.Show();
        }

        private void checkBoxCommand_Checked(object sender, RoutedEventArgs e)
        {
            textBoxCommand.IsEnabled = true;
            textBoxCommand.BorderBrush = new SolidColorBrush(Colors.Green);
        }

        private void checkBoxCommand_UnChecked(object sender, RoutedEventArgs e)
        {
            textBoxCommand.IsEnabled = false;
            textBoxCommand.BorderBrush = new SolidColorBrush(Colors.Gray);
        }

        private void buttonClearCommand_Click(object sender, RoutedEventArgs e)
        {
            textBoxCommand.Text = "";
        }

        private void listMode_Checked(object sender, RoutedEventArgs e)
        {
            UpdateQueryEditor(null, null);
            comboBoxList.IsEnabled = true;
            comboBoxList.Foreground = new SolidColorBrush(Colors.Black);
            comboBoxExtract.IsEnabled = false;
            comboBoxExtract.Foreground = gray;
            checkBoxJSONOut.Visibility = Visibility.Visible;
        }

        private void extractMode_Checked(object sender, RoutedEventArgs e)
        {
            UpdateQueryEditor(null, null);
            comboBoxList.IsEnabled = false;
            comboBoxList.Foreground = gray;
            comboBoxExtract.IsEnabled = true;
            comboBoxExtract.Foreground = new SolidColorBrush(Colors.Black);
            checkBoxJSONOut.IsChecked = false;
            checkBoxJSONOut.Visibility = Visibility.Hidden;
        }

        private void UpdateQueryEditor(object sender, RoutedEventArgs e)
        {
            if (radioButtonExtractMode.IsChecked == true && 
                (e_heroUnlocks.IsSelected == true ||
                e_npcs.IsSelected == true ||
                e_heroVoice.IsSelected == true ||
                e_maps.IsSelected == true))
                buttonExtractQuery.Visibility = Visibility.Visible;
            else buttonExtractQuery.Visibility = Visibility.Hidden;
        }

        private void buttonExtractQuery_Click(object sender, RoutedEventArgs e)
        {
            query.Show();
        }

        private void checkBoxNoTex_Checked(object sender, RoutedEventArgs e)
        {
            comboBoxTextureFmt.IsEnabled = false;
        }

        private void checkBoxNoTex_Unchecked(object sender, RoutedEventArgs e)
        {
            comboBoxTextureFmt.IsEnabled = true;
        }
    }
}
