namespace apcurium.MK.Common.Configuration.Impl
{
    public class BraintreeServerSettings
    {
        public BraintreeServerSettings()
        {
#if DEBUG

            MerchantId = "8xv86vbp9cy3cv96";
            PrivateKey = "eaee7d483323a971beb07edfc91880ae";
            PublicKey = "33nbqvrwjg7hy2n5";
            IsSandbox = true;

#endif
        }

        public bool IsSandbox { get; set; }

        public string MerchantId { get; set; }

        public string PublicKey { get; set; }

        public string PrivateKey { get; set; }
    }
}