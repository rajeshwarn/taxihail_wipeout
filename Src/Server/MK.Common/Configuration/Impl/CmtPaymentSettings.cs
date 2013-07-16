using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Common.Configuration.Impl
{
    public class CmtPaymentSettings
    {
        public CmtPaymentSettings()
        {
#if DEBUG
            MerchantToken = "E4AFE87B0E864228200FA947C4A5A5F98E02AA7A3CFE907B0AD33B56D61D2D13E0A75F51641AB031500BD3C5BDACC114";
            CustomerKey = "vmAoqWEY3zIvUCM4";
            ConsumerSecretKey = "DUWzh0jAldPc7C5I";
            SandboxBaseUrl = "https://payment-sandbox.cmtapi.com/";
            BaseUrl = "https://payment.cmtapi.com/"; // for now will will not use production
            IsSandbox = true;
            CurrencyCode = CurrencyCodes.Main.UnitedStatesDollar;
#endif
        }

        public bool IsSandbox { get; set; }

        public  string BaseUrl { get; set; }

        public string ConsumerSecretKey { get; set; }

        public string SandboxBaseUrl { get; set; }
        
        public string CustomerKey { get; set; }

        public string MerchantToken { get; set; }

        public string CurrencyCode { get; set; }
        
    }
}
