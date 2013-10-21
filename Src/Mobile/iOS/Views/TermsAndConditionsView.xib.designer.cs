// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace apcurium.MK.Booking.Mobile.Client
{
	[Register ("TermsAndConditionsView")]
	partial class TermsAndConditionsView
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.GradientButton btnAccept { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.GradientButton btnCancel { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblAcknowledgement { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.FormLabel lblTitle { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.TextView txtTermsAndConditions { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (lblTitle != null) {
				lblTitle.Dispose ();
				lblTitle = null;
			}

			if (txtTermsAndConditions != null) {
				txtTermsAndConditions.Dispose ();
				txtTermsAndConditions = null;
			}

			if (btnAccept != null) {
				btnAccept.Dispose ();
				btnAccept = null;
			}

			if (btnCancel != null) {
				btnCancel.Dispose ();
				btnCancel = null;
			}

			if (lblAcknowledgement != null) {
				lblAcknowledgement.Dispose ();
				lblAcknowledgement = null;
			}
		}
	}
}
