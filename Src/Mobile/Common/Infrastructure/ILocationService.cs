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
                
        IObservable<Position> GetNextPosition(TimeSpan timeout, float maxAccuracy);

        Position LastKnownPosition {get;}

    }

    public class Position
    {
        public float Accuracy  { get; set; }

        public DateTime Time  { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

    }
}