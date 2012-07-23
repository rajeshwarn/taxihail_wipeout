using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RestService("/accounts/{AccountId}/addresses", "GET")]    
    public class Addresses
    {
        public Guid AccountId{ get; set; }
    }
}
