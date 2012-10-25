using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinyIoC;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Provider;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class PopularAddressProvider : BaseService, IPopularAddressProvider
    {
        private readonly ICacheService _cacheService;

        private const string _popularAddressesCacheKey = "Company.PopularAddresses";

        public PopularAddressProvider(ICacheService cacheService)
        {
            _cacheService = cacheService;
			_cacheService.Clear(_popularAddressesCacheKey);
        }

        public IEnumerable<Address> GetPopularAddresses()
        {

            var cached = _cacheService.Get<Address[]>(_popularAddressesCacheKey);

            if (cached != null)
            {
                return cached;
            }
            else
            {

                IEnumerable<Address> result = new Address[0];
                UseServiceClient<PopularAddressesServiceClient>(service =>
                                                                    {
                                                                        result = service.GetPopularAddresses();
                                                                    }
                    );
                _cacheService.Set(_popularAddressesCacheKey, result.ToArray());
                return result;
            }
        }
    }
}