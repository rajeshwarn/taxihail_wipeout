using apcurium.MK.Common.Resources;

namespace apcurium.MK.Booking.Services.Impl
{
    public class InitializeWebPaymentResponse : BasePaymentResponse
    {
        public string PaymentId { get; set; }

        public string PayerId { get; set; }

        public string PayPalCheckoutUrl { get; set; }
    }
}