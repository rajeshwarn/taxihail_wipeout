using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MK.Common.Android.Configuration.Impl;

namespace apcurium.MK.Common.Configuration.Impl
{
    public class ServerPaymentSettings : ClientPaymentSettings
    {
        public ServerPaymentSettings() //for serialization
        {
            BraintreeServerSettings = new BraintreeServerSettings();
                PayPalSettings = new PayPalSettings();   
        }

        public ServerPaymentSettings(Guid companyId) : this()
        {
            CompanyId = companyId;
        }
       
        [Key]
        public Guid CompanyId { get; set; }
        
        public BraintreeServerSettings BraintreeServerSettings { get; set; }

        public PayPalSettings PayPalSettings { get; set; }


    }
}
