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