using System;
using System.Collections.Generic;

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IOverduePaymentDao
    {
        IList<OverduePaymentDetail> GetAll();

        OverduePaymentDetail FindById(Guid id);

        IList<OverduePaymentDetail> FindByAccountId(Guid id);
    }
}