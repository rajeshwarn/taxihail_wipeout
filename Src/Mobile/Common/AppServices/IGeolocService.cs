using System.Collections.Generic;
using apcurium.MK.Common.Entity;
 using apcurium.MK.Booking.Api.Contract.Resources;
using System;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface IGeolocService
    {
        Address ValidateAddress(string address);

        [Obsolete("Use async method instead")]
        Address[] SearchAddress(double latitude, double longitude, bool searchPopularAddresses = false);
        Task<Address[]> SearchAddressAsync(double latitude, double longitude, bool searchPopularAddresses = false);

		Address[] SearchAddress(string address, double? latitude = null, double? longitude = null);

        DirectionInfo GetDirectionInfo(double originLat, double originLong, double destLat, double destLong, DateTime? date= null);

        DirectionInfo GetDirectionInfo(Address origin, Address dest);

        IEnumerable<Address> FindSimilar(string address);
    }
}