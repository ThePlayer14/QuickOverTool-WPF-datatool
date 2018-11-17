﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace QuickOverTool_WPF
{
    /// <summary>
    /// QueryWindow.xaml 的交互逻辑
    /// </summary>
    public partial class QueryWindow : Window
    {
        public QueryWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            windowQuery.Hide();
        }

        // Method for passing the query back to MainWindow
        public string GetQueries()
        {
            if (String.IsNullOrWhiteSpace(textBoxQuery.Text)) return null;
            string input = textBoxQuery.Text;   // In case someone forgets the parentheses
            if (!input.StartsWith("\"")) input = "\"" + input;
            if (!input.EndsWith("\"")) input += "\"";
            return " " + input;
        }
        // Event selector
        private void AppendEvent(object sender, RoutedEventArgs e)
        {
            string selection = "(event=";
            if (checkBoxExcept.IsChecked == true) selection += "!";
            selection += comboBoxEvent.SelectedItem
                .ToString().Substring(37);
            selection += ")";
            textBoxQuery.Text += selection;
        }
        // Type selector
        private void AppendType(object sender, RoutedEventArgs e)
        {
            string selection = ("|" + comboBoxType.SelectedItem
                .ToString().Substring(37) + "=");
            textBoxQuery.Text += selection;
        }

        private void buttonExit_Click(object sender, RoutedEventArgs e)
        {  
            windowQuery.Hide();
        }

        private void butonCommon_Click(object sender, RoutedEventArgs e)
        {
            textBoxQuery.Text += "(rarity=common)";
        }

        private void buttonRare_Click(object sender, RoutedEventArgs e)
        {
            textBoxQuery.Text += "(rarity=rare)";
        }

        private void buttonLegendary_Click(object sender, RoutedEventArgs e)
        {
            textBoxQuery.Text += "(rarity=legendary)";
        }

        public string HeroUnlocksHelp
        {
            get
            {
                return "    Format: {hero name}|{type}=({tag name}={tag}),{item name}\n" +
                    "        Enter the hero name in target locale, then use buttons to compose your query.\n" +
                    "        You may add multiple queries separated by space.\n" +
                    "        Example: \"Roadhog|emote=(rarity=rare)\"\n" +
                    "       To extract OWL skins, examples:\n" +
                    "        \"{hero name}|skin=(leagueTeam=*)\"\n" +
                    "        \"{hero name}|skin=(leagueTeam=Boston Uprising)\"\n";
            }
            set { }
        }

        public string HeroVoiceHelp
        {
            get
            {
                return "    Supports optional queries: (leaving query empty will extract all voice lines)\n" +
                     "        Types:\n" +
                     "            soundRestriction: only extract a certain sound\n" +
                     "            groupRestriction: only extract a certain group\n" +
                     "        Example: \"Lúcio|soundRestriction = 00000000B56B.0B2\"\n";
            }
            set { }
        }

        public string NPCsHelp
        {
            get
            {
                return "    Enter the NPC name in target locale.\n" +
                    "        Example: \"Eradicator\"";
            }
            set { }
        }

        public string MapHelp
        {
            get
            {
                return "    Enter the map name in target locale.\n" +
                    "        Example: \"Black Forest\"";
            }
            set { }
        }
    }
}
