using System.Collections.Generic;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public interface ITariffDao
    {
        IList<TariffDetail> GetAll();
    }
}