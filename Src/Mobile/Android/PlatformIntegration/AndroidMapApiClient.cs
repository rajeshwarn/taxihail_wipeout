#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Android.OS;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.MapDataProvider.Resources;
using apcurium.MK.Booking.MapDataProvider;
using System.Text;
using System.Threading.Tasks;
using MK.Booking.MapDataProvider.Foursquare;
using Android.Locations;
using Cirrious.CrossCore.Droid;
using MK.Common.Configuration;

#endregion
namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
	public class AndroidGeocoder : IGeocoder
	{
		private readonly IAppSettings _settings;
		private readonly IMvxAndroidGlobals _androidGlobals;
		private readonly ILogger _logger;

		public AndroidGeocoder (IAppSettings settings, ILogger logger, IMvxAndroidGlobals androidGlobals)
		{
			_androidGlobals = androidGlobals;
			_logger = logger;
			_settings = settings;
		}

		public GeoAddress[] GeocodeAddress (string address, string currentLanguage)
		{
			return GeocodeAddressAsync(address, currentLanguage).Result;
		}

	    public async Task<GeoAddress[]> GeocodeAddressAsync(string address, string currentLanguage)
	    {
            // Do nothing with currentLanguage parameter since Android Geocoder
            // automatically gets the results using the system language
            var geocoder = new Geocoder (_androidGlobals.ApplicationContext);

	        var locationsTask = SettingsForGeocodingRegionAreSet 
                ? geocoder.GetFromLocationNameAsync(address.Replace("+", " "), 100,_settings.Data.LowerLeftLatitude.Value, _settings.Data.LowerLeftLongitude.Value,_settings.Data.UpperRightLatitude.Value, _settings.Data.UpperRightLongitude.Value)
                : geocoder.GetFromLocationNameAsync(address.Replace("+", " "), 100);
	        try
	        {
                var locations = await locationsTask;

                return locations.Select(ConvertAddressToGeoAddress).ToArray();
	        }
	        catch (Exception ex)
	        {
                _logger.LogError(ex);
                return new GeoAddress[0];
	        }
	        		
	    }

	    public GeoAddress[] GeocodeLocation (double latitude, double longitude, string currentLanguage)
	    {
	        return GeocodeLocationAsync(latitude, longitude, currentLanguage).Result;
	    }

	    public async Task<GeoAddress[]> GeocodeLocationAsync(double latitude, double longitude, string currentLanguage)
	    {
            // Do nothing with currentLanguage parameter since Android Geocoder
            // automatically gets the results using the system language
            var geocoder = new Geocoder(_androidGlobals.ApplicationContext);

            try
            {
                var locations = await geocoder.GetFromLocationAsync(latitude, longitude, 25);
                return locations
                    .Where(l => l.HasLatitude && l.HasLongitude)
                    .Select(ConvertAddressToGeoAddress)
                    .ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return new GeoAddress[0];
            }
	    }

	    private GeoAddress ConvertAddressToGeoAddress (Address address)
		{		
            var streetNumber = ConvertStreetNumberRangeToSingle(address.SubThoroughfare, address.PostalCode);
            var fullAddress = GetFormatFullAddress(address);

            // replace corrected street number in the full address
            if (streetNumber.HasValue())
            {
                fullAddress = fullAddress.Replace(address.SubThoroughfare, streetNumber);
            }

			var geoAddress = new GeoAddress 
            { 
				StreetNumber = streetNumber,
				Street = address.Thoroughfare,
				Latitude = address.Latitude,
				Longitude = address.Longitude,
				City = address.Locality ?? address.SubLocality,
                FullAddress = fullAddress,
				State = address.AdminArea,
				ZipCode = address.PostalCode
			};

			return geoAddress;
		}

        private string GetFormatFullAddress(Address address)
        {
            // address object contains address lines used for displaying an address on Android on separate lines
            // we combine them with ", " to have the full address on a single line
            var fullAddressComponents = new List<string>();
            for (int i = 0; i < address.MaxAddressLineIndex; i++)
            {
                var addressLine = address.GetAddressLine(i);
                fullAddressComponents.Add(addressLine);
            }
            var full = fullAddressComponents.JoinBy(", ");
            return full;
        }

        private string ConvertStreetNumberRangeToSingle(string subThoroughFare, string zipCode)
        {
            var streetNumber = subThoroughFare;

            // Android geocoder doesn't differentiate the dash character for Queens vs normal range of addresses
            if (streetNumber == null || !streetNumber.Contains("-"))
            {
                return streetNumber;
            }
            
            if (streetNumber.Count(x => x == '-') == 3)
            {
                // a range of Queens formatted address
                var positionOfFirstDash = streetNumber.IndexOf('-');
                return streetNumber.Substring(0, streetNumber.IndexOf('-', positionOfFirstDash + 1));
            }

            if (!zipCode.HasValue())
            {
                return streetNumber.Substring(0, streetNumber.IndexOf('-'));
            }

            // leave Queens addresses intact
            if (!ListOfQueensZipCodes.Contains(zipCode))
            {
                return streetNumber.Substring(0, streetNumber.IndexOf('-'));
            }

            return streetNumber;
        }

        private IEnumerable<string> ListOfQueensZipCodes
	    {
	        get
	        {
	            return new[]
	            {
	                "11433", "11434", "11692", "11101", "11102", "11103", "11104", "11105", "11106", "11107", "11108",
	                "11109", "11359", "11360", "11361", "11364", "11357", "11694", "11426", "11427", "11428", "11424", 
                    "11697", "11435", "11693", "11411", "11356", "11368", "11362", "11363", "11369", "11370", "11371", 
                    "11690", "11373", "11379", "11691", "11004", "11005", "11351", "11354", "11355", "11358", "11375", 
                    "11695", "11365", "11366", "11385", "11423", "11414", "11372", "11412", "11413", "11415", "11416",
                    "11417", "11418", "11419", "11432", "11367", "11378", "11429", "11374", "11422", "11420", "11436", 
                    "11421", "11377"
	            };
	        }
	    }

        private bool SettingsForGeocodingRegionAreSet
        {
            get
            {
                return
                    new[]
	                {
	                    _settings.Data.LowerLeftLatitude, 
                        _settings.Data.LowerLeftLongitude, 
                        _settings.Data.UpperRightLatitude,
	                    _settings.Data.UpperRightLongitude
	                }.All(d => d.HasValue);
            }
        }
	}
}

