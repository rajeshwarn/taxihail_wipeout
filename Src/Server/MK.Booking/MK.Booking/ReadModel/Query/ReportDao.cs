using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class ReportDao : IReportDao
    {
        private readonly Func<BookingDbContext> _contextFactory;
        private readonly IClock _clock;
        private readonly IServerSettings _serverSettings;

        public ReportDao(Func<BookingDbContext> contextFactory, IClock clock, IServerSettings serverSettings)
        {
            _contextFactory = contextFactory;
            _clock = clock;
            _serverSettings = serverSettings;
        }

        public IEnumerable<OrderReportDetail> GetAll()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<OrderReportDetail>().OrderBy(x => x.CreateDateTime).ToArray();
            }
        }

        public IEnumerable<OrderReportDetail> GetAll(Guid accountId)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<OrderReportDetail>().OrderBy(x => x.CreateDateTime).Where(x => x.AccountId == accountId);
            }
        }

        public IEnumerable<OrderReportDetail> GetAll(DateTime startDate, DateTime endDate)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<OrderReportDetail>().OrderBy(x => x.CreateDateTime).Where(x => x.CreateDateTime > startDate && x.CreateDateTime <= endDate).ToArray();
            }
        }
    }
}