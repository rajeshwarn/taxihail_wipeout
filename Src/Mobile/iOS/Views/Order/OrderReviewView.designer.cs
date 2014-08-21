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
		MonoTouch.UIKit.UIImageView iconApartment { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView iconNbLargeBags { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView iconNbPasserngers { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView iconPassengerName { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView iconPhone { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView iconRingCode { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblApt { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblChargeType { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblDate { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblName { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblNbLargeBags { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblNbPassengers { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblPhone { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblRingCode { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblVehicule { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextView txtNote { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (iconNbPasserngers != null) {
				iconNbPasserngers.Dispose ();
				iconNbPasserngers = null;
			}

			if (iconNbLargeBags != null) {
				iconNbLargeBags.Dispose ();
				iconNbLargeBags = null;
			}

			if (iconPassengerName != null) {
				iconPassengerName.Dispose ();
				iconPassengerName = null;
			}

			if (iconPhone != null) {
				iconPhone.Dispose ();
				iconPhone = null;
			}

			if (iconApartment != null) {
				iconApartment.Dispose ();
				iconApartment = null;
			}

			if (iconRingCode != null) {
				iconRingCode.Dispose ();
				iconRingCode = null;
			}

			if (lblApt != null) {
				lblApt.Dispose ();
				lblApt = null;
			}

			if (lblChargeType != null) {
				lblChargeType.Dispose ();
				lblChargeType = null;
			}

			if (lblDate != null) {
				lblDate.Dispose ();
				lblDate = null;
			}

			if (lblName != null) {
				lblName.Dispose ();
				lblName = null;
			}

			if (lblNbPassengers != null) {
				lblNbPassengers.Dispose ();
				lblNbPassengers = null;
			}

			if (lblNbLargeBags != null) {
				lblNbLargeBags.Dispose ();
				lblNbLargeBags = null;
			}

			if (lblPhone != null) {
				lblPhone.Dispose ();
				lblPhone = null;
			}

			if (lblRingCode != null) {
				lblRingCode.Dispose ();
				lblRingCode = null;
			}

			if (lblVehicule != null) {
				lblVehicule.Dispose ();
				lblVehicule = null;
			}

			if (txtNote != null) {
				txtNote.Dispose ();
				txtNote = null;
			}
		}
	}
}
