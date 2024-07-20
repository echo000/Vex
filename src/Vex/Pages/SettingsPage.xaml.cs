using Microsoft.Win32;
using Vex.Library;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Vex.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        /// <summary>
        /// Gets the ViewModel
        /// </summary>
        private readonly VexSettings Settings;

        private readonly Action reloadSettings;

        public SettingsPage(VexInstance instance, Action reload)
        {
            InitializeComponent();
            Settings = instance.Settings;
            reloadSettings = reload;
            DataContext = Settings;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFolderDialog = new OpenFolderDialog()
            {
                Title = "Select export folder ..."
            };

            if ((bool)openFolderDialog.ShowDialog())
            {
                Settings.ExportDirectory = openFolderDialog.FolderName;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Save("Settings.vcfg");
            reloadSettings();
            NavigationService.GoBack();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // Clear any event handlers
            BrowseButton.Click -= BrowseButton_Click;
            BackButton.Click -= BackButton_Click;

            // Clear data bindings
            ExportBrowseFolder.ClearValue(TextBox.TextProperty);
            ModelExport.ClearValue(ComboBox.SelectedIndexProperty);
            AnimExport.ClearValue(ComboBox.SelectedIndexProperty);
            ImageExport.ClearValue(ComboBox.SelectedIndexProperty);
            AudioExport.ClearValue(ComboBox.SelectedIndexProperty);

            // Clear the DataContext if necessary
            DataContext = null;
        }
    }
}
