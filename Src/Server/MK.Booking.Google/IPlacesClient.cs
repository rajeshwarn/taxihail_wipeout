using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Google.Resources;

namespace apcurium.MK.Booking.Google
{
    public interface IPlacesClient
    {
        Place[] GetNearbyPlaces(double latitude, double longitude, string languageCode, bool sensor, int radius);
    }
}
