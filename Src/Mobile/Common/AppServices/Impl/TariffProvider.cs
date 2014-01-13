using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Provider;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class TariffProvider: BaseService, ITariffProvider
    {
        private readonly ICacheService _cacheService;

        const string CacheKey = "Company.Tariffs";

        public TariffProvider(ICacheService cacheService)
        {
            _cacheService = cacheService;
            _cacheService.Clear(CacheKey);
        }

        public IEnumerable<Tariff> GetTariffs()
        {

            var cached = _cacheService.Get<Tariff[]>(CacheKey);

            if (cached != null)
            {
                return cached;
            }
            var result = UseServiceClientAsync<TariffsServiceClient, IEnumerable<Tariff>>(service => service.GetTariffs());
            var enumerable = result as Tariff[] ?? result.ToArray();
            _cacheService.Set(CacheKey, enumerable.ToArray());
            return enumerable;
        }
    }
}