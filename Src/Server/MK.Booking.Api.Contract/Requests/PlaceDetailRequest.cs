using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RestService("/places/{ReferenceId}", "GET")]
    public class PlaceDetailRequest
    {
        public string ReferenceId { get; set; }
        public string Name { get; set; }
    }
}
