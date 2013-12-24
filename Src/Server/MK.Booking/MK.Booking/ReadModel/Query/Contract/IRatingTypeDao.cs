#region

using System;
using System.Collections.Generic;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IRatingTypeDao
    {
        IList<RatingTypeDetail> GetAll();
        RatingTypeDetail GetById(Guid id);
    }
}