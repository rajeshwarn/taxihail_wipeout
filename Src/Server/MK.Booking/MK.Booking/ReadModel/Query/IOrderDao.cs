using System;
using System.Collections.Generic;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public interface IOrderDao
    {
        IList<OrderDetail> GetAll();
        OrderDetail FindById(Guid id);
        IList<OrderDetail> FindByAccountId(Guid id);
        IList<OrderDetailWithAccount> GetAllWithAccountSummary();
    }
}