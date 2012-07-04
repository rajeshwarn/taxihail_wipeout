using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using ServiceStack.Common.Web;

namespace apcurium.MK.Booking.Api.Services
{
    public class FavoriteAddressesService : RestServiceBase<FavoriteAddresses> 
    {
        public override object OnGet(FavoriteAddresses request)
        {
            if (!request.AccountId.Equals(new Guid(this.GetSession().UserAuthId)))
            {
                throw HttpError.Unauthorized("Unauthorized");
            }

            return new AddressList { Addresses = new Address[0] };
        }
    }
}
