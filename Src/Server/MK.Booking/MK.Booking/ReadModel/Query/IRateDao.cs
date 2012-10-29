using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public interface IRateDao
    {
        IList<RateDetail> GetAll();
    }
}
