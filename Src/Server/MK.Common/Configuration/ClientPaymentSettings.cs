using System.Collections.Generic;
using MK.Common.Android.Configuration.Impl;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Common.Configuration
{

    public class ClientPaymentSettings
    {
		public ClientPaymentSettings ()
		{
			PaymentMode = PaymentMethod.Fake;
			CmtPaymentSettings = new CmtPaymentSettings();
		}
        public PaymentMethod PaymentMode { get; set; }
		public CmtPaymentSettings CmtPaymentSettings { get; set; }
    }
}
