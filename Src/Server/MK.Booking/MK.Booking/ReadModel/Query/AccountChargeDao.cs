using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class AccountChargeDao : IAccountChargeDao
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public AccountChargeDao(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IList<AccountChargeDetail> GetAll()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<AccountChargeDetail>()
                    .Include(x => x.Questions)
                    .ToList();
            }
        }

        public AccountChargeDetail FindByAccountNumber(string number)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<AccountChargeDetail>()
                    .Include(x => x.Questions)
                    .FirstOrDefault(x => x.Number == number);
            }
        }

        public IList<AccountChargeQuestionAnswer> GetLastAnswersForAccountId(Guid accountId)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<AccountChargeQuestionAnswer>()
                    .Where(x => x.AccountId == accountId)
                    .ToList();
            }
        }
    }
}