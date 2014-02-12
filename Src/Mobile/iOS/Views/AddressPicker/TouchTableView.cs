using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client.Views.AddressPicker
{
    public class TouchTableView : UITableView
    {

        public event EventHandler OnTouchesBegan;

        public TouchTableView(RectangleF frame, UITableViewStyle style )  : base( frame, style )
        {
        }

        public override void TouchesBegan(MonoTouch.Foundation.NSSet touches, UIEvent evt)
        {
            if (OnTouchesBegan != null)
            {
                OnTouchesBegan(this, EventArgs.Empty);
            }
            base.TouchesBegan(touches, evt);
        }
    }
}

