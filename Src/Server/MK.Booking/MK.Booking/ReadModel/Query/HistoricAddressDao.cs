using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Database;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class HistoricAddressDao : IHistoricAddressDao
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public HistoricAddressDao(Func<BookingDbContext> contextFactory)
        {            
            _contextFactory = contextFactory;
        }

        public IList<HistoricAddress> GetAll()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<HistoricAddress>().ToList();
            }
        }

        public HistoricAddress FindById(Guid id)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<HistoricAddress>().SingleOrDefault(c => c.Id == id);
            }
        }

        public IList<HistoricAddress> FindByAccountId(Guid accountId)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<HistoricAddress>().Where(c => c.AccountId.Equals(accountId)).ToList();
            }
        }
    }
}
