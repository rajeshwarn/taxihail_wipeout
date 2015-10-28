#region

using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Enumeration;
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
        public string PickupZipCode { get; set; }
        public string DropoffZipCode { get; set; }
        public string AccountNumber { get; set; }
        public int? CustomerNumber { get; set; }
        public int? TripDurationInSeconds { get; set; }
        public int? VehicleType  { get; set; }
        public ServiceType ServiceType { get; set; }
        
    }

    public class IbsFareResponse : DirectionInfo
    {
    }
}