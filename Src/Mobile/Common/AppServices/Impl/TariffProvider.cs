using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Api.Client;
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
            else
            {

                IEnumerable<Tariff> result = new Tariff[0];
                UseServiceClient<TariffsServiceClient>(service =>
                {
                    result = service.GetTariffs();
                }
                    );
                _cacheService.Set(CacheKey, result.ToArray());
                return result;
            }
        }
    }
}