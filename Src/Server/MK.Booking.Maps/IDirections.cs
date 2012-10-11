using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Maps
{
    public interface IDirections
    {
        Direction GetDirection(double? originLat, double? originLng, double? destinationLat, double? destinationLng);
    }
}
