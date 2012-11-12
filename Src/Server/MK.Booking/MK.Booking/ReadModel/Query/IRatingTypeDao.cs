using System;
using System.Collections.Generic;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public interface IRatingTypeDao
    {
        IList<RatingTypeDetail> GetAll();
        RatingTypeDetail GetById(Guid id);
    }
}