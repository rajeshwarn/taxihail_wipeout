#region

using System;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class AddressesService : Service
    {
        public AddressesService(IAddressDao dao)
        {
            Dao = dao;
        }

        protected IAddressDao Dao { get; set; }

        public object Get(Addresses request)
        {
            var session = this.GetSession();
            return Dao.FindFavoritesByAccountId(new Guid(session.UserAuthId));
        }
    }
}