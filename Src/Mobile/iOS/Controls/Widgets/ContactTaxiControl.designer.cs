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
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnCallDriver { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnMessageDriver { get; set; }

		[Outlet]
		UIKit.UIImageView imgPhone { get; set; }

		[Outlet]
		UIKit.UILabel lblMedallion { get; set; }

		[Outlet]
		UIKit.UILabel lblMedallionTitle { get; set; }

		[Outlet]
		UIKit.UIView view { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (btnCallDriver != null) {
				btnCallDriver.Dispose ();
				btnCallDriver = null;
			}

			if (btnMessageDriver != null) {
				btnMessageDriver.Dispose ();
				btnMessageDriver = null;
			}

			if (imgPhone != null) {
				imgPhone.Dispose ();
				imgPhone = null;
			}

			if (lblMedallion != null) {
				lblMedallion.Dispose ();
				lblMedallion = null;
			}

			if (lblMedallionTitle != null) {
				lblMedallionTitle.Dispose ();
				lblMedallionTitle = null;
			}

			if (view != null) {
				view.Dispose ();
				view = null;
			}
		}
	}
}
