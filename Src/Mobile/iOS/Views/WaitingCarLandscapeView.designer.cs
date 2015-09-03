// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	[Register ("WaitingCarLandscapeView")]
	partial class WaitingCarLandscapeView
	{
		[Outlet]
		UIKit.UILabel carNumberLabel { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton closeButton { get; set; }

		[Outlet]
		UIKit.UIView mainView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (carNumberLabel != null) {
				carNumberLabel.Dispose ();
				carNumberLabel = null;
			}

			if (closeButton != null) {
				closeButton.Dispose ();
				closeButton = null;
			}

			if (mainView != null) {
				mainView.Dispose ();
				mainView = null;
			}
		}
	}
}
