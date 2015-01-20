namespace apcurium.MK.Common.Configuration.Impl
{
    public class PayPalServerCredentials
    {
        public PayPalServerCredentials()
        {
#if DEBUG
            Username = "vincent.costel-facilitator_api1.gmail.com";
            Password = "1372362468";
            Secret = "EB3vYRDLhiEa5r12VhMb2pXCtyz33jeeeluQgMLQ4yDjaDD7W7M8Vo81iBg1";
#endif
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public string Secret { get; set; }
    }
}