#region

using System;
using System.Collections.Generic;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IAirlineDao
    {
        IList<Airline> GetAll();
        Airline FindById(String id);
        IList<Airline> FindByName(string name);
    }
}