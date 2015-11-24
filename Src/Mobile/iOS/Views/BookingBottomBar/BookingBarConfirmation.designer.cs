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
	[Register ("BookingBarConfirmation")]
	partial class BookingBarConfirmation
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.AppBarLabelButton buttonEdit { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.AppBarLabelButton buttonCancel { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton buttonConfirm { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (buttonCancel != null) {
				buttonCancel.Dispose ();
				buttonCancel = null;
			}

			if (buttonConfirm != null) {
				buttonConfirm.Dispose ();
				buttonConfirm = null;
			}

			if (buttonEdit != null) {
				buttonEdit.Dispose ();
				buttonEdit = null;
			}
		}
	}
}
