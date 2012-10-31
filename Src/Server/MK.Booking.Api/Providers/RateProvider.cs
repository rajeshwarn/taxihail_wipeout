using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using AutoMapper;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Provider;

namespace apcurium.MK.Booking.Api.Providers
{
    public class RateProvider : IRateProvider
    {
        private readonly IRateDao _rateDao;

        public RateProvider(IRateDao rateDao)
        {
            _rateDao = rateDao;
        }


        public IEnumerable<Rate> GetRates()
        {
            return _rateDao.GetAll().Select(Mapper.Map<Rate>);
        }
    }
}
