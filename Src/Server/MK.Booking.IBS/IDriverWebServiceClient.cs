using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.IBS
{
    public interface IDriverWebServiceClient
    {
        IBSDriverInfos GetDriverInfos(string driverId);
    }
}
