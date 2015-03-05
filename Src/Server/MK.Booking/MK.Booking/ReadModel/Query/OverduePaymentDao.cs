using System;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class OverduePaymentDao : IOverduePaymentDao
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public OverduePaymentDao(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public OverduePaymentDetail FindNotPaidByAccountId(Guid accountId)
        {
            using (var context = _contextFactory.Invoke())
            {
                // Should only be one overdue payment by account at any time
                return context.Query<OverduePaymentDetail>().FirstOrDefault(x => x.AccountId == accountId && !x.IsPaid);
            }
        }
    }
}
