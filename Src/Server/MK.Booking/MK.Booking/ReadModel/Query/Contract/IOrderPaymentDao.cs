using System;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public interface IOrderPaymentDao
    {
        OrderPaymentDetail FindByTransactionId(string transactionId);
        OrderPaymentDetail FindByOrderId(Guid orderId);


        OrderPaymentDetail FindByPayPalToken(string token);
    }
}