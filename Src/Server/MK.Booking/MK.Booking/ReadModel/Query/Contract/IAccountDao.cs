using System;
using System.Collections.Generic;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public interface IAccountDao
    {
        IList<AccountDetail> GetAll();
        AccountDetail FindById(Guid Id);
        AccountDetail FindByEmail(string email);
        AccountDetail FindByFacebookId(string id);
        AccountDetail FindByTwitterId(string id);
    }
}