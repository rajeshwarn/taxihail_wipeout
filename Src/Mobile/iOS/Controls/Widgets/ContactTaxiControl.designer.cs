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
	[Register ("ContactTaxiControl")]
	partial class ContactTaxiControl
	{
		[Outlet]
		UIKit.UIButton btnCallDriver { get; set; }

		[Outlet]
		UIKit.UIImageView imgPhone { get; set; }

		[Outlet]
		UIKit.UILabel lblMedallion { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (lblMedallion != null) {
				lblMedallion.Dispose ();
				lblMedallion = null;
			}

			if (imgPhone != null) {
				imgPhone.Dispose ();
				imgPhone = null;
			}

			if (btnCallDriver != null) {
				btnCallDriver.Dispose ();
				btnCallDriver = null;
			}
		}
	}
}
