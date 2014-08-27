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
	[Register ("NotificationSettingsView")]
	partial class NotificationSettingsView
	{
		[Outlet]
		MonoTouch.UIKit.UITableView tableNotifications { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (tableNotifications != null) {
				tableNotifications.Dispose ();
				tableNotifications = null;
			}
		}
	}
}
