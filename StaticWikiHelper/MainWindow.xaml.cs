using System;
using System.Collections.Generic;
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
using System.IO;
using StaticWiki;
using System.Windows.Forms;

namespace StaticWikiHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FileSystemWatcher fileSystemWatcher;
        private string sourceDirectory;
        private string destinationDirectory;
        private string themeFileName;
        private string titleName;

        public MainWindow()
        {
            InitializeComponent();

            Title = "StaticWiki Helper";
        }

        private void BrowseSourceDirectory_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new FolderBrowserDialog();

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SourceDirectoryText.Text = openFileDialog.SelectedPath;
            }
        }

        private void BrowseDestinationDirectory_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new FolderBrowserDialog();

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DestinationDirectoryText.Text = openFileDialog.SelectedPath;
            }
        }

        private void BrowseThemeFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = Environment.CurrentDirectory;

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ThemeFileNameText.Text = openFileDialog.FileName;
            }
        }

        private void WatcherButton_Click(object sender, RoutedEventArgs e)
        {
            if(fileSystemWatcher != null)
            {
                fileSystemWatcher.EnableRaisingEvents = false;
                fileSystemWatcher.Dispose();
                fileSystemWatcher = null;

                WatcherButton.Content = "Start Watcher";
            }
            else
            {
                if(!Directory.Exists(SourceDirectoryText.Text) || !Directory.Exists(DestinationDirectoryText.Text) || !File.Exists(ThemeFileNameText.Text))
                {
                    System.Windows.MessageBox.Show("You need to specify a valid Source folder, Destination folder, and Theme file.", "Invalid paths", MessageBoxButton.OK, MessageBoxImage.Error);

                    return;
                }

                fileSystemWatcher = new FileSystemWatcher();
                fileSystemWatcher.Path = SourceDirectoryText.Text;
                fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite;
                fileSystemWatcher.Filter = "*.md";
                fileSystemWatcher.Changed += new FileSystemEventHandler(OnChanged);

                fileSystemWatcher.EnableRaisingEvents = true;

                sourceDirectory = SourceDirectoryText.Text;
                destinationDirectory = DestinationDirectoryText.Text;
                themeFileName = ThemeFileNameText.Text;
                titleName = TitleText.Text;

                WatcherButton.Content = "Stop Watcher";

                Process();
            }
        }

        private void Process()
        {
            var logMessage = "";

            StaticWikiCore.ProcessDirectory(sourceDirectory, destinationDirectory, themeFileName, titleName, ref logMessage);
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            if (!Directory.Exists(sourceDirectory) || !Directory.Exists(destinationDirectory) || !File.Exists(themeFileName))
            {
                return;
            }

            Process();
        }
    }
}
