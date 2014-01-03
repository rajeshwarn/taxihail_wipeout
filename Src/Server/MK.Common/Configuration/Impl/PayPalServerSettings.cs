namespace apcurium.MK.Common.Configuration.Impl
{
    public class PayPalServerSettings
    {
        public PayPalServerSettings()
        {
            SandboxCredentials = new PayPalCredentials();
            Credentials = new PayPalCredentials();
            IsSandbox = true;
        }

        public bool IsSandbox { get; set; }
        public PayPalCredentials SandboxCredentials { get; set; }
        public PayPalCredentials Credentials { get; set; }
    }
}