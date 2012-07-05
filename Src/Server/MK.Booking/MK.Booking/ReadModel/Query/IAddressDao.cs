using System;
using System.Collections.Generic;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public interface IAddressDao
    {
        IList<FavoriteAddress> GetAll();
        FavoriteAddress FindById(Guid id);
        IList<FavoriteAddress> FindByAccountId(Guid addressId);
    }
}