namespace apcurium.MK.Booking.Api.Contract.Resources.Payments
{
    public class BasePaymentResponse
    {
        public bool IsSuccessful { get; set; }

        public string Message { get; set; }
    }
}