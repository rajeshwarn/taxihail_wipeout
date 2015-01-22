using System;
using System.Collections.Generic;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IReportDao
    {
        IEnumerable<OrderReportDetail> GetAll();

        IEnumerable<OrderReportDetail> GetAll(Guid accountId);

        IEnumerable<OrderReportDetail> GetAll(DateTime startDate, DateTime endDate);
    }
}