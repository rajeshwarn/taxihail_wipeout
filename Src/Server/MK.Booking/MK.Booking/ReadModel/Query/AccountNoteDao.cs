using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using ServiceStack.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class AccountNoteDao : IAccountNoteDao
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public AccountNoteDao(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IList<AccountNoteDetail> GetAll()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<AccountNoteDetail>().ToList();
            }
        }

        public AccountNoteDetail FindById(Guid id)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<AccountNoteDetail>().SingleOrDefault(c => c.Id == id);
            }
        }

        public IList<AccountNoteDetail> FindByAccountId(Guid accountId)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<AccountNoteDetail>().Select(c => c.AccountId == accountId).ToList<AccountNoteDetail>();
            }
        }
    }
}
