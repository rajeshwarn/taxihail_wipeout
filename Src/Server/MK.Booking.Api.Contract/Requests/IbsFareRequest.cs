#region

using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/ibsfare/", "GET")]
    public class IbsFareRequest : IReturn<IbsFareResponse>
    {
        public double PickupLatitude { get; set; }
        public double PickupLongitude { get; set; }
        public double DropoffLatitude { get; set; }
        public double DropoffLongitude { get; set; }
    }

    public class IbsFareResponse : DirectionInfo
    {
    }
}