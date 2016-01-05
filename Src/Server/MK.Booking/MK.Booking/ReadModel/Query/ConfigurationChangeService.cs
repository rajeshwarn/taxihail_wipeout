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
    public class ConfigurationChangeService : IConfigurationChangeService
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public ConfigurationChangeService(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IList<ConfigurationChangeEntry> GetAll()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<ConfigurationChangeEntry>().ToList();
            }
        }

        public void Add(ConfigurationChangeEntry entry)
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
                context.RemoveWhere<ConfigurationChangeEntry>(c => c.Id.Equals(id));
                context.SaveChanges();
            }
        }

        public void DeleteAll()
        {
            using (var context = _contextFactory.Invoke())
            {
                context.RemoveAll<ConfigurationChangeEntry>();
                context.SaveChanges();
            }
        }
    }
}
