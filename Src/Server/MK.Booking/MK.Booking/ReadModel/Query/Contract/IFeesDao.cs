using System.Collections.Generic;

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IFeesDao
    {
        IList<FeesDetail> GetAll();

        FeesDetail GetMarketFees(string market);
    }
}
