using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using AutoMapper;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Provider;

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
