using System;
using System.Collections.Generic;

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
   public interface IReportDao
   {
      IEnumerable<OrderReportDetail> GetOrderReports(DateTime startDate, DateTime endDate);

      IEnumerable<OrderReportDetail> GetOrderReportsByAccountId(Guid accountId, DateTime startDate, DateTime endDate);

      OrderReportDetail GetOrderReportWithOrderId(Guid orderId);
   }
}