using System;

namespace apcurium.MK.Booking.Mobile.TaxihailEventArgs
{
    public class BookingStatusChangedEventArgs : EventArgs
    {
        public string CarNumber { get; set; }
        public bool ShouldCloseWaitingCarLandscapeView { get; set; }
    }
}