using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Api.Contract.Resources.Payments
{
    [NoCache]
    public class PaymentSettingsResponse
    {
        public ClientPaymentSettings ClientPaymentSettings { get; set; }
    }
}
