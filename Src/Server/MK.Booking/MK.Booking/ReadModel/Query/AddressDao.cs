using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Database;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class AddressDao : IAddressDao
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public AddressDao(Func<BookingDbContext> contextFactory)
        {            
            _contextFactory = contextFactory;
        }

        public IList<FavoriteAddress> GetAll()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<FavoriteAddress>().ToList();
            }
        }

        public FavoriteAddress FindById(Guid id)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<FavoriteAddress>().SingleOrDefault(c => c.Id == id);
            }
        }

        public IList<FavoriteAddress> FindByAccountId(Guid addressId)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<FavoriteAddress>().Where(c => c.AccountId.Equals(addressId)).ToList();
            }
        }
    }
}
