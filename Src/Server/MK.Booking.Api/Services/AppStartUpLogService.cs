using System;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Services
{
    public class AppStartUpLogService : BaseApiService
    {
        private readonly IAppStartUpLogDao _dao;

        public AppStartUpLogService(IAppStartUpLogDao dao)
        {
            _dao = dao;
        }

        public object Get(AppStartUpLogRequest request)
        {
            // the client sends a utc date, but when it's written in the database, it's converted to server's local time
            // when we get it back from entity framework, the "kind" is unspecified, which is converted to local
            return _dao.GetAll().Where(x => x.DateOccured >= DateTime.Now.AddMinutes(-request.LastMinutes));
        }
    }
}