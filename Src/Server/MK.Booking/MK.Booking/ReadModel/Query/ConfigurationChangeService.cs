using System;
using System.Collections.Generic;
using System.Linq;
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

        public void Add(Dictionary<string, string> oldValues, Dictionary<string, string> newValues, ConfigurationChangeType type, string accountId, string email)
        {
            if (oldValues.Any() && newValues.Any())
            {
                using (var context = _contextFactory.Invoke())
                {
                    context.Save(new ConfigurationChangeEntry
                    {
                        AccountId = accountId,
                        AccountEmail = email,
                        OldValues = oldValues.ToJson(),
                        NewValues = newValues.ToJson(),
                        Date = DateTime.UtcNow,
                        Type = type.ToString()
                    });
                }
            }
        }
    }
}
