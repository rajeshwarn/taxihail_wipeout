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

            IsPayInTaxiEnabled = false;
            AutomaticPayment = false;
            AutomaticPaymentPairing = false;
        }

        public PaymentMethod PaymentMode { get; set; }
        public bool IsPayInTaxiEnabled { get; set; }
        public bool AutomaticPayment { get; set; }
        public bool AutomaticPaymentPairing { get; set; }
        public CmtPaymentSettings CmtPaymentSettings { get; set; }
        public BraintreeClientSettings BraintreeClientSettings { get; set; }
        public MonerisPaymentSettings MonerisPaymentSettings { get; set; }
        public PayPalClientSettings PayPalClientSettings { get; set; }
    }
}