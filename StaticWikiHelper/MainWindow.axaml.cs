using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using StaticWiki;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StaticWikiHelper
{
    public partial class MainWindow : Window
    {
        private FileSystemWatcher fileSystemWatcher = null;

        private string sourceDirectory = "";
        private string destinationDirectory = "";
        private string defaultThemeName = "";
        private string titleName = "";
        private string navigationFileName = "";
        private KeyValuePair<string, string>[] themes = [];
        private string[] contentExtensions = [];
        private bool disableAutoPageExtension = false;
        private bool disableLinkCorrection = false;
        private string[] markdownExtensions = [];

        private bool autoUpdatesEnabled = true;
        private bool forceUpdate = false;

        private Thread workThread;
        private bool shouldTerminateWorkThread = false;
        private bool shouldUpdate = false;

        public MainWindow()
        {
            InitializeComponent();

            projectLoadedLabel.IsVisible = false;
            updateButton.IsVisible = false;

            Title = "Static Wiki Helper";

            Log("Starting Static Wiki");

            workThread = new Thread(new ParameterizedThreadStart((parameter) =>
            {
                for (; ; )
                {
                    var shouldProcessWork = false;

                    lock (this)
                    {
                        if (shouldTerminateWorkThread)
                        {
                            return;
                        }

                        shouldProcessWork = (shouldUpdate && autoUpdatesEnabled) || forceUpdate;

                        if (shouldProcessWork)
                        {
                            shouldUpdate = false;
                            forceUpdate = false;
                        }
                    }

                    if (shouldProcessWork)
                    {
                        Process();
                    }

                    Thread.Sleep(100);
                }
            }));

            workThread.Start(null);
        }

        private string LogFileName
        {
            get
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var appFolder = Path.Combine(path, "Static Wiki");

                if (!Directory.Exists(appFolder))
                {
                    Directory.CreateDirectory(appFolder);
                }

                return Path.Combine(appFolder, "StaticWiki.log");
            }
        }

        protected void Log(string message)
        {
            try
            {
                using var writer = File.Exists(LogFileName) == false ? File.CreateText(LogFileName) :
                    File.AppendText(LogFileName);

                writer.Write(string.Format("{0} - {1}\n", DateTime.Now.ToString(), message));
            }
            catch (Exception)
            {
                return;
            }
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            if (fileSystemWatcher != null)
            {
                fileSystemWatcher.EnableRaisingEvents = false;
                fileSystemWatcher.Dispose();
                fileSystemWatcher = null;
            }

            lock (this)
            {
                shouldTerminateWorkThread = true;
            }

            Log("Closing Static Wiki");

            base.OnClosing(e);
        }

        private void OpenProject(object? sender, RoutedEventArgs args)
        {
            if (fileSystemWatcher != null)
            {
                fileSystemWatcher.EnableRaisingEvents = false;
                fileSystemWatcher.Dispose();
                fileSystemWatcher = null;
            }

            noProjectLabel.IsVisible = true;
            projectLoadedLabel.IsVisible = false;
            updateButton.IsVisible = false;

            lock (this)
            {
                shouldUpdate = false;
            }

            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
                desktop.MainWindow?.StorageProvider is not { } provider)
            {
                throw new NullReferenceException("Missing StorageProvider instance.");
            }

            var task = provider.OpenFolderPickerAsync(new()
            {
                Title = "Open Project Folder",
                AllowMultiple = false,
            });

            Task.WaitAny(task);

            if (task.Result.Count == 0)
            {
                return;
            }

            var folder = task.Result[0];

            var basePath = Uri.UnescapeDataString(folder.Path.AbsolutePath);

            Log($"Attempting to op+en project at {basePath}");

            var logMessage = "";

            if (!StaticWikiCore.GetWorkspaceDetails(basePath, ref sourceDirectory, ref destinationDirectory, ref defaultThemeName,
                ref themes, ref titleName, ref navigationFileName, ref contentExtensions, ref disableAutoPageExtension,
                ref disableLinkCorrection, ref markdownExtensions, ref logMessage))
            {
                noProjectLabel.Content = $"Unable to load project details:\n{logMessage}";

                return;
            }

            fileSystemWatcher = new FileSystemWatcher();
            fileSystemWatcher.Path = sourceDirectory;
            fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime;
            fileSystemWatcher.Filter = "*.md";
            fileSystemWatcher.IncludeSubdirectories = true;
            fileSystemWatcher.Created += new FileSystemEventHandler(OnChanged);
            fileSystemWatcher.Deleted += new FileSystemEventHandler(OnChanged);
            fileSystemWatcher.Changed += new FileSystemEventHandler(OnChanged);
            fileSystemWatcher.Renamed += new RenamedEventHandler(OnRenamed);
            fileSystemWatcher.EnableRaisingEvents = true;

            Log("Successfully loaded project");

            noProjectLabel.IsVisible = false;

            projectLoadedLabel.IsVisible = true;
            projectLoadedLabel.Content = string.Format("Loaded '{0}'", titleName);

            updateButton.IsVisible = true;

            if (autoUpdatesEnabled)
            {
                lock (this)
                {
                    shouldUpdate = true;
                }
            }
        }

        private void EnableAutoUpdates(object? sender, RoutedEventArgs args)
        {
            lock (this)
            {
                autoUpdatesEnabled = true;
            }
        }

        private void DisableAutoUpdates(object? sender, RoutedEventArgs args)
        {
            lock (this)
            {
                autoUpdatesEnabled = false;
            }
        }

        private void HandleManualUpdate(object? sender, RoutedEventArgs args)
        {
            if (Directory.Exists(sourceDirectory) == false || themes.Any(x => File.Exists(x.Value) == false))
            {
                return;
            }

            lock (this)
            {
                forceUpdate = true;
            }
        }

        private void Process()
        {
            var logMessage = "";

            StaticWikiCore.ProcessDirectory(sourceDirectory, destinationDirectory, defaultThemeName, themes, navigationFileName, contentExtensions, titleName,
                disableAutoPageExtension, disableLinkCorrection, markdownExtensions, ref logMessage);

            if (logMessage.Length > 0)
            {
                Log(string.Format("Static Wiki Message: {0}", logMessage));
            }
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            if (!Directory.Exists(sourceDirectory) || themes.Any(x => !File.Exists(x.Value)) || !autoUpdatesEnabled)
            {
                return;
            }

            lock (this)
            {
                shouldUpdate = true;
            }
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            if (!Directory.Exists(sourceDirectory) || themes.Any(x => !File.Exists(x.Value)) || !autoUpdatesEnabled)
            {
                return;
            }

            lock (this)
            {
                shouldUpdate = true;
            }
        }
    }
}