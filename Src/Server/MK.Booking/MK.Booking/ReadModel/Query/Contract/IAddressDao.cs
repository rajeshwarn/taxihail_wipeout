#region

using System;
using System.Collections.Generic;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IAddressDao
    {
        IList<AddressDetails> GetAll();
        AddressDetails FindById(Guid id);
        IList<AddressDetails> FindFavoritesByAccountId(Guid addressId);
        IList<AddressDetails> FindHistoricByAccountId(Guid addressId);
    }
}