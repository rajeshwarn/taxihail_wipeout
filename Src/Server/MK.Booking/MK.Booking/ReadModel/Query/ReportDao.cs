using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class ReportDao : IReportDao
    {
        private readonly Func<BookingDbContext> _contextFactory;
        private readonly IClock _clock;
        private readonly IServerSettings _serverSettings;

        public ReportDao(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IEnumerable<OrderReportDetail> GetAll()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<OrderReportDetail>().OrderBy(x => x.Order.CreateDateTime).OrderBy(x => x.Order.CreateDateTime).ToList();
            }
        }

        public IEnumerable<OrderReportDetail> GetAll(Guid accountId)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<OrderReportDetail>().OrderBy(x => x.Order.CreateDateTime).Where(x => x.Account.AccountId == accountId).ToList();
            }
        }

        public IEnumerable<OrderReportDetail> GetAll(DateTime startDate, DateTime endDate)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<OrderReportDetail>().OrderBy(x => x.Order.CreateDateTime).Where(x => x.Order.CreateDateTime > startDate && x.Order.CreateDateTime <= endDate).ToList();
            }
        }
    }
}