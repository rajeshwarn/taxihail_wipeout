// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//

using apcurium.MK.Booking.Mobile.Client.Controls;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	[Register ("RideSettingsView")]
	partial class RideSettingsView
	{
		[Outlet]
		MonoTouch.UIKit.UIView Container { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblName { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtName { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblPhone { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblVehicleType { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblChargeType { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField txtPhone { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.ModalTextField pickerVehiculeType { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.ModalTextField pickerChargeType { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIScrollView scrollView { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblPassword { get; set; }

		[Outlet]
		FormLabel lblTipAmount { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.NavigateTextField txtPassword { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblCreditCard { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView TipSlider { get; set; }

		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.CreditCardButton btnCreditCard { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel lblOptional { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (Container != null) {
				Container.Dispose ();
				Container = null;
			}

			if (lblName != null) {
				lblName.Dispose ();
				lblName = null;
			}

			if (txtName != null) {
				txtName.Dispose ();
				txtName = null;
			}

			if (lblPhone != null) {
				lblPhone.Dispose ();
				lblPhone = null;
			}

			if (lblVehicleType != null) {
				lblVehicleType.Dispose ();
				lblVehicleType = null;
			}

			if (lblChargeType != null) {
				lblChargeType.Dispose ();
				lblChargeType = null;
			}

			if (txtPhone != null) {
				txtPhone.Dispose ();
				txtPhone = null;
			}

			if (pickerVehiculeType != null) {
				pickerVehiculeType.Dispose ();
				pickerVehiculeType = null;
			}

			if (pickerChargeType != null) {
				pickerChargeType.Dispose ();
				pickerChargeType = null;
			}

			if (scrollView != null) {
				scrollView.Dispose ();
				scrollView = null;
			}

			if (lblPassword != null) {
				lblPassword.Dispose ();
				lblPassword = null;
			}

			if (lblTipAmount != null) {
				lblTipAmount.Dispose ();
				lblTipAmount = null;
			}

			if (txtPassword != null) {
				txtPassword.Dispose ();
				txtPassword = null;
			}

			if (lblCreditCard != null) {
				lblCreditCard.Dispose ();
				lblCreditCard = null;
			}

			if (TipSlider != null) {
				TipSlider.Dispose ();
				TipSlider = null;
			}

			if (btnCreditCard != null) {
				btnCreditCard.Dispose ();
				btnCreditCard = null;
			}

			if (lblOptional != null) {
				lblOptional.Dispose ();
				lblOptional = null;
			}
		}
	}
}
