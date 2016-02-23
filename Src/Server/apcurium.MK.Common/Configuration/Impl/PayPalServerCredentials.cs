using apcurium.MK.Common.Cryptography;
namespace apcurium.MK.Common.Configuration.Impl
{
    public class PayPalServerCredentials
    {
        public PayPalServerCredentials()
        {
#if DEBUG
            Secret = "EB3vYRDLhiEa5r12VhMb2pXCtyz33jeeeluQgMLQ4yDjaDD7W7M8Vo81iBg1";
            MerchantId = "WFYLSDQLU5TK6";
#endif
        }

		[PropertyEncrypt]
        public string Secret { get; set; }

		[PropertyEncrypt]
        public string MerchantId { get; set; }
    }
}