using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Common.Configuration.Impl
{
    public class PayPalSettings
    {
        public PayPalSettings()
        {
            PayPalSandboxCredentials = new PayPalCredentials();
            PayPalCredentials = new PayPalCredentials();
            IsSandBox = true;

        }
        public bool IsSandBox {get; set;}

        public PayPalCredentials PayPalSandboxCredentials { get; set; }
        public PayPalCredentials PayPalCredentials { get; set; }
    }
}
