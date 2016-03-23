﻿using apcurium.MK.Common.Cryptography;
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
            IsPaymentOutOfAppDisabled = OutOfAppPaymentDisabled.None;
            AskForCVVAtBooking = false;
            DisableAMEX = false;
            DisableVisaMastercard = false;
            DisableDiscover = false;
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
        [Obsolete("This property is deprecated, use 'IsPaymentOutOfAppDisabled' instead. Now, Out Of App Payment can be enabled for web only", false)]
        public bool IsOutOfAppPaymentDisabled { get; set; }

        /// <summary>
        /// Manual payment, not through app
        /// </summary>
        public OutOfAppPaymentDisabled IsPaymentOutOfAppDisabled { get; set; }

        public bool IsChargeAccountPaymentEnabled { get; set; }

        [Obsolete("This property is deprecated. It is only kept to support older versions.", false)]
        public bool AutomaticPaymentPairing { get; set; }

        public bool AskForCVVAtBooking { get; set; }

        public bool DisableAMEX { get; set; }

        public bool DisableVisaMastercard { get; set; }

        public bool DisableDiscover { get; set; }

        public bool CancelOrderOnUnpair { get; set; }

        public bool CreditCardIsMandatory { get; set; }

		[PropertyEncrypt]
        public CmtPaymentSettings CmtPaymentSettings { get; set; }

		[PropertyEncrypt]
        public BraintreeClientSettings BraintreeClientSettings { get; set; }

        public PayPalClientSettings PayPalClientSettings { get; set; }

		[PropertyEncrypt]
        public MonerisPaymentSettings MonerisPaymentSettings { get; set; }

        public SupportedPaymentMethod SupportedPaymentMethod
        {
            get
            {
                return IsPayInTaxiEnabled 
                    ? SupportedPaymentMethod.CreditCardOnly 
                    : SupportedPaymentMethod.None;
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

    public enum OutOfAppPaymentDisabled
    {
        /// <summary>
        /// Default value for migration. Don't use it.
        /// </summary>
        NotSet,
        /// <summary>
        /// Out of App payment enabled for Web and App
        /// </summary>
        None,
        /// <summary>
        /// Out of App payment enabled for Web only
        /// </summary>
        AppOnly,
        /// <summary>
        /// Out of App payment disabled for Web and App
        /// </summary>
        All
    }
}