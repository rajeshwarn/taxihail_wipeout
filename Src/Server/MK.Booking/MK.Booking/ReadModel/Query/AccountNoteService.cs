using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using System;
using System.Collections.Generic;
using System.Linq;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class AccountNoteService : IAccountNoteService
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public AccountNoteService(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IList<AccountNoteEntry> GetAll()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<AccountNoteEntry>().ToList();
            }
        }

        public AccountNoteEntry FindById(Guid id)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<AccountNoteEntry>().SingleOrDefault(c => c.Id == id);
            }
        }

        public IList<AccountNoteEntry> FindByAccountId(Guid accountId)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<AccountNoteEntry>().Where(c => c.AccountId == accountId).ToList();
            }
        }

        public void Add(AccountNoteEntry accountNoteEntry)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.Save(accountNoteEntry);
            }
        }
    }
}
