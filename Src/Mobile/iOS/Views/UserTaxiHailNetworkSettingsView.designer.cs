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
	[Register ("UserTaxiHailNetworkSettingsView")]
	partial class UserTaxiHailNetworkSettingsView
	{
		[Outlet]
		MonoTouch.UIKit.NSLayoutConstraint constraintLeftLabel { get; set; }

		[Outlet]
		MonoTouch.UIKit.NSLayoutConstraint constraintRightSwitch { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel labelTaxiHailNetworkEnabled { get; set; }

		[Outlet]
		MonoTouch.UIKit.UISwitch switchTaxiHailNetworkEnabled { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITableView tableTaxiHailNetworkSettings { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (labelTaxiHailNetworkEnabled != null) {
				labelTaxiHailNetworkEnabled.Dispose ();
				labelTaxiHailNetworkEnabled = null;
			}

			if (switchTaxiHailNetworkEnabled != null) {
				switchTaxiHailNetworkEnabled.Dispose ();
				switchTaxiHailNetworkEnabled = null;
			}

			if (tableTaxiHailNetworkSettings != null) {
				tableTaxiHailNetworkSettings.Dispose ();
				tableTaxiHailNetworkSettings = null;
			}

			if (constraintLeftLabel != null) {
				constraintLeftLabel.Dispose ();
				constraintLeftLabel = null;
			}

			if (constraintRightSwitch != null) {
				constraintRightSwitch.Dispose ();
				constraintRightSwitch = null;
			}
		}
	}
}
