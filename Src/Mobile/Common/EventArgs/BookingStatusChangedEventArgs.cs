namespace apcurium.MK.Booking.Mobile.EventArgs
{
    public class BookingStatusChangedEventArgs : System.EventArgs
    {
        public string CarNumber { get; set; }
        public bool ShouldCloseWaitingCarLandscapeView { get; set; }
    }
}