using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.Common.Web;
using apcurium.MK.Booking.ReadModel.Query;

namespace apcurium.MK.Booking.Api.Services
{
    public class FavoriteAddressesService : RestServiceBase<FavoriteAddresses> 
    {

        public FavoriteAddressesService(IAddressDao dao)
        {
            Dao = dao;
        }

        protected IAddressDao Dao { get; set; }

        public override object OnGet(FavoriteAddresses request)
        {
            if (!request.AccountId.Equals(new Guid(this.GetSession().UserAuthId)))
            {
                throw HttpError.Unauthorized("Unauthorized");
            }

            var session = this.GetSession();
            return Dao.FindByAccountId(new Guid(session.UserAuthId));
            //return new AddressList { Addresses = new Address[0] };
        }
    }
}
