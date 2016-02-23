using apcurium.MK.Common.Configuration.Impl;

using CustomerPortal.Contract.Resources.Payment;

namespace CustomerPortal.Contract.Resources
{
    public class CompanyPaymentSettings
    {
        public PaymentMethod PaymentMode { get; set; }

        public CmtPaymentSettings CmtPaymentSettings { get; set; }

        public BraintreePaymentSettings BraintreePaymentSettings { get; set; }

        public MonerisPaymentSettings MonerisPaymentSettings { get; set; }
    }
}
