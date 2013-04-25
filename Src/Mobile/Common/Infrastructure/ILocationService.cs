using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
    public interface ILocationService
    {
        void Start();
        void Stop ();

        bool IsLocationServicesEnabled{get;}

        IObservable<Position> Positions { get; }
                
        IObservable<Position> GetNextBest(TimeSpan timeout);

        IObservable<Position> GetNextPosition(TimeSpan timeout, float maxAccuracy);

        Position LastKnownPosition {get;}
        Position BestPosition {get;}

    }

    public class Position
    {
        public float Accuracy  { get; set; }

        public DateTime Time  { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

    }


    public static class PositionExtensions{

        public static TimeSpan ValidCoordinateTime = new TimeSpan(0,0,30);

        public static bool IsBetterThan (this Position thisPosition,Position thatPosition)
        {
            if(thatPosition == null)
            {
                return true;
            }

            if(     thisPosition ==null 
               ||   thatPosition.Time - thisPosition.Time > ValidCoordinateTime)
            {
                return false;
            }

            return thisPosition.Accuracy > thatPosition.Accuracy;
        }
    }
}