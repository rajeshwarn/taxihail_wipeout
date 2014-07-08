using System;

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface IAppStartUpLogDao
    {
        AppStartUpLogDetail FindById(Guid id);
    }
}
