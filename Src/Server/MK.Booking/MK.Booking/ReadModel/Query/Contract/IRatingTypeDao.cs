#region

using System;
using System.Collections.Generic;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IRatingTypeDao
    {
        IList<RatingTypeDetail[]> GetAll();
        IList<RatingTypeDetail> GetById(Guid id);
        RatingTypeDetail FindByName(string name, string language);
    }
}