using apcurium.MK.Common.Cryptography;

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

		[PropertyEncrypt]
        public PayPalClientCredentials SandboxCredentials { get; set; }

		[PropertyEncrypt]
        public PayPalClientCredentials Credentials { get; set; }

		[PropertyEncrypt]
        public string CurrentEnvironmentClientId
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