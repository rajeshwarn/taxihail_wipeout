using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/places/detail", "GET")]
    public class PlaceDetailRequest : IReturn<Address>
    {
        public string ReferenceId { get; set; }
        public string PlaceName { get; set; }
    }
}
