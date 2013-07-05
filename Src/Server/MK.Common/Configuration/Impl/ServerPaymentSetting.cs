using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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


        }

        public ServerPaymentSettings(Guid companyId)

        {
            CompanyId = companyId;
            BraintreeServerSettings = new BraintreeServerSettings();
            PayPalSettings = new PayPalSettings(Guid.NewGuid());
        }
       
        [Key]
        public Guid CompanyId { get; set; }
        
        public BraintreeServerSettings BraintreeServerSettings { get; set; }

        public PayPalSettings PayPalSettings { get; set; }
    }
}
