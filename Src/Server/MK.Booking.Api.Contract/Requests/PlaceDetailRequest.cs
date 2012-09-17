using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Authenticate]
    [RestService("/places/{ReferenceId}", "GET, OPTIONS")]
    public class PlaceDetailRequest
    {
        public string ReferenceId { get; set; }
    }
}
