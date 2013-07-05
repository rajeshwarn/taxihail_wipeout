using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Common.Configuration.Impl
{
    public class BraintreeServerSettings
    {
        public BraintreeServerSettings()
        {
            MerchantId = "v3kjnzjzhv8z37pq";
            PublicKey = "d268b7by244xnvw9";
            PrivateKey = "92780e4aa457e9269b1910d88ac79d17";
            IsSandbox = true;
        }

        public bool IsSandbox { get; set; }

        public string MerchantId { get; set; }

        public string PublicKey { get; set; }

        public string PrivateKey { get; set; }

    }
}
