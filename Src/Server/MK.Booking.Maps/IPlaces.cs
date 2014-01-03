#region

using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.Maps
{
    public interface IPlaces
    {
        Address GetPlaceDetail(string name, string referenceId);
        Address[] SearchPlaces(string name, double? latitude, double? longitude, int? radius);
    }
}