#region

using System;
using System.Collections.Generic;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IPickupPointDao
    {
        IList<PickupPoint> GetAll();
        PickupPoint FindById(String id);
        IList<PickupPoint> FindByName(string name);
    }
}