using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

 using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface IGeolocService
    {
        Address ValidateAddress(string address);

        Address[] SearchAddress(double latitude, double longitude);

        DirectionInfo GetDirectionInfo(double originLat, double originLong, double destLat, double destLong);

        DirectionInfo GetDirectionInfo(Address origin, Address dest);

        IEnumerable<Address> FindSimilar(string address);
    }
}