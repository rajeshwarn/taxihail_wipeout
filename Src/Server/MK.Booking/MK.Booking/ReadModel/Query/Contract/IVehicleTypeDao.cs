using System;
using System.Collections.Generic;

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IVehicleTypeDao
    {
        IList<VehicleTypeDetail> GetAll();
        VehicleTypeDetail FindById(Guid id);
    }
}