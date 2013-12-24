#region

using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Provider;
using AutoMapper;

#endregion

namespace apcurium.MK.Booking.Api.Providers
{
    public class TariffProvider : ITariffProvider
    {
        private readonly ITariffDao _rateDao;

        public TariffProvider(ITariffDao rateDao)
        {
            _rateDao = rateDao;
        }

        public IEnumerable<Tariff> GetTariffs()
        {
            return _rateDao.GetAll().Select(Mapper.Map<Tariff>);
        }
    }
}