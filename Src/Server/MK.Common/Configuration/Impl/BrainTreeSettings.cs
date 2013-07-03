using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Common.Configuration.Impl
{
    public class BraintreeSettings
    {
        public BraintreeSettings()
        {
            MerchantId = "v3kjnzjzhv8z37pq";
            PublicKey = "d268b7by244xnvw9";
            PrivateKey = "92780e4aa457e9269b1910d88ac79d17";
            ClientKey = "MIIBCgKCAQEAoj1SlyPOlcbsemg8jNsZkgBjYspWd8goY7Dyf7IAMi68s6lX1QkEZ5iRVDZW8WANT46bbXFSZcerULT9nUx9lMWP8rrcv+i7Qy9LGjj2Zys7D0b98mzcdOoYiAg1GKDWjDW49mEtzlRbTSpgETvzCt3tonqAgZKt5E68P8SkQX+lem7N06KwaW5jFJRYNkYc5cNTyo3pMoCGnWJvBLMuW1CV4dXWxvTQU8dgnug6Y/i0AVJGJtnH2iaqk40+w6mifzpjDI6luTFw9ZXI7wlXitrQDcE0a3Dqx896IdvqP7PNLi6zVGM2DtOojO5f5KIXiFcBkepYnDkzJ33L1iwTKQIDAQAB";
            IsSandBox = true;
        }

        public bool IsSandBox { get; set; }

        public string MerchantId { get; set; }

        public string PublicKey { get; set; }

        public string PrivateKey { get; set; }

        public string ClientKey { get; set; }
    }
}
