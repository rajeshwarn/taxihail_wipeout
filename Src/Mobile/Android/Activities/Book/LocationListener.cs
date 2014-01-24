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
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
	public class LocationListener : Java.Lang.Object, ILocationListener, IObservable<Position>
	{
		List<IObserver<Position>> _observers;

		public Position LastKnownPosition { get; set; }

		public Position BestPosition { get; set; }

		public LocationListener()
		{
			_observers = new List<IObserver<Position>>();
		}

		public void OnLocationChanged(Location location)
		{
			
			var position = new Position()
			{
				Time = location.Time.ToDateTime(),
				Error = location.Accuracy,
				Latitude = location.Latitude,
				Longitude = location.Longitude
			};

			try
			{

				foreach (var observer in _observers.ToList())
				{
					observer.OnNext(position);
				}
			
				if (!BestPosition.IsBetterThan(position))
				{
					BestPosition = position;
				}
			
				LastKnownPosition = position;
			}
			catch 
			{
				//hack : crash randomly
			}
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

		public IDisposable Subscribe(IObserver<Position> observer)
		{
			_observers.Add(observer);
			return new ActionDisposable(() =>
			{
				_observers.Remove(observer);
			});
		}
	}
}