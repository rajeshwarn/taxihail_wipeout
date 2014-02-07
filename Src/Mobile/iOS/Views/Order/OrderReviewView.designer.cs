// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace apcurium.MK.Booking.Mobile.Client.Views.Order
{
	[Register ("OrderReviewView")]
	partial class OrderReviewView
	{
		[Outlet]
		MonoTouch.UIKit.UILabel lblApt { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblChargeType { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblDate { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblName { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblNbPassengers { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblPhone { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblRingCode { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblVehicule { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (lblName != null) {
				lblName.Dispose ();
				lblName = null;
			}

			if (lblPhone != null) {
				lblPhone.Dispose ();
				lblPhone = null;
			}

			if (lblNbPassengers != null) {
				lblNbPassengers.Dispose ();
				lblNbPassengers = null;
			}

			if (lblDate != null) {
				lblDate.Dispose ();
				lblDate = null;
			}

			if (lblVehicule != null) {
				lblVehicule.Dispose ();
				lblVehicule = null;
			}

			if (lblChargeType != null) {
				lblChargeType.Dispose ();
				lblChargeType = null;
			}

			if (lblApt != null) {
				lblApt.Dispose ();
				lblApt = null;
			}

			if (lblRingCode != null) {
				lblRingCode.Dispose ();
				lblRingCode = null;
			}
		}
	}
}
