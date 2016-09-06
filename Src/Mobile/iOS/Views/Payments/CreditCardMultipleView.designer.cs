// WARNING
//
// This file has been generated automatically by Xamarin Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
    [Register ("CreditCardMultipleView")]
    partial class CreditCardMultipleView
    {
        [Outlet]
        apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnAddCard { get; set; }


        [Outlet]
        UIKit.UILabel lblPaymentMethods { get; set; }


        [Outlet]
        UIKit.UILabel lblTip { get; set; }


        [Outlet]
        UIKit.UITableView tblCreditCards { get; set; }


        [Outlet]
        apcurium.MK.Booking.Mobile.Client.Controls.Widgets.ModalFlatTextField txtTip { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (btnAddCard != null) {
                btnAddCard.Dispose ();
                btnAddCard = null;
            }

            if (lblPaymentMethods != null) {
                lblPaymentMethods.Dispose ();
                lblPaymentMethods = null;
            }

            if (lblTip != null) {
                lblTip.Dispose ();
                lblTip = null;
            }

            if (tblCreditCards != null) {
                tblCreditCards.Dispose ();
                tblCreditCards = null;
            }

            if (txtTip != null) {
                txtTip.Dispose ();
                txtTip = null;
            }
        }
    }
}