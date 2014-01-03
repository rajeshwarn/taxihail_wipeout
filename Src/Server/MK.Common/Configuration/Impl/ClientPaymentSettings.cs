#region



#endregion

namespace apcurium.MK.Common.Configuration.Impl
{
    public class ClientPaymentSettings
    {
        public ClientPaymentSettings()
        {
            PaymentMode = PaymentMethod.None;
            CmtPaymentSettings = new CmtPaymentSettings();
            BraintreeClientSettings = new BraintreeClientSettings();
            PayPalClientSettings = new PayPalClientSettings();
            IsPayInTaxiEnabled = false;
        }

        public PaymentMethod PaymentMode { get; set; }
        public bool IsPayInTaxiEnabled { get; set; }
        public CmtPaymentSettings CmtPaymentSettings { get; set; }
        public BraintreeClientSettings BraintreeClientSettings { get; set; }

        public PayPalClientSettings PayPalClientSettings { get; set; }
    }
}