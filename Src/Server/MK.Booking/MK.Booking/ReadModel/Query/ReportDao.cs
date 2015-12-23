using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class ReportDao : IReportDao
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public ReportDao(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IEnumerable<OrderReportDetail> GetOrderReports(DateTime startDate, DateTime endDate)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<OrderReportDetail>()
                    .OrderBy(x => x.Order.CreateDateTime)
                    .Where(x => x.Order.CreateDateTime >= startDate
                             && x.Order.CreateDateTime <= endDate).ToList();
            }
        }

        public IEnumerable<OrderReportDetail> GetOrderReportsByAccountId(Guid accountId)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<OrderReportDetail>()
                    .OrderBy(x => x.Order.CreateDateTime)
                    .Where(x => x.Account.AccountId == accountId).ToList();
            }
        }

        public OrderReportDetail GetOrderReportWithOrderId(Guid orderId)
        {
            using (var context = _contextFactory.Invoke())
            {
                var orders = context.Query<OrderReportDetail>().Where(x => x.Id == orderId);

                if (orders.Any())
                {
                    return orders.First();
                }

                return null;
            }
        }
    }
}