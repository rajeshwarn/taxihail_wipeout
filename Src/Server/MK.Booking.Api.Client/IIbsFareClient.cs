#region

using apcurium.MK.Booking.Api.Contract.Resources;

#endregion

namespace apcurium.MK.Booking.Api.Client
{
    public interface IIbsFareClient
    {
        DirectionInfo GetDirectionInfoFromIbs(double pickupLatitude, double pickupLongitude, double dropoffLatitude,
            double dropofLongitude);
    }
}