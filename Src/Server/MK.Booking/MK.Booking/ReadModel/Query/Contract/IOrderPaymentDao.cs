#region

using System;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IOrderPaymentDao
    {
        OrderPaymentDetail FindByTransactionId(string transactionId);
        OrderPaymentDetail FindByOrderId(Guid orderId);
        OrderPaymentDetail FindByPayPalToken(string token);
    }
}