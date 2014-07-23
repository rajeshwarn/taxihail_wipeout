using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class VehicleTypeDao : IVehicleTypeDao
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public VehicleTypeDao(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IList<VehicleTypeDetail> GetAll()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<VehicleTypeDetail>().OrderBy(x => x.CreatedDate).ToList();
            }
        }

        public VehicleTypeDetail FindById(Guid id)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<VehicleTypeDetail>().FirstOrDefault(x => x.Id == id);
            }
        }
    }
}