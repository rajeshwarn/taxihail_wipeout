using System;
using System.Reactive.Linq;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public abstract class BaseLocationService :ILocationService
	{
		public abstract void Start();

		public abstract void Stop();

		public abstract bool IsLocationServicesEnabled{ get; }

		public abstract bool IsStarted { get; }

		public IObservable<Position> Positions { get; protected set; }

		public IObservable<Position> GetNextBest(TimeSpan timeout)
		{
			return Positions.TakeLast(timeout).Select(_ => BestPosition);
		}

		public IObservable<Position> GetNextPosition(TimeSpan timeout, float maxAccuracy)
		{
			if (!IsStarted)
			{
				Start();
			}
			return Positions.Where(p => p.Error <= maxAccuracy).Take(timeout).Take(1);
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

			// ReSharper disable once LocalizableElement
			Console.WriteLine("IsBetterThan current error {0}, other error {1}", trueIfBetter.Error, falseIfBetter.Error);
			return trueIfBetter.Error < falseIfBetter.Error;
		}
	}
}