
using CustomerPortal.Contract.Resources.Payment;

namespace CustomerPortal.Contract.Resources
{
    public class CompanyPaymentSettings
    {
        public CompanyPaymentSettings()
        {
            CmtPaymentSettings = new CmtPaymentSettings();
            BraintreePaymentSettings = new BraintreePaymentSettings();
            MonerisPaymentSettings = new MonerisPaymentSettings();
        }

        public CmtPaymentSettings CmtPaymentSettings { get; set; }

        public BraintreePaymentSettings BraintreePaymentSettings { get; set; }

        public MonerisPaymentSettings MonerisPaymentSettings { get; set; }
    }
}
