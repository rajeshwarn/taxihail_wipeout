using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [RestService("/accounts/{AccountId}/addresses/history", "GET")]
    public class AddressHistoryRequest : BaseDTO
    {
        public Guid AccountId { get; set; }
    }
}
