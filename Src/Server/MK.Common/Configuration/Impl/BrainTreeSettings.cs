namespace apcurium.MK.Common.Configuration.Impl
{
    public class BraintreeServerSettings
    {
        public BraintreeServerSettings()
        {
#if DEBUG

            MerchantId = "v3kjnzjzhv8z37pq";
            PrivateKey = "92780e4aa457e9269b1910d88ac79d17";
            PublicKey = "d268b7by244xnvw9";
            IsSandbox = true;

#endif
        }

        public bool IsSandbox { get; set; }

        public string MerchantId { get; set; }

        public string PublicKey { get; set; }

        public string PrivateKey { get; set; }
    }
}