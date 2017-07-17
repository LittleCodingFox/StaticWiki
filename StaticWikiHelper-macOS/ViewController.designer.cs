// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace StaticWikiHelpermacOS
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AppKit.NSButton autoUpdatesCheckBox { get; set; }

		[Outlet]
		AppKit.NSTextField noProjectLabel { get; set; }

		[Outlet]
		AppKit.NSTextField projectLoadedLabel { get; set; }

		[Outlet]
		AppKit.NSButton updateNowButton { get; set; }

		[Action ("autoUpdateButtonPressed:")]
		partial void autoUpdateButtonPressed (Foundation.NSObject sender);

		[Action ("updateNowButtonPressed:")]
		partial void updateNowButtonPressed (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (noProjectLabel != null) {
				noProjectLabel.Dispose ();
				noProjectLabel = null;
			}

			if (projectLoadedLabel != null) {
				projectLoadedLabel.Dispose ();
				projectLoadedLabel = null;
			}

			if (autoUpdatesCheckBox != null) {
				autoUpdatesCheckBox.Dispose ();
				autoUpdatesCheckBox = null;
			}

			if (updateNowButton != null) {
				updateNowButton.Dispose ();
				updateNowButton = null;
			}
		}
	}
}
