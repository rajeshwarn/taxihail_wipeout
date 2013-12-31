using System;

namespace apcurium.MK.Booking.Mobile.Client.Views.Payments
{
    public delegate void DraggedHandler (object sender,EventArgs e);

        public class DraggedEventArgs : EventArgs
    {
        public DraggedEventArgs (float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X;
        public float Y;
    }

}

