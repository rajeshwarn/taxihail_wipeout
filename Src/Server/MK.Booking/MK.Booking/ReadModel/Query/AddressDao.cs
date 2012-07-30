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

        public IList<Address> GetAll()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<Address>().ToList();
            }
        }

        public Address FindById(Guid id)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<Address>().SingleOrDefault(c => c.Id == id);
            }
        }

        public IList<Address> FindFavoritesByAccountId(Guid addressId)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<Address>().Where(c => c.AccountId.Equals(addressId) && c.IsHistoric.Equals(false)).ToList();
            }
        }

        public IList<Address> FindHistoricByAccountId(Guid addressId)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<Address>().Where(c => c.AccountId.Equals(addressId) && c.IsHistoric.Equals(true)).ToList();
            }
        }
    }
}
