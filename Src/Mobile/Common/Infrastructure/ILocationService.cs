using System;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public interface ILocationService 
    {
         void Start();

         void Stop();

         bool IsStarted { get; }

		IObservable<Position> Positions { get;}

		IObservable<Position> GetNextBest(TimeSpan timeout);

        Task<Position> GetUserPosition();

		Position GetInitialPosition();
        
        Position LastKnownPosition { get; }

        Position BestPosition { get; }
    }

}