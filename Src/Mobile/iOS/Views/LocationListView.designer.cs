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
    [Register ("LocationListView")]
    partial class LocationListView
	{
		[Outlet]
		UIKit.UITableView tableLocations { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (tableLocations != null) {
				tableLocations.Dispose ();
				tableLocations = null;
			}
		}
	}
}
