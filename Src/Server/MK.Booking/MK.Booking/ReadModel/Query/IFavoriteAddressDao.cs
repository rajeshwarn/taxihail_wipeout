using System;
using System.Collections.Generic;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public interface IFavoriteAddressDao
    {
        IList<FavoriteAddress> GetAll();
        FavoriteAddress FindById(Guid id);
        IList<FavoriteAddress> FindByAccountId(Guid addressId);
    }
}