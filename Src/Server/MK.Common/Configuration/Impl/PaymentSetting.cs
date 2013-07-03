using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MK.Common.Android.Configuration.Impl;

namespace apcurium.MK.Common.Configuration.Impl
{
    public class PaymentSetting : ClientPaymentSetting
    {
        public PaymentSetting() //for serialization
        {
            CmtPaymentSettings = new CmtPaymentSettings();
            BraintreeSettings = new BraintreeSettings();
        }

        public PaymentSetting(Guid companyId)
        {
            CompanyId = companyId;

            CmtPaymentSettings = new CmtPaymentSettings();
            BraintreeSettings = new BraintreeSettings();
        }
       
        [Key]
        public Guid CompanyId { get; set; }

        public CmtPaymentSettings CmtPaymentSettings { get; set; }

        public BraintreeSettings BraintreeSettings { get; set; }


    }
}
