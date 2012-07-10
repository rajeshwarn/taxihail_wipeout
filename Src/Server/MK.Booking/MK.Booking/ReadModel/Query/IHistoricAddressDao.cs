using System;
using System.Collections.Generic;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public interface IHistoricAddressDao
    {
        IList<HistoricAddress> GetAll();
        HistoricAddress FindById(Guid id);
        IList<HistoricAddress> FindByAccountId(Guid addressId);
    }
}
