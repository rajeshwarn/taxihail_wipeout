using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class BlackListEntryService : IBlackListEntryService
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public BlackListEntryService(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IList<BlackListEntry> GetAll()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<BlackListEntry>().ToList();
            }
        }

        public void Add(BlackListEntry entry)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.Save(entry);
            }
        }

        public void Delete(Guid id)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.RemoveWhere<BlackListEntry>(c => c.Id.Equals(id));
                context.SaveChanges();
            }
        }

        public void DeleteAll()
        {
            using (var context = _contextFactory.Invoke())
            {
                context.RemoveAll<BlackListEntry>();
                context.SaveChanges();
            }
        }

        public BlackListEntry FindByPhoneNumber(string phoneNumber)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<BlackListEntry>().FirstOrDefault(c => c.PhoneNumber.Equals(phoneNumber));
            }
        }
    }
}
