using System;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class CompanyDao : ICompanyDao
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public CompanyDao(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public CompanyDetail Get()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<CompanyDetail>().First();
            }
        }
    }
}