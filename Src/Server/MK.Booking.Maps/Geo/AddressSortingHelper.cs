using System.Globalization;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Maps.Geo
{
    public static class AddressSortingHelper
    {
        private const double ORDER_WHEN_NAME_PRESENT = -.01d;
        private const double DEFAULT_ORDER = 0d;

        public static double GetRelevance(Address adrs, string name, double? latitude, double? longitude)
        {
            var nameToUse = (name ?? string.Empty).ToLowerInvariant();

			var isNamePresentInAddress =
				(adrs.FriendlyName.SelectOrDefault(friendlyName => friendlyName.ToLowerInvariant().Contains(nameToUse))
				|| adrs.FullAddress.SelectOrDefault(fullAddress => fullAddress.ToLowerInvariant().Contains(nameToUse)))
				&& !string.IsNullOrWhiteSpace(name);

            if (isNamePresentInAddress)
            {
                return ORDER_WHEN_NAME_PRESENT;
            }

            if (latitude.HasValue && longitude.HasValue)
            {
                return Position.CalculateDistance(adrs.Latitude, adrs.Longitude, latitude.Value, longitude.Value);
            }

            return DEFAULT_ORDER;
        }
	}
}