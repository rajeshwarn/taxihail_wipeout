
namespace apcurium.MK.Common.Resources
{
    public class InitializePayPalCheckoutResponse : BasePaymentResponse
    {
        public string PaymentId { get; set; }

        public string PayPalCheckoutUrl { get; set; }
    }
}
