#region

using System;
using System.Collections.Generic;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IPopularAddressDao
    {
        IList<PopularAddressDetails> GetAll();
        PopularAddressDetails FindById(Guid id);
    }
}