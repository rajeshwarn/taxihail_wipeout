using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;

namespace apcurium.MK.Booking.Api.Services
{
    public class SaveFavoriteAddressService : RestServiceBase<SaveFavoriteAddress> 
    {
        public override object OnPut(SaveFavoriteAddress request)
        {
            return base.OnPut(request);
        }

        public override object OnPost(SaveFavoriteAddress request)
        {
            return base.OnPost(request);
        }
    }
}
