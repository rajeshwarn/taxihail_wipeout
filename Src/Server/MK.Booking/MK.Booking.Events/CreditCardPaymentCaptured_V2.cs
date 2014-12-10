namespace apcurium.MK.Booking.Events
{
    public class CreditCardPaymentCaptured_V2 : CreditCardPaymentCaptured
    {
        public decimal Tax { get; set; }
    }
}