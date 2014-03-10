using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface IGeolocService
    {
        Address ValidateAddress(string address);

        Address[] SearchAddress(double latitude, double longitude, bool searchPopularAddresses = false);

		Address[] SearchAddress(string address, double? latitude = null, double? longitude = null);

		Task<DirectionInfo> GetDirectionInfo(double originLat, double originLong, double destLat, double destLong, DateTime? date= null);

		Task<DirectionInfo> GetDirectionInfo(Address origin, Address dest);
    }
}