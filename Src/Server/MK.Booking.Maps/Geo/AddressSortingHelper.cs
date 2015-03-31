using System.Globalization;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Maps.Geo
{
    public static class AddressSortingHelper
    {
        public static double GetPositionByRelevance(Address adrs, string name, double? latitude, double? longitude)
        {
            var nameToUse = (name ?? string.Empty).ToLowerInvariant();

            var isNamePresentInAddress =
                adrs.FriendlyName.SelectOrDefault(friendlyName => friendlyName.ToLowerInvariant().Contains(nameToUse)) ||
                adrs.FullAddress.SelectOrDefault(fullAddress => fullAddress.ToLowerInvariant().Contains(nameToUse));

            if (isNamePresentInAddress)
            {
                return -.01d;
            }

            if (latitude.HasValue && longitude.HasValue)
            {
                return Position.CalculateDistance(adrs.Latitude, adrs.Longitude, latitude.Value, longitude.Value);
            }

            return 0d;
        }
    }
}