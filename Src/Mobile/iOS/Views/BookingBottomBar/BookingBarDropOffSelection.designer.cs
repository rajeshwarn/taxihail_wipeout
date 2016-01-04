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

namespace apcurium.MK.Booking.Mobile.Client.Views.BookingBottomBar
{
    [Register("BookingBarDropOffSelection")]
    partial class BookingBarDropOffSelection
    {
        [Outlet]
        apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnCancel { get; set; }

        [Outlet]
        apcurium.MK.Booking.Mobile.Client.Controls.Widgets.FlatButton btnOk { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (btnOk != null) {
                btnOk.Dispose ();
                btnOk = null;
            }

            if (btnCancel != null) {
                btnCancel.Dispose ();
                btnCancel = null;
            }
        }
    }
}

