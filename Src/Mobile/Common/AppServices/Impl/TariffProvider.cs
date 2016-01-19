using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Resources;
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
            //Case 1: If tariff is overridden by market settings if overriden in roaming market:
            var marketSettings = new MarketSettings();

            if (marketSettings.HashedMarket != null && marketSettings.OverrideEnableAppFareEstimates)
            {
                return new[]
                {
                    new Tariff()
                    {
                        Type = (int) TariffType.Default,
                        FlatRate = marketSettings.FlatRate,
                        KilometerIncluded = marketSettings.KilometerIncluded,
                        KilometricRate = marketSettings.KilometricRate,
                        MarginOfError = marketSettings.MarginOfError,
                        MinimumRate = marketSettings.MinimumRate,
                        PerMinuteRate = marketSettings.PerMinuteRate,
                    },
                };
            }

            //Case 2: Using tariff as set in local company.
            var cached = _cacheService.Get<Tariff[]>(CacheKey);
			if (cached != null)
            {
                return cached;
            }

			var result = UseServiceClientAsync<TariffsServiceClient, IEnumerable<Tariff>>(service => service.GetTariffs()).Result;
            var enumerable = result as Tariff[] ?? result.ToArray();
            _cacheService.Set(CacheKey, enumerable.ToArray());
            return enumerable;
        }
    }
}