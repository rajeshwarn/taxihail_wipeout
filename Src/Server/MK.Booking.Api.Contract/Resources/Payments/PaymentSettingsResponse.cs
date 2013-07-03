using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Api.Contract.Resources.Payments
{
    public class PaymentSettingsResponse
    {
        public ClientPaymentSettings ClientPaymentSettings { get; set; }
    }
}
