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
            BraintreeSettings = new BraintreeSettings();
        }

        public ServerPaymentSettings(Guid companyId)
        {
            CompanyId = companyId;

            BraintreeSettings = new BraintreeSettings();
        }
       
        [Key]
        public Guid CompanyId { get; set; }
        
        public BraintreeSettings BraintreeSettings { get; set; }


    }
}
