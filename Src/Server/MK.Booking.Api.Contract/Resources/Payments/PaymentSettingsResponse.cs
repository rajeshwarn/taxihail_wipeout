using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.Api.Contract.Resources.Payments
{
    public class PaymentSettingsResponse
    {
        public ClientPaymentSettings ClientPaymentSettings { get; set; }
    }
}
