
namespace apcurium.MK.Common.Configuration.Impl
{
    public class PayPalServerSettings
    {
        public PayPalServerSettings()
        {
            SandboxCredentials = new PayPalServerCredentials();
            Credentials = new PayPalServerCredentials();
        }

        public PayPalServerCredentials SandboxCredentials { get; set; }

        public PayPalServerCredentials Credentials { get; set; }
    }
}