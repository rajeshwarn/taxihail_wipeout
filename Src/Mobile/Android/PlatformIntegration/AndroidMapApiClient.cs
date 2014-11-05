#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using ServiceStack.ServiceClient.Web;
using apcurium.MK.Booking.MapDataProvider.Resources;
using apcurium.MK.Booking.MapDataProvider;
using System.Text;
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
			// Do nothing with currentLanguage parameter since Android Geocoder
			// automatically gets the results using the system language

			var geocoder = new Geocoder (_androidGlobals.ApplicationContext);

			try 
            {
                if (SettingsForGeocodingRegionAreSet)
                {
					var locations = geocoder.GetFromLocationName (address.Replace ("+", " "), 100, _settings.Data.LowerLeftLatitude.Value, _settings.Data.LowerLeftLongitude.Value, _settings.Data.UpperRightLatitude.Value, _settings.Data.UpperRightLongitude.Value);				
					return locations.Select (ConvertAddressToGeoAddress).ToArray ();			
				} 
                else 
                {
					var locations = geocoder.GetFromLocationName (address.Replace ("+", " "), 100);				
					return locations.Select (ConvertAddressToGeoAddress).ToArray ();			
				}
			} 
            catch (Exception ex) 
            {
				_logger.LogError (ex);
				return new GeoAddress [0];
			}
		}

		public GeoAddress[] GeocodeLocation (double latitude, double longitude, string currentLanguage)
		{
			// Do nothing with currentLanguage parameter since Android Geocoder
			// automatically gets the results using the system language

			var geocoder = new Geocoder (_androidGlobals.ApplicationContext);
            
			try 
            {
                var locations = geocoder.GetFromLocation(latitude, longitude, 25).Where(l => l.HasLatitude && l.HasLongitude);
				return locations.Select (ConvertAddressToGeoAddress).ToArray ();			
			} 
            catch (Exception ex) 
            {
				_logger.LogError (ex);
				return new GeoAddress [0];
			}
		}
        
		private GeoAddress ConvertAddressToGeoAddress (Address address)
		{		
			var streetNumber = address.SubThoroughfare;
			if ((streetNumber != null) && (streetNumber.Any (c => c == '-'))) 
            {
				streetNumber = streetNumber.Substring (0, streetNumber.IndexOf ('-')); 			
			}

			var full = GetFormatFullAddress (address);
            
			var r = new GeoAddress 
            { 
				StreetNumber = streetNumber,
				Street = address.Thoroughfare,
				Latitude = address.Latitude,
				Longitude = address.Longitude,
				City = address.Locality ?? address.SubLocality,
				FullAddress = full,
				State = address.AdminArea,
				ZipCode = address.PostalCode
			};

			return r;
		}

        private string GetFormatFullAddress(Address address)
        {
            var fullAddressComponents = new List<string>();
            for (int i = 0; i < address.MaxAddressLineIndex; i++)
            {
                var l = address.GetAddressLine(i);
                if (l.HasValue() && l.Split(' ').Length > 1 && l.Split(' ')[0].Contains("-"))
                {
                    var sNumber = l.Split(' ')[0].Split('-')[0];
                    l = sNumber + l.Split(' ').Skip(1).JoinBy(" ");
                }
                fullAddressComponents.Add(l);
            }
            var full = fullAddressComponents.JoinBy(", ");
            return full;
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

