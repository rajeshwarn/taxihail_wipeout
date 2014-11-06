using System.Linq;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.Extensions
{
    public static class AddressExtension
    {
        public static string Display(this Address instance)
        {
            return Params.Get(instance.FriendlyName, instance.DisplayAddress).Where(s => s.HasValue()).JoinBy(" - ");             
        }

        public static bool HasValidCoordinate(this Address instance)
        {
            if (instance == null)
            {
                return false;
            }

            return instance.Longitude != 0 
                        && instance.Latitude != 0 
                        && instance.Latitude >= -90 
                        && instance.Latitude <= 90 
                        && instance.Longitude >= -180 
                        && instance.Longitude <= 180;
        }
    }
}