using System;
using System.Collections.Generic;
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

        public IList<OverduePaymentDetail> GetAll()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<OverduePaymentDetail>().ToList();
            }
        }

        public OverduePaymentDetail FindById(Guid id)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<OverduePaymentDetail>().SingleOrDefault(c => c.OrderId == id);
            }
        }

        public IList<OverduePaymentDetail> FindByAccountId(Guid id)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<OverduePaymentDetail>().Where(c => c.AccountId == id).ToList();
            }
        }
    }
}
