#region

using System;
using System.Collections.Generic;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IAccountDao
    {
        IList<AccountDetail> GetAll();
        AccountDetail FindById(Guid id);
        AccountDetail FindByEmail(string email);
        AccountDetail FindByFacebookId(string id);
        AccountDetail FindByTwitterId(string id);
    }
}