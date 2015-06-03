using System;
using apcurium.MK.Booking.MapDataProvider;
using apcurium.MK.Booking.MapDataProvider.Resources;
using CoreLocation;
using System.Linq;
using Foundation;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class AppleGeocoder : IGeocoder
    {

        public AppleGeocoder ()
        {
        }

        public GeoAddress[] GeocodeAddress (string address, string currentLanguage)
        {
            try
            {
                return GeocodeAddressAsync(address, currentLanguage).Result;
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException as NSErrorException;
                if (inner != null)
                {
                    Console.WriteLine(inner.Error.GetNSErrorString());
                }
                throw;
            }
        }

        public async Task<GeoAddress[]> GeocodeAddressAsync(string address, string currentLanguage)
        {
            // Do nothing with currentLanguage parameter since Apple Geocoder
            // automatically gets the results using the system language

            var geocoder = new CLGeocoder();

            var placemarks = await geocoder.GeocodeAddressAsync(address.Replace("+"," ")).ConfigureAwait(false);

            return placemarks
                .Select(ConvertPlacemarkToAddress)
                .ToArray();
        }

        public GeoAddress[] GeocodeLocation (double latitude, double longitude, string currentLanguage)
        {
            // Do nothing with currentLanguage parameter since Apple Geocoder
            // automatically gets the results using the system language

            try{
                var geocoder = new CLGeocoder ();

                var result = geocoder.ReverseGeocodeLocationAsync (new CLLocation (latitude, longitude));
                result.Wait ();

                if (result.Exception != null) 
                {
                    return new GeoAddress [0];
                }
                return result.Result.Select (ConvertPlacemarkToAddress).ToArray (); 

            }catch(Exception ex)
            {
                var inner = ex.InnerException as NSErrorException;
                if (inner != null)
                {
                    Console.WriteLine(inner.Error.GetNSErrorString());
                }
                throw;
            }
                 
        }

        private GeoAddress ConvertPlacemarkToAddress (CLPlacemark placemark)
        {       
            var streetNumber = ConvertStreetNumberRangeToSingle(placemark.SubThoroughfare);
            var fullAddress = ConvertAddressDictionaryToFullAddress(placemark.AddressDictionary);

            // replace corrected street number in the full address
            if (streetNumber.HasValue())
            {
                fullAddress = fullAddress.Replace(placemark.SubThoroughfare, streetNumber);
            }

            var geoAddress = new GeoAddress  
            { 
                StreetNumber = streetNumber,
                Street = placemark.Thoroughfare,
                Latitude = placemark.Location.Coordinate.Latitude,
                Longitude = placemark.Location.Coordinate.Longitude,
                City = placemark.Locality ?? placemark.SubLocality,
                FullAddress = fullAddress,
                State = placemark.AdministrativeArea,
                ZipCode = placemark.PostalCode
            };

            return geoAddress;
        }

        private string ConvertStreetNumberRangeToSingle(string subThoroughFare)
        {
            var streetNumber = subThoroughFare;

            if (streetNumber != null 
                && (streetNumber.Contains("-") || streetNumber.Contains("-") || streetNumber.Contains("–"))) // careful, these are not the same characters!
            {
                if (streetNumber.Contains("-"))
                {
                    // this is a special case for Queens where we want to keep the dash with the 2 values
                    // see: http://stackoverflow.com/questions/2783155/how-to-distinguish-a-ny-queens-style-street-address-from-a-ranged-address-and
                }
                else if (streetNumber.Contains("-"))
                {
                    streetNumber = streetNumber.Substring(0, streetNumber.IndexOf('-'));
                }
                else
                {
                    streetNumber = streetNumber.Substring(0, streetNumber.IndexOf('–'));
                }
            }

            return streetNumber;
        }

        private string ConvertAddressDictionaryToFullAddress(NSDictionary addressDictionary)
        {
            var addressLines = (NSArray)NSArray.FromObject(addressDictionary.ValueForKey(new NSString("FormattedAddressLines")));

            var arr = new List<string>();
            for (uint i = 0; i < addressLines.Count; i++) 
            {
                var o = ObjCRuntime.Runtime.GetNSObject(addressLines.ValueAt(i));
                arr.Add(o.ToString());
            }
            return string.Join(", ", arr.ToArray());
        }
    }
}

