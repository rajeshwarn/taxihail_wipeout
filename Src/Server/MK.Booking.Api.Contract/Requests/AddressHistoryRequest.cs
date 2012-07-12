using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RestService("/accounts/{AccountId}/addresses/history", "GET")]
    public class AddressHistoryRequest
    {
        public Guid AccountId { get; set; }
    }
}
