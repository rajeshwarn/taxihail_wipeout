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
using Android.Locations;
using apcurium.MK.Booking.Mobile.Infrastructure;
using MK.Common.iOS.Patterns;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    public class LocationListener : Java.Lang.Object, ILocationListener, IObservable<Position>
    {

		List<IObserver<Position>> _observers;
		
		public Position LastKnownPosition { get; set;}
		public Position BestPosition { get; set;}
		
		public LocationListener()
		{
			_observers = new List<IObserver<Position>>();
		}
		
		public void OnLocationChanged(Location location)
		{

			var position = new Position()
			{
				Time = DateTime.Now,
				Accuracy = location.Accuracy,
				Latitude = location.Latitude,
				Longitude = location.Longitude
			};
			
			foreach(var observer in _observers)
			{
				observer.OnNext(position);
			}

			if(!BestPosition.IsBetterThan(position))
			{
				BestPosition = position;
			}

			LastKnownPosition = position;
			
		}
		
		public void OnProviderDisabled(string provider)
		{
		}
		
		public void OnProviderEnabled(string provider)
		{
		}
		
		public void OnStatusChanged(string provider, Availability status, Bundle extras)
		{
		}
		
		
		
		public IDisposable Subscribe (IObserver<Position> observer)
		{
			_observers.Add(observer);
			return new ActionDisposable(()=>{
				_observers.Remove(observer);
			});
		}


    }
}