using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Provider;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class RateProvider: BaseService, IRateProvider
    {
        private readonly ICacheService _cacheService;

        const string CacheKey = "Company.Rates";

        public RateProvider(ICacheService cacheService)
        {
            _cacheService = cacheService;
            _cacheService.Clear(CacheKey);
        }

        public IEnumerable<Rate> GetRates()
        {

            var cached = _cacheService.Get<Rate[]>(CacheKey);

            if (cached != null)
            {
                return cached;
            }
            else
            {

                IEnumerable<Rate> result = new Rate[0];
                UseServiceClient<RatesServiceClient>(service =>
                {
                    result = service.GetRates();
                }
                    );
                _cacheService.Set(CacheKey, result.ToArray());
                return result;
            }
        }
    }
}