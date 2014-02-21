using System;
using System.Reactive.Linq;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public interface ILocationService 
    {
         void Start();

         void Stop();

         bool IsLocationServicesEnabled{ get; }

         bool IsStarted { get; }

		IObservable<Position> Positions { get;}

		IObservable<Position> GetNextBest(TimeSpan timeout);
        
		IObservable<Position> GetNextPosition(TimeSpan timeout, float maxAccuracy);
        
        Position LastKnownPosition { get; }

        Position BestPosition { get; }
    }

}