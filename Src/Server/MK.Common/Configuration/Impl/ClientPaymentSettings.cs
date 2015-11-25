using apcurium.MK.Common.Cryptography;
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
            AskForCVVAtBooking = false;
            CancelOrderOnUnpair = false;
            CreditCardIsMandatory = false;
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

        public bool AskForCVVAtBooking { get; set; }

        public bool CancelOrderOnUnpair { get; set; }

        public bool CreditCardIsMandatory { get; set; }

		[PropertyEncrypt]
        public CmtPaymentSettings CmtPaymentSettings { get; set; }

		[PropertyEncrypt]
        public BraintreeClientSettings BraintreeClientSettings { get; set; }

		[PropertyEncrypt]
        public MonerisPaymentSettings MonerisPaymentSettings { get; set; }

		[PropertyEncrypt]
        public PayPalClientSettings PayPalClientSettings { get; set; }

        public SupportedPaymentMethod SupportedPaymentMethod
        {
            get
            {
                if (PayPalClientSettings.IsEnabled && IsPayInTaxiEnabled)
                {
                    return SupportedPaymentMethod.Multiple;
                }

                if (PayPalClientSettings.IsEnabled)
                {
                    return SupportedPaymentMethod.PayPalOnly;
                }

                if (IsPayInTaxiEnabled)
                {
                    return SupportedPaymentMethod.CreditCardOnly;
                }

                return SupportedPaymentMethod.None;
            }
        }
    }

    public enum SupportedPaymentMethod
    {
        None,
        CreditCardOnly,
        PayPalOnly,
        Multiple
    }
}