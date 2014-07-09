using System;
using System.Collections.Generic;

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IAppStartUpLogDao
    {
        IList<AppStartUpLogDetail> GetAll();
    }
}
