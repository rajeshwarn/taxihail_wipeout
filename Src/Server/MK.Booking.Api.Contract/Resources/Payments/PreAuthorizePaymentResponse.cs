namespace apcurium.MK.Booking.Api.Contract.Resources.Payments
{
    public class PreAuthorizePaymentResponse : BasePaymentResponse
    {
        public string TransactionId { get; set; }
    }
}