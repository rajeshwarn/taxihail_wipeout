using System;
using System.Threading;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
    public interface ILocationService
    {
        void Initialize();

        Task<Position> GetPositionAsync(int timeout, float accuracy, int fallbackTimeout, float fallbackAccuracy, CancellationToken cancelToken);

        Position LastKnownPosition {get;}

        bool IsGpsActive { get; }
    }

    public class Position
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public float Accuracy { get; set; }

        public double Altitude { get; set; }

        public float Bearing { get; set; }

        public float Speed { get; set; }

        public DateTime Time { get; set; }

        public string Description { get; set; }

    }
}