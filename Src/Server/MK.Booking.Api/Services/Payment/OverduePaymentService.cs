using System;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Resources;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class OverduePaymentService : Service
    {
        private readonly IOverduePaymentDao _overduePaymentDao;
        private readonly IPaymentService _paymentService;

        public OverduePaymentService(IOverduePaymentDao overduePaymentDao, IPaymentService paymentService)
        {
            _overduePaymentDao = overduePaymentDao;
            _paymentService = paymentService;
        }

        public object Get(OverduePaymentRequest request)
        {
            var session = this.GetSession();
            var accountId = new Guid(session.UserAuthId);

            var overduePaymentHistory = _overduePaymentDao.FindByAccountId(accountId);

            // Should only be one overdue payment by account at any time
            return overduePaymentHistory.FirstOrDefault(p => !p.IsPaid);
        }

        public object Post(SettleOverduePaymentRequest request)
        {
            var session = this.GetSession();
            var accountId = new Guid(session.UserAuthId);

            var overduePaymentHistory = _overduePaymentDao.FindByAccountId(accountId);
            var overduePayment = overduePaymentHistory.FirstOrDefault(p => !p.IsPaid);

            if (overduePayment == null)
            {
                return new SettleOverduePaymentResponse
                {
                    IsSuccessful = true,
                    Message = "No overdue payment to settle"
                };
            }

            return _paymentService.SettleOverduePayment(overduePayment.OrderId, overduePayment.TransactionId, overduePayment.OverdueAmount);
        }
    }
}
