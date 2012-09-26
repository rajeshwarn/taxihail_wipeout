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

        public IList<AddressDetails> GetAll()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<AddressDetails>().ToList();
            }
        }

        public AddressDetails FindById(Guid id)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<AddressDetails>().SingleOrDefault(c => c.Id == id);
            }
        }

        public IList<AddressDetails> FindFavoritesByAccountId(Guid addressId)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<AddressDetails>().Where(c => c.AccountId.Equals(addressId) && c.IsHistoric.Equals(false)).ToList();
            }
        }

        public IList<AddressDetails> FindHistoricByAccountId(Guid addressId)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<AddressDetails>().Where(c => c.AccountId.Equals(addressId) && c.IsHistoric.Equals(true)).ToList();
            }
        }
    }
}
