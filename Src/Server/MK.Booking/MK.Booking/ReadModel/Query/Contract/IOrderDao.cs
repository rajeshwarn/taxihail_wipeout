using System;
using System.Collections.Generic;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public interface IOrderDao
    {
        IList<OrderDetail> GetAll();
        OrderDetail FindById(Guid id);
        IList<OrderDetail> FindByAccountId(Guid id);
        IList<OrderDetailWithAccount> GetAllWithAccountSummary();
        IList<OrderStatusDetail> GetOrdersInProgress();
        IList<OrderStatusDetail> GetOrdersInProgressByAccountId(Guid accountId);
        OrderStatusDetail FindOrderStatusById(Guid orderId);
        OrderPairingDetail FindOrderPairingById(Guid orderId);
    }
}