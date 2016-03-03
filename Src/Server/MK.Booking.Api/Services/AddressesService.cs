#region

using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class AddressesService : BaseApiService
    {
        public AddressesService(IAddressDao dao)
        {
            Dao = dao;
        }

        protected IAddressDao Dao { get; set; }

        public IList<AddressDetails> Get()
        {
            return Dao.FindFavoritesByAccountId(Session.UserId);
        }
    }
}