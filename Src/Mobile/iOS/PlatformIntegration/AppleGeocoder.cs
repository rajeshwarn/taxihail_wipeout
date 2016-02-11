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
using MapKit;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
    public class AppleGeocoder : IGeocoder
    {

        public AppleGeocoder ()
        {
        }

        public GeoAddress[] GeocodeAddress (string query, string currentLanguage, double? pickupLatitude, double? pickupLongitude, double searchRadiusInMeters)
        {
            try
            {
                return GeocodeAddressAsync(query, currentLanguage, pickupLatitude, pickupLongitude, searchRadiusInMeters).Result;
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

        public async Task<GeoAddress[]> GeocodeAddressAsync(string query, string currentLanguage, double? pickupLatitude, double? pickupLongitude, double searchRadiusInMeters)
        {
            // Do nothing with currentLanguage parameter since Apple Geocoder
            // automatically gets the results using the system language
            var placemarks = await SearchAsync(query, pickupLatitude, pickupLongitude, searchRadiusInMeters);

            var geoAddresses = placemarks
                .Select(ConvertPlacemarkToAddress)
                .ToArray();
            
            return geoAddresses;
        }

        private Task<MKPlacemark[]> SearchAsync(string query, double? lat, double? lng, double radiusInMeters)
        {
            var tcs = new TaskCompletionSource<MKPlacemark[]>();
            var result = new MKPlacemark[0];

            var o = new NSObject ();
            o.InvokeOnMainThread(async () =>
            {
                try
                {
                    var searchRequest = new MKLocalSearchRequest { NaturalLanguageQuery = query };

                    if(lat.HasValue && lng.HasValue 
                        && lat.Value != 0 && lng.Value != 0)
                    {
                        // You can use this parameter to narrow the list of search results to those inside or close to the specified region. 
                        // Specifying a region does not guarantee that the results will all be inside the region. It is merely a hint to the search engine. 

                        var region = MKCoordinateRegion.FromDistance(new CLLocationCoordinate2D(lat.Value, lng.Value), radiusInMeters * 2, radiusInMeters * 2);
                        searchRequest.Region = region;
                    }

                    var search = new MKLocalSearch(searchRequest);

                    var searchResult = await search.StartAsync();
                    result = searchResult.MapItems.Select(x => x.Placemark).ToArray();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    tcs.TrySetResult(result);
                }
            });

            return tcs.Task;
        }

        public GeoAddress[] GeocodeLocation (double latitude, double longitude, string currentLanguage)
        {
            try
            {
                return GeocodeLocationAsync(latitude, longitude, currentLanguage).Result;
            }
            catch(Exception ex)
            {
                var inner = ex.InnerException as NSErrorException;
                if (inner != null)
                {
                    Console.WriteLine(inner.Error.GetNSErrorString());
                }
                throw;
            }
        }

        public async Task<GeoAddress[]> GeocodeLocationAsync(double latitude, double longitude, string currentLanguage)
        {
            var geocoder = new CLGeocoder();

            var placemarks = await geocoder.ReverseGeocodeLocationAsync(new CLLocation(latitude, longitude));

            return placemarks
                .Select(ConvertPlacemarkToAddress)
                .ToArray();
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

