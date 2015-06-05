using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/addressFromPostalCode", "POST")]
    public class GetAddressesFromPostcalCodeRequest
    {
        public string PostalCode { get; set; }
    }
}
