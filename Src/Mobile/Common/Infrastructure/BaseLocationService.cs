using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using TinyIoC;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public abstract class BaseLocationService :ILocationService
	{
		public abstract void Start();

		public abstract void Stop();

		public abstract bool IsStarted { get; }

		public IObservable<Position> Positions { get; protected set; }

		public IObservable<Position> GetNextBest(TimeSpan timeout)
		{
			return Positions.TakeLast(timeout).Select(_ => BestPosition);
		}

		private IObservable<Position> GetNextPosition(TimeSpan timeout, float maxAccuracy)
		{
			if (!IsStarted)
			{
				Start();
			}
			return Positions.Where(p => p.IsActive() && p.Error <= maxAccuracy).Take(timeout).Take(1);
		}

        public async Task<Position> GetUserPosition()
        {
			if((BestPosition != null) && BestPosition.IsActive())
            {
                return BestPosition;
            }

			// ios does not send CLLocationManagerDelegate.didUpdateLocation constantly when there is no position update so the PositionExtensions.ValidCoordinateTime
			// quickly becomes invalid which causes to pause (GetNextPosition([pause time],.....) every time when Find My Location click before acting to position on the map
			// so the pause time should be 0
			var position = await GetNextPosition(TimeSpan.Zero, 70)
                .Take(1)
                .DefaultIfEmpty() // Will return null in case of a timeout
                .ToTask();

            if (position != null)
            {
                return position;
            }

            // between the first call to BestPosition, we might have received a position if LocationService was started by GetNextPosition()
            return BestPosition;
		}

		public Position GetInitialPosition()
		{
			var settings = TinyIoCContainer.Current.Resolve<IAppSettings>().Data;
			var lastKnownPosition = TinyIoCContainer.Current.Resolve<ICacheService>().Get<Position>("UserLastKnownPosition");

			var defaultPosition = lastKnownPosition == null 
				? new Position() 
				{ 
					Latitude = settings.GeoLoc.DefaultLatitude,
					Longitude = settings.GeoLoc.DefaultLongitude
				}
				: lastKnownPosition;

			var position = BestPosition != null ? BestPosition : defaultPosition;

			return position;
		}

		public abstract Position LastKnownPosition { get; }

		public abstract Position BestPosition { get; }        
	}

	public class Position
	{
		public float Error  { get; set; }

		public DateTime Time  { get; set; }

		public double Latitude { get; set; }

		public double Longitude { get; set; }
	}

	public static class PositionExtensions
	{
		public static TimeSpan ValidCoordinateTime = new TimeSpan(0, 0, 30);

		public static bool IsActive(this Position postion)
		{
			return postion.Time > DateTime.UtcNow.Subtract (ValidCoordinateTime);
		}

		public static bool IsBetterThan(this Position trueIfBetter, Position falseIfBetter)
		{

			if (falseIfBetter == null)
			{
				return true;
			}

			if (trueIfBetter == null)
			{
				return false;
			}

			if((falseIfBetter.Time - trueIfBetter.Time).Duration() > ValidCoordinateTime)            
			{
				return false;
			}

			return trueIfBetter.Error < falseIfBetter.Error;
		}
	}
}