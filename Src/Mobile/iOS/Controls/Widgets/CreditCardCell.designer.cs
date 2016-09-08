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

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register ("CreditCardCell")]
    partial class CreditCardCell
    {
        [Outlet]
        UIKit.UIView backgroundView { get; set; }


        [Outlet]
        UIKit.UILabel lblLabel { get; set; }


        [Outlet]
        apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatTextField txtCardNumber { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView Background { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (Background != null) {
                Background.Dispose ();
                Background = null;
            }

            if (lblLabel != null) {
                lblLabel.Dispose ();
                lblLabel = null;
            }

            if (txtCardNumber != null) {
                txtCardNumber.Dispose ();
                txtCardNumber = null;
            }
        }
    }
}