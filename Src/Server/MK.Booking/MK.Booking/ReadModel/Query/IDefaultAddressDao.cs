using System;
using System.Collections.Generic;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public interface IDefaultAddressDao
    {
        IList<DefaultAddressDetails> GetAll();
        DefaultAddressDetails FindById(Guid id);
    }
}