using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using AutoMapper;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Provider;

namespace apcurium.MK.Booking.Api.Providers
{
    public class RateProvider : IRateProvider
    {
        private readonly IConfigurationManager _configManager;
        private readonly IRateDao _rateDao;

        public RateProvider(IConfigurationManager configManager, IRateDao rateDao)
        {
            _configManager = configManager;
            _rateDao = rateDao;
        }


        public IEnumerable<Rate> GetRates()
        {
            return _rateDao.GetAll().Select(Mapper.Map<Rate>);
        }
    }
}
