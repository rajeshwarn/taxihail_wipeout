using apcurium.MK.Common.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Maps
{
    public interface IPlaces
    {
        Address GetPlaceDetail(string referenceId);
        Address[] SearchPlaces(string name, double? latitude, double? longitude, int? radius);
    }
}
