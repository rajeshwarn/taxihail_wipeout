using apcurium.MK.Common.Cryptography;
namespace apcurium.MK.Common.Configuration.Impl
{
    public class BraintreeClientSettings
    {
        public BraintreeClientSettings()
        {
#if DEBUG
            ClientKey =
                "MIIBCgKCAQEAqdnjoSEUyK+va/Dsw5yJ37Mt7ac9lm93dE9aEH2C69GOBHXEj5OhVzqAOUe7mBrD4Ky9Wymfouj+VzpnINTMuTo5e+V6huzgbw7MBISmkgl2lAxvd5VMGcT4HXe3WGo2iaJHvnfVHe8Ipey19ngevmUXyl8AIOSF3AImANvnRnInNMNG0qxCxBzdPm6lNnSaM8acAPIBFX2y6d0BxasLAXzLgY0BDwzp6cdOMza9rYiPInS3WL20kLh9g2spQ/2KoUY4uS7yaD7hqtBfnLtkaceK6x3rnWmMT8mEOxvfrTIq+M/sVi8v1HIKfWNPsaOB8Wqfb9SSK3nk1OoucbV0sQIDAQAB";
#endif
        }

		[PropertyEncrypt]
        public string ClientKey { get; set; }

        public bool EnablePayPal { get; set; }
    }
}