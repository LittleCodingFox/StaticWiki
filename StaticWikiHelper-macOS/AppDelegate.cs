using AppKit;
using Foundation;

namespace StaticWikiHelpermacOS
{
    [Register("AppDelegate")]
    public class AppDelegate : NSApplicationDelegate
    {
        public AppDelegate()
        {
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            // Insert code here to initialize your application
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }

        public override bool ApplicationShouldHandleReopen(NSApplication sender, bool hasVisibleWindows)
        {
            if(!hasVisibleWindows)
            {
                for (int n = 0; n < NSApplication.SharedApplication.Windows.Length; ++n)
				{
					var content = NSApplication.SharedApplication.Windows[n].ContentViewController as ViewController;

                    if (content != null)
					{
						// Bring window to front - MakeKey makes it active as well
						NSApplication.SharedApplication.Windows[n].MakeKeyAndOrderFront(this);
						return true;
					}
				}
            }

            return false;
        }
    }
}
