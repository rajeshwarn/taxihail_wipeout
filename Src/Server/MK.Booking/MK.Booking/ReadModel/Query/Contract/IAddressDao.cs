using System;
using System.Collections.Generic;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public interface IAddressDao
    {
        IList<AddressDetails> GetAll();
        AddressDetails FindById(Guid id);
        IList<AddressDetails> FindFavoritesByAccountId(Guid addressId);
        IList<AddressDetails> FindHistoricByAccountId(Guid addressId);
    }
}