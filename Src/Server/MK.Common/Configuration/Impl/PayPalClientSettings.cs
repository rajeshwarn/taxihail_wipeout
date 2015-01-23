
namespace apcurium.MK.Common.Configuration.Impl
{
    public class PayPalClientSettings
    {
        public PayPalClientSettings()
        {
            IsSandbox = true;
            SandboxCredentials = new PayPalClientCredentials();
            Credentials = new PayPalClientCredentials();
        }

        public bool IsEnabled { get; set; }

        public bool IsSandbox { get; set; }

        public PayPalClientCredentials SandboxCredentials { get; set; }

        public PayPalClientCredentials Credentials { get; set; }

        public string ClientId
        {
            get
            {
                return IsSandbox
                    ? SandboxCredentials.ClientId
                    : Credentials.ClientId;
            }
        }
    }
}