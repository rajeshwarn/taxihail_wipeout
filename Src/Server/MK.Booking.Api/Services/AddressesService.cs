using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.Common.Web;
using apcurium.MK.Booking.ReadModel.Query;

namespace apcurium.MK.Booking.Api.Services
{
    public class AddressesService : RestServiceBase<Addresses> 
    {
        public AddressesService(IAddressDao dao)
        {
            Dao = dao;
        }

        protected IAddressDao Dao { get; set; }

        public override object OnGet(Addresses request)
        {
            var session = this.GetSession();
            return Dao.FindFavoritesByAccountId(new Guid(session.UserAuthId));
        }
    }
}
