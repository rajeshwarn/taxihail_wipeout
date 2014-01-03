#region

using System;
using System.Collections.Generic;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IDefaultAddressDao
    {
        IList<DefaultAddressDetails> GetAll();
        DefaultAddressDetails FindById(Guid id);
    }
}