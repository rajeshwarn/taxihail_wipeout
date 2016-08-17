using apcurium.MK.Common.Services;

namespace apcurium.MK.Common.Configuration.Impl
{
    public class BraintreeServerSettings
    {
        public BraintreeServerSettings()
        {
#if DEBUG
            MerchantAccountId = "r5ftmsvsjksgrrjq";
            MerchantId = "8xv86vbp9cy3cv96";
            PrivateKey = "eaee7d483323a971beb07edfc91880ae";
            PublicKey = "33nbqvrwjg7hy2n5";
            IsSandbox = true;

#endif
        }

        public bool IsSandbox { get; set; }

		[PropertyEncrypt]
        public string MerchantId { get; set; }

        [PropertyEncrypt]
        public string MerchantAccountId { get; set; }

        [PropertyEncrypt]
        public string PublicKey { get; set; }

		[PropertyEncrypt]
        public string PrivateKey { get; set; }
    }
}