using System;
using System.Reactive;
using System.Reactive.Linq;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
    public abstract class AbstractLocationService
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
            return Positions.Where(p =>
            {
                return p.Error <= maxAccuracy;
            }).Take(timeout).Take(1);
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
        public static TimeSpan ValidCoordinateTime = new TimeSpan(0, 0, 10);

        public static bool IsBetterThan(this Position thisPosition, Position thatPosition)
        {
            if (thatPosition == null)
            {
                return true;
            }

            if (thisPosition == null)
            {
                return false;
            }

            if((thatPosition.Time - thisPosition.Time).Duration() > ValidCoordinateTime)            
            {
                return true;
            }

            return thisPosition.Error <= thatPosition.Error;
        }
    }
}