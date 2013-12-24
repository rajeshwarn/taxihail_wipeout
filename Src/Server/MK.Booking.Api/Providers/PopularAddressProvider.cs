using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using AutoMapper;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Provider;

namespace apcurium.MK.Booking.Api.Providers
{
    public class PopularAddressProvider : IPopularAddressProvider
    {
        private readonly IPopularAddressDao _dao;

        public PopularAddressProvider(IPopularAddressDao dao)
        {
            _dao = dao;
        }

        public IEnumerable<Address> GetPopularAddresses()
        {
            return _dao.GetAll().Select(Mapper.Map<Address>);
        }
    }
}
