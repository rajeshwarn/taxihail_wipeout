using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
    public interface ILocationService
    {
        void Initialize();

        Task<Position> GetPositionAsync(int timeout, float accuracy, CancellationToken cancelToken);
    }

    public class Position
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }

    }
}