using System;
using System.Linq;
using System.Threading;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
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
        private readonly IOrderDao _orderDao;
        private readonly IOrderPaymentDao _orderPaymentDao;
        private readonly IPromotionDao _promotionDao;
        private readonly IPaymentService _paymentService;
        private readonly IServerSettings _serverSettings;

        public OverduePaymentService(
            ICommandBus commandBus,
            IOverduePaymentDao overduePaymentDao,
            IAccountDao accountDao,
            IOrderDao orderDao,
            IOrderPaymentDao orderPaymentDao,
            IPromotionDao promotionDao,
            IPaymentService paymentService,
            IServerSettings serverSettings)
        {
            _commandBus = commandBus;
            _overduePaymentDao = overduePaymentDao;
            _accountDao = accountDao;
            _orderDao = orderDao;
            _orderPaymentDao = orderPaymentDao;
            _promotionDao = promotionDao;
            _paymentService = paymentService;
            _serverSettings = serverSettings;
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

            var payment = _orderPaymentDao.FindByOrderId(overduePayment.OrderId);
            var reAuth = payment != null;

            var preAuthResponse = _paymentService.PreAuthorize(overduePayment.OrderId, accountDetail, overduePayment.OverdueAmount, reAuth);
            if (preAuthResponse.IsSuccessful)
            {
                // Wait for payment to be created
                Thread.Sleep(500);

                var commitResponse = _paymentService.CommitPayment(
                    overduePayment.OrderId,
                    accountDetail,
                    overduePayment.OverdueAmount,
                    overduePayment.OverdueAmount,
                    overduePayment.OverdueAmount,
                    0,
                    preAuthResponse.TransactionId,
                    preAuthResponse.ReAuthOrderId);

                if (commitResponse.IsSuccessful)
                {
                    // Go fetch declined order, and send its receipt
                    var paymentDetail = _orderPaymentDao.FindByOrderId(overduePayment.OrderId);
                    var promotion = _promotionDao.FindByOrderId(overduePayment.OrderId);

                    var tipAmount = GetTipFromTotalAmount(overduePayment.OrderId, overduePayment.OverdueAmount);
                    var meterAmount = overduePayment.OverdueAmount - tipAmount;

                    var fareObject = Fare.FromAmountInclTax(
                        Convert.ToDouble(meterAmount), _serverSettings.ServerData.VATIsEnabled
                            ? _serverSettings.ServerData.VATPercentage
                            : 0);

                    _commandBus.Send(new CaptureCreditCardPayment
                    {
                        NewCardToken = paymentDetail.CardToken,
                        AccountId = accountDetail.Id,
                        PaymentId = paymentDetail.PaymentId,
                        Provider = _paymentService.ProviderType(overduePayment.OrderId),
                        Amount = overduePayment.OverdueAmount,
                        MeterAmount = Convert.ToDecimal(fareObject.AmountExclTax),
                        TipAmount = tipAmount,
                        TaxAmount = Convert.ToDecimal(fareObject.TaxAmount),
                        AuthorizationCode = commitResponse.AuthorizationCode,
                        TransactionId = commitResponse.TransactionId,
                        PromotionUsed = promotion != null ? promotion.PromoId : default(Guid?),
                        AmountSavedByPromotion = promotion != null ? promotion.AmountSaved : 0
                    });

                    _commandBus.Send(new SettleOverduePayment
                    {
                        AccountId = accountId,
                        OrderId = overduePayment.OrderId
                    });

                    return new SettleOverduePaymentResponse
                    {
                        IsSuccessful = true
                    };
                }

                // Payment failed, void preauth
                _paymentService.VoidPreAuthorization(overduePayment.OrderId);
            }

            return new SettleOverduePaymentResponse
            {
                IsSuccessful = false
            };
        }

        private decimal GetTipFromTotalAmount(Guid orderId, decimal overdueAmount)
        {
            var pairingInfo = _orderDao.FindOrderPairingById(orderId);
            decimal tipPercentage = pairingInfo.AutoTipPercentage ?? _serverSettings.ServerData.DefaultTipPercentage;

            var tip = tipPercentage / 100;
            return Math.Round(overdueAmount * tip, 2);
        }
    }
}