namespace apcurium.MK.Booking.Api.Contract.Resources.Payments
{
    public class CommitPreauthorizedPaymentResponse : BasePaymentResponse
    {
        public string AuthorizationCode { get; set; }
    }
}