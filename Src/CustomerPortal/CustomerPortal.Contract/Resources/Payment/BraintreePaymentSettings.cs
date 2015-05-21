using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerPortal.Contract.Resources.Payment
{
    public class BraintreePaymentSettings
    {
        public BraintreePaymentSettings()
        {
#if DEBUG
            MerchantId = "8xv86vbp9cy3cv96";
            PrivateKey = "eaee7d483323a971beb07edfc91880ae";
            PublicKey = "33nbqvrwjg7hy2n5";
            ClientKey = "MIIBCgKCAQEAqdnjoSEUyK+va/Dsw5yJ37Mt7ac9lm93dE9aEH2C69GOBHXEj5OhVzqAOUe7mBrD4Ky9Wymfouj+VzpnINTMuTo5e+V6huzgbw7MBISmkgl2lAxvd5VMGcT4HXe3WGo2iaJHvnfVHe8Ipey19ngevmUXyl8AIOSF3AImANvnRnInNMNG0qxCxBzdPm6lNnSaM8acAPIBFX2y6d0BxasLAXzLgY0BDwzp6cdOMza9rYiPInS3WL20kLh9g2spQ/2KoUY4uS7yaD7hqtBfnLtkaceK6x3rnWmMT8mEOxvfrTIq+M/sVi8v1HIKfWNPsaOB8Wqfb9SSK3nk1OoucbV0sQIDAQAB";
            IsSandbox = true;
#endif
        }

        public bool IsSandbox { get; set; }

        public string MerchantId { get; set; }

        public string PublicKey { get; set; }

        public string PrivateKey { get; set; }

        public string ClientKey { get; set; }
    }
}
