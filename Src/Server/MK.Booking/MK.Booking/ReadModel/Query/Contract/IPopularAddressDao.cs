using System;
using System.Collections.Generic;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public interface IPopularAddressDao
    {
        IList<PopularAddressDetails> GetAll();
        PopularAddressDetails FindById(Guid id);
    }
}