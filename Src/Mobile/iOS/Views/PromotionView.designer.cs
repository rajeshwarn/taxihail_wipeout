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
	[Register ("PromotionView")]
	partial class PromotionView
	{
		[Outlet]
		apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnApplyPromo { get; set; }

		[Outlet]
		UIKit.UILabel lblInstruction { get; set; }

		[Outlet]
		UIKit.UILabel lblNoPromotions { get; set; }

		[Outlet]
		UIKit.UITableView tblPromotions { get; set; }

		[Outlet]
		UIKit.UITextField txtPromoCode { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (txtPromoCode != null) {
				txtPromoCode.Dispose ();
				txtPromoCode = null;
			}

			if (btnApplyPromo != null) {
				btnApplyPromo.Dispose ();
				btnApplyPromo = null;
			}

			if (lblInstruction != null) {
				lblInstruction.Dispose ();
				lblInstruction = null;
			}

			if (lblNoPromotions != null) {
				lblNoPromotions.Dispose ();
				lblNoPromotions = null;
			}

			if (tblPromotions != null) {
				tblPromotions.Dispose ();
				tblPromotions = null;
			}
		}
	}
}
