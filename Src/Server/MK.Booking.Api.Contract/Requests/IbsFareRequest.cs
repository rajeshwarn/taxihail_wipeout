using System.Collections.Generic;
using ServiceStack.ServiceHost;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RestService("/ibsfare/", "GET")]
    public class IbsFareRequest : IReturn<IbsFareResponse>
    {
        public double PickupLatitude { get; set; }
        public double PickupLongitude { get; set; }
        public double DropoffLatitude { get; set; }
        public double DropoffLongitude { get; set; }
    }

    public class IbsFareResponse : DirectionInfo
    {
        public IbsFareResponse()
        {
            
        }
    }
}
