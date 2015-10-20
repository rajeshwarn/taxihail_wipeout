// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
	[Register ("ChangeDropOffControl")]
	partial class ChangeDropOffControl
	{
		[Outlet]
		UIKit.UIImageView imgDropOffIcon { get; set; }

		[Outlet]
		UIKit.UILabel lblChangeDropOff { get; set; }

		[Outlet]
		UIKit.UIView view { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (imgDropOffIcon != null) {
				imgDropOffIcon.Dispose ();
				imgDropOffIcon = null;
			}

			if (lblChangeDropOff != null) {
				lblChangeDropOff.Dispose ();
				lblChangeDropOff = null;
			}

			if (view != null) {
				view.Dispose ();
				view = null;
			}
		}
	}
}
