using System;
using System.IO;
using System.Threading;
using AppKit;
using Foundation;
using StaticWiki;

namespace StaticWikiHelpermacOS
{
    public partial class ViewController : NSViewController
    {
		private FileSystemWatcher fileSystemWatcher;

		private string sourceDirectory = "";
		private string destinationDirectory = "";
		private string themeFileName = "";
		private string titleName = "";
		private string navigationFileName = "";
		private string[] contentExtensions = new string[0];
		private bool disableAutoPageExtension = false;
		private bool disableLinkCorrection = false;
		private string[] markdownExtensions = new string[0];

		private bool autoUpdatesEnabled = true;

		private Thread workThread;
		private bool shouldTerminateWorkThread = false;
		private bool shouldUpdate = false;
		
        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

			// Do any additional setup after loading the view.

			Log("Starting Static Wiki");

			workThread = new Thread(new ParameterizedThreadStart((parameter) =>
			{
				for (;;)
				{
					var shouldProcessWork = false;

					lock (this)
					{
						if (shouldTerminateWorkThread)
						{
							return;
						}

						shouldProcessWork = shouldUpdate && autoUpdatesEnabled;

						if (shouldProcessWork)
						{
							shouldUpdate = false;
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

        public override void ViewWillDisappear()
        {
            base.ViewWillDisappear();

			if (fileSystemWatcher != null)
			{
				fileSystemWatcher.EnableRaisingEvents = false;
				fileSystemWatcher.Dispose();
				fileSystemWatcher = null;
			}

			lock (this)
			{
                shouldUpdate = false;
			}

            noProjectLabel.Hidden = false;
            projectLoadedLabel.Hidden = true;
            updateNowButton.Hidden = true;
		}

		private string logFileName
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
				StreamWriter writer = null;

				if (!File.Exists(logFileName))
				{
					writer = File.CreateText(logFileName);
				}
				else
				{
					writer = File.AppendText(logFileName);
				}

				if (writer != null)
				{
					writer.Write(string.Format("{0} - {1}\n", DateTime.Now.ToString(), message));
				}

				writer.Close();
			}
			catch (Exception)
			{
				return;
			}
		}

		private void Process()
		{
			var logMessage = "";

			StaticWikiCore.ProcessDirectory(sourceDirectory, destinationDirectory, themeFileName, navigationFileName, contentExtensions, titleName,
				disableAutoPageExtension, disableLinkCorrection, markdownExtensions, ref logMessage);

			if (logMessage.Length > 0)
			{
				Log(string.Format("Static Wiki Message: {0}", logMessage));
			}
		}

		private void OnChanged(object source, FileSystemEventArgs e)
		{
			if (!Directory.Exists(sourceDirectory) || !File.Exists(themeFileName) || !autoUpdatesEnabled)
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
			if (!Directory.Exists(sourceDirectory) || !File.Exists(themeFileName) || !autoUpdatesEnabled)
			{
				return;
			}

			lock (this)
			{
				shouldUpdate = true;
			}
		}

		public override NSObject RepresentedObject
        {
            get
            {
                return base.RepresentedObject;
            }
            set
            {
                base.RepresentedObject = value;
                // Update the view, if already loaded.
            }
        }

        [Export("openDocument:")]
        void OpenProject(NSObject sender)
        {
            var dialog = NSOpenPanel.OpenPanel;
            dialog.CanChooseFiles = false;
            dialog.CanChooseDirectories = true;

			if(dialog.RunModal() == 1)
            {
				if (fileSystemWatcher != null)
				{
					fileSystemWatcher.EnableRaisingEvents = false;
					fileSystemWatcher.Dispose();
					fileSystemWatcher = null;
				}

				lock (this)
				{
					shouldUpdate = false;
				}
				
                var basePath = dialog.Url.Path;

				Log(string.Format("Attempting to open project at '{0}'", basePath));

                noProjectLabel.Hidden = false;
                projectLoadedLabel.Hidden = true;
				updateNowButton.Hidden = true;

				string logMessage = "";

				if (!StaticWikiCore.GetWorkspaceDetails(basePath, ref sourceDirectory, ref destinationDirectory, ref themeFileName, ref titleName, ref navigationFileName, ref contentExtensions,
					ref disableAutoPageExtension, ref disableLinkCorrection, ref markdownExtensions, ref logMessage))
				{
                    var alert = new NSAlert()
                    {
                        AlertStyle = NSAlertStyle.Warning,
                        InformativeText = "Unable to load project details",
                        MessageText = "Error"
                    };

                    alert.RunModal();

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

                noProjectLabel.Hidden = true;

                projectLoadedLabel.Hidden = false;
                projectLoadedLabel.StringValue = string.Format("Loaded '{0}'", titleName);

                updateNowButton.Hidden = false;

				if (autoUpdatesEnabled)
				{
					lock (this)
					{
						shouldUpdate = true;
					}
				}
			}
        }

        partial void autoUpdateButtonPressed(NSObject sender)
        {
			lock (this)
			{
                autoUpdatesEnabled = autoUpdatesCheckBox.State == NSCellStateValue.On;
			}
		}

        partial void updateNowButtonPressed(NSObject sender)
        {
			if (!Directory.Exists(sourceDirectory) || !File.Exists(themeFileName))
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
