// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	[Register ("UpdatedTermsAndConditionsView")]
	partial class UpdatedTermsAndConditionsView
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnAccept { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextView txtTermsAndConditions { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btnAccept != null) {
				btnAccept.Dispose ();
				btnAccept = null;
			}

			if (txtTermsAndConditions != null) {
				txtTermsAndConditions.Dispose ();
				txtTermsAndConditions = null;
			}
		}
	}
}
