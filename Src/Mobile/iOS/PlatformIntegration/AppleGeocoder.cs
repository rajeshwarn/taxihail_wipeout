﻿using System;
using apcurium.MK.Booking.MapDataProvider;
using apcurium.MK.Booking.MapDataProvider.Resources;
using ServiceStack.ServiceClient.Web;
using MonoTouch.CoreLocation;
using System.Linq;
using MonoTouch.AddressBookUI;
using MonoTouch.Foundation;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Mobile.Client.PlatformIntegration
{
	public class AppleGeocoder : IGeocoder
	{
		public AppleGeocoder ()
		{
		}

		public GeoAddress[] GeocodeAddress (string address, string currentLanguage)
		{	
			// Do nothing with currentLanguage parameter since Apple Geocoder
			// automatically gets the results using the system language

			var geocoder = new CLGeocoder ();
			var result = geocoder.GeocodeAddressAsync (address.Replace ("+", " "));
			result.Wait ();
			if (result.Exception != null) {
				return new GeoAddress [0];
			}

			return result.Result.Select (ConvertPlacemarkToAddress).ToArray ();			
		}

		public GeoAddress[] GeocodeLocation (double latitude, double longitude, string currentLanguage)
		{
			// Do nothing with currentLanguage parameter since Apple Geocoder
			// automatically gets the results using the system language

			var geocoder = new CLGeocoder ();

			var result = geocoder.ReverseGeocodeLocationAsync (new CLLocation (latitude, longitude));
			result.Wait ();

			if (result.Exception != null) 
            {
				return new GeoAddress [0];
			}

			return result.Result.Select (ConvertPlacemarkToAddress).ToArray ();			
		}

		private GeoAddress ConvertPlacemarkToAddress (CLPlacemark placemark)
		{		
			var streetNumber = placemark.SubThoroughfare;
			if ((streetNumber != null) && (streetNumber.Any (c => c == '–'))) 
            {
				streetNumber = streetNumber.Substring (0, streetNumber.IndexOf ('–')); 
			}

            var t =  new GeoAddress 
            { 
				StreetNumber = streetNumber,
				Street = placemark.Thoroughfare,
				Latitude = placemark.Location.Coordinate.Latitude,
				Longitude = placemark.Location.Coordinate.Longitude,
				City = placemark.Locality,
                FullAddress = placemark.Description.Split(new []{" @"}, StringSplitOptions.RemoveEmptyEntries)[0],
				State = placemark.AdministrativeArea,
				ZipCode = placemark.PostalCode
			};

            return t;
		}
	}
}

