using System;

namespace apcurium.MK.Common.Configuration.Impl
{
    public class ClientPaymentSettings
    {
        public ClientPaymentSettings()
        {
            PaymentMode = PaymentMethod.None;
            CmtPaymentSettings = new CmtPaymentSettings();
            BraintreeClientSettings = new BraintreeClientSettings();
			MonerisPaymentSettings = new MonerisPaymentSettings();
            PayPalClientSettings = new PayPalClientSettings();

            IsChargeAccountPaymentEnabled = false;
            IsPayInTaxiEnabled = false;
            IsOutOfAppPaymentDisabled = false;
        }

        public PaymentMethod PaymentMode { get; set; }
        
        /// <summary>
        /// In app payment
        /// </summary>
        public bool IsPayInTaxiEnabled { get; set; }
        
        /// <summary>
        /// Manual payment, not through app
        /// </summary>
        public bool IsOutOfAppPaymentDisabled { get; set; }

        public bool IsChargeAccountPaymentEnabled { get; set; }

        [Obsolete("This property is deprecated. It is only kept to support older versions.", false)]
        public bool AutomaticPaymentPairing { get; set; }

        

        public CmtPaymentSettings CmtPaymentSettings { get; set; }
        public BraintreeClientSettings BraintreeClientSettings { get; set; }
        public MonerisPaymentSettings MonerisPaymentSettings { get; set; }
        public PayPalClientSettings PayPalClientSettings { get; set; }
    }
}