#region

using System.Collections.Generic;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface ITariffDao
    {
        IList<TariffDetail> GetAll();
    }
}