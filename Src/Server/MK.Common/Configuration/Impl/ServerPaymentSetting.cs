#region

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
            PreAuthAmount = 200;
        }

        [Key]
        public Guid Id { get; set; }

        public BraintreeServerSettings BraintreeServerSettings { get; set; }
        public PayPalServerSettings PayPalServerSettings { get; set; }

        public decimal? NoShowFee { get; set; }

        public decimal? PreAuthAmount { get; set; }
    }
}