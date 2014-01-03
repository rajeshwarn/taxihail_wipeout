using System.Linq;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common;


namespace apcurium.MK.Booking.Mobile.Extensions
{
    public static class AddressExtension
    {
        public static string Display( this Address instance )
        {
            return Params.Get( instance.FriendlyName ,instance.DisplayAddress ).Where( s=>s.HasValue() ).JoinBy( " - " );             
        }

        public static bool HasValidCoordinate(this Address instance)
        {
            if ( instance == null )
            {
                return false;
            }

// ReSharper disable CompareOfFloatsByEqualityOperator
            return instance.Longitude != 0 
                        && instance.Latitude != 0 
                        && instance.Latitude >= -90 
                        && instance.Latitude <= 90 
                        && instance.Longitude >= -180 
                        && instance.Longitude <= 180;
// ReSharper restore CompareOfFloatsByEqualityOperator
        }


        public static bool IsSame(this Address instance, Address data)
        {
            return (instance.FullAddress.SoftEqual(data.FullAddress) &&
                    instance.RingCode.SoftEqual(data.RingCode) &&
                    instance.Apartment.SoftEqual(data.Apartment));
        }
    }
}