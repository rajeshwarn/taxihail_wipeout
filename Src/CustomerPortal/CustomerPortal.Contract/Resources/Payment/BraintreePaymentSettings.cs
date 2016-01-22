
namespace CustomerPortal.Contract.Resources.Payment
{
    public class BraintreePaymentSettings
    {
        public bool IsSandbox { get; set; }

        public string MerchantId { get; set; }

        public string MerchantAccountId { get; set; }

        public string PublicKey { get; set; }

        public string PrivateKey { get; set; }

        public string ClientKey { get; set; }
    }
}
