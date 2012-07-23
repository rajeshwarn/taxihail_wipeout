using System;
using System.Collections.Generic;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public interface IAddressDao
    {
        IList<Address> GetAll();
        Address FindById(Guid id);
        IList<Address> FindFavoritesByAccountId(Guid addressId);
        IList<Address> FindHistoricByAccountId(Guid addressId);
    }
}