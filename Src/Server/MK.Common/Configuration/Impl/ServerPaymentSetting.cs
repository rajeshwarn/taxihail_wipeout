using System;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Common.Configuration.Impl
{
    public class ServerPaymentSettings : ClientPaymentSettings
    {
        public ServerPaymentSettings() //for serialization
        {
        }

        public ServerPaymentSettings(Guid id)
        {
            Id = id;
            BraintreeServerSettings = new BraintreeServerSettings();
            PayPalServerSettings = new PayPalServerSettings(Guid.NewGuid());
        }

        [Key]
        public Guid Id { get; set; }

        
        public BraintreeServerSettings BraintreeServerSettings { get; set; }

        public PayPalServerSettings PayPalServerSettings { get; set; }
    }
}
