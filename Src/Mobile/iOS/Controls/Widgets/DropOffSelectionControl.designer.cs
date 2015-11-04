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
	[Register ("DropOffSelectionControl")]
	partial class DropOffSelectionControl
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.AddressTextBox viewDestination { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (viewDestination != null) {
				viewDestination.Dispose ();
				viewDestination = null;
			}
		}
	}
}
