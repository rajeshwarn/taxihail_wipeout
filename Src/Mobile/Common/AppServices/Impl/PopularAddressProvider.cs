using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Provider;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class PopularAddressProvider : BaseService, IPopularAddressProvider
    {
        private readonly ICacheService _cacheService;

        private const string PopularAddressesCacheKey = "Company.PopularAddresses";

        public PopularAddressProvider(ICacheService cacheService)
        {
            _cacheService = cacheService;
			_cacheService.Clear(PopularAddressesCacheKey);
        }

        public IEnumerable<Address> GetPopularAddresses()
        {
            var cached = _cacheService.Get<Address[]>(PopularAddressesCacheKey);
			if (cached != null)
            {
                return cached;
            }

			var result = UseServiceClientAsync<PopularAddressesServiceClient, IEnumerable<Address>>(service => service.GetPopularAddresses()).Result;
            var popularAddresses = result as Address[] ?? result.ToArray();
            _cacheService.Set(PopularAddressesCacheKey, popularAddresses.ToArray());
            return popularAddresses;
        }
    }
}