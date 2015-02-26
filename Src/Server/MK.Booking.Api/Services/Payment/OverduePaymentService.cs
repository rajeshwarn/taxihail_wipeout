using System;
using System.Linq;
using System.Threading;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Resources;
using Infrastructure.Messaging;
using ServiceStack.ServiceInterface;

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class OverduePaymentService : Service
    {
        private readonly ICommandBus _commandBus;
        private readonly IOverduePaymentDao _overduePaymentDao;
        private readonly IAccountDao _accountDao;
        private readonly IPaymentService _paymentService;

        public OverduePaymentService(ICommandBus commandBus, IOverduePaymentDao overduePaymentDao, IAccountDao accountDao, IPaymentService paymentService)
        {
            _commandBus = commandBus;
            _overduePaymentDao = overduePaymentDao;
            _accountDao = accountDao;
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

            var accountDetail = _accountDao.FindById(accountId);
            var overduePaymentOrderId = Guid.NewGuid();

            var preAuthResponse = _paymentService.PreAuthorize(overduePaymentOrderId, accountDetail, overduePayment.OverdueAmount);
            if (preAuthResponse.IsSuccessful)
            {
                // Wait for payment to be created
                Thread.Sleep(500);

                var commitResponse = _paymentService.CommitPayment(
                    overduePaymentOrderId,
                    accountDetail,
                    overduePayment.OverdueAmount,
                    overduePayment.OverdueAmount,
                    overduePayment.OverdueAmount,
                    0,
                    preAuthResponse.TransactionId);

                if (commitResponse.IsSuccessful)
                {
                    _commandBus.Send(new SettleOverduePayment
                    {
                        OrderId = overduePayment.OrderId
                    });

                    return new SettleOverduePaymentResponse
                    {
                        IsSuccessful = true
                    };
                }

                // Payment failed, void preauth
                _paymentService.VoidPreAuthorization(overduePaymentOrderId);
            }

            return new SettleOverduePaymentResponse
            {
                IsSuccessful = false
            };
        }
    }
}