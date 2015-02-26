using System;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class OverduePaymentService : Service
    {
        private readonly IOverduePaymentDao _overduePaymentDao;

        public OverduePaymentService(IOverduePaymentDao overduePaymentDao)
        {
            _overduePaymentDao = overduePaymentDao;
        }

        public object Get(OverduePaymentRequest request)
        {
            var session = this.GetSession();
            var accountId = new Guid(session.UserAuthId);

            var overduePaymentHistory = _overduePaymentDao.FindByAccountId(accountId);

            // Should only be one overdue payment by account at any time
            return overduePaymentHistory.FirstOrDefault(p => !p.IsPaid);
        }
    }
}
