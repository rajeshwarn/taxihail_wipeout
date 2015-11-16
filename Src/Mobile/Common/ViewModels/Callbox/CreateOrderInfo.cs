namespace apcurium.MK.Booking.Mobile.ViewModels.Callbox
{
    public class CreateOrderInfo
    {
        public string PassengerName { get; set; }
        public bool IsPendingCreation { get; set; }
        public CallboxOrderViewModel Order { get; set; }
    }
}