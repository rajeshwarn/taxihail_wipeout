#region

using apcurium.MK.Common.Cryptography;
using System;
using System.ComponentModel.DataAnnotations;

#endregion

namespace apcurium.MK.Common.Configuration.Impl
{
    public class ServerPaymentSettings : ClientPaymentSettings
    {
        public ServerPaymentSettings() //for serialization
        {
            Id = AppConstants.CompanyId;
            BraintreeServerSettings = new BraintreeServerSettings();
            PayPalServerSettings = new PayPalServerSettings();
            IsPreAuthEnabled = true;
            PreAuthAmount = 200;
            UnpairingTimeOut = 120;
            IsUnpairingDisabled = false;
            IsPrepaidEnabled = false;
            AlwaysDisplayCoFOption = false;
        }

        [Key]
        public Guid Id { get; set; }

		[PropertyEncrypt]
        public string CompanyKey { get; set; }

		[PropertyEncrypt]
        public BraintreeServerSettings BraintreeServerSettings { get; set; }

		[PropertyEncrypt]
        public PayPalServerSettings PayPalServerSettings { get; set; }

        [Obsolete("This property is deprecated. It is only kept to support older versions.", false)]
        public decimal? NoShowFee { get; set; }

        public bool IsPreAuthEnabled { get; set; }

        public decimal? PreAuthAmount { get; set; }

        public bool IsUnpairingDisabled { get; set; }

        public int UnpairingTimeOut { get; protected set; }

        /// <summary>
        /// This setting is more a "IsWebPaymentEnabled", Braintree is prepaid, CMT is normal payment
        /// </summary>
        public bool IsPrepaidEnabled { get; set; }
        
        public bool AlwaysDisplayCoFOption { get; set; }
    }
}