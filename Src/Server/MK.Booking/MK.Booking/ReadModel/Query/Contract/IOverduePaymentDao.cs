using System;

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IOverduePaymentDao
    {
        OverduePaymentDetail FindNotPaidByAccountId(Guid accountId);
    }
}