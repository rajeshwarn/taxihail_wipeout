using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common;


namespace apcurium.MK.Booking.Mobile.Extensions
{
    public static class AddressExtension
    {

        public static Address Copy(this Address instance)
        {
            var copy = new Address();
            copy.Id = instance.Id;
            copy.FriendlyName = instance.FriendlyName;
            copy.FullAddress = instance.FullAddress;
            copy.Longitude = instance.Longitude;
            copy.Latitude = instance.Latitude;
            copy.Apartment = instance.Apartment;
            copy.RingCode = instance.RingCode;

            return copy;
        }

        public static string Display( this Address instance )
        {
            return Params.Get( instance.FriendlyName ,instance.FullAddress ).Where( s=>s.HasValue() ).JoinBy( " - " );             
        }

        public static bool HasValidCoordinate(this Address instance)
        {
            return instance.Longitude != 0 && instance.Latitude != 0 && instance.Latitude >= -90 && instance.Latitude <= 90 && instance.Longitude >= -180 && instance.Longitude <= 180;
        }


        public static bool IsSame(this Address instance, Address data)
        {
            return (instance.FullAddress.SoftEqual(data.FullAddress) &&
                    instance.RingCode.SoftEqual(data.RingCode) &&
                    instance.Apartment.SoftEqual(data.Apartment));
        }



    }
}