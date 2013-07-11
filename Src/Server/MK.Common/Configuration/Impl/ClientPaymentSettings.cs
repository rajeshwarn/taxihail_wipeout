using System.Collections.Generic;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Common.Configuration
{

    public class ClientPaymentSettings
    {
		public ClientPaymentSettings ()
		{
			PaymentMode = PaymentMethod.Fake;
			CmtPaymentSettings = new CmtPaymentSettings();
            BraintreeClientSettings = new BraintreeClientSettings();
		    PayPalClientSettings = new PayPalClientSettings();
		}
        public PaymentMethod PaymentMode { get; set; }
        public CmtPaymentSettings CmtPaymentSettings { get; set; }
        public BraintreeClientSettings BraintreeClientSettings { get; set; }

        public PayPalClientSettings PayPalClientSettings { get; set; }
    }
}
