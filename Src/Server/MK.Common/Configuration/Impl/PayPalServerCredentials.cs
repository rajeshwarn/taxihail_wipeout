namespace apcurium.MK.Common.Configuration.Impl
{
    public class PayPalServerCredentials
    {
        public PayPalServerCredentials()
        {
#if DEBUG
            Secret = "EB3vYRDLhiEa5r12VhMb2pXCtyz33jeeeluQgMLQ4yDjaDD7W7M8Vo81iBg1";
#endif
        }

        public string Secret { get; set; }
    }
}