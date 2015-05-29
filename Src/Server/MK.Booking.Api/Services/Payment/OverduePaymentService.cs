using System;
using System.Threading;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel;
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
        private readonly IFeesDao _feesDao;

        public OverduePaymentService(
            ICommandBus commandBus,
            IOverduePaymentDao overduePaymentDao,
            IAccountDao accountDao,
            IOrderDao orderDao,
            IOrderPaymentDao orderPaymentDao,
            IPromotionDao promotionDao,
            IPaymentService paymentService,
            IServerSettings serverSettings,
            IFeesDao feesDao)
        {
            _commandBus = commandBus;
            _overduePaymentDao = overduePaymentDao;
            _accountDao = accountDao;
            _orderDao = orderDao;
            _orderPaymentDao = orderPaymentDao;
            _promotionDao = promotionDao;
            _paymentService = paymentService;
            _serverSettings = serverSettings;
            _feesDao = feesDao;
        }

        public object Get(OverduePaymentRequest request)
        {
            var session = this.GetSession();
            var accountId = new Guid(session.UserAuthId);

            var overduePayment = _overduePaymentDao.FindNotPaidByAccountId(accountId);

            // Client app can crash if this value is null. Make sure that it doesn't happen.
            overduePayment.IBSOrderId = overduePayment.IBSOrderId ?? 0;

            return overduePayment;
        }

        public object Post(SettleOverduePaymentRequest request)
        {
            var session = this.GetSession();
            var accountId = new Guid(session.UserAuthId);

            var overduePayment = _overduePaymentDao.FindNotPaidByAccountId(accountId);
            if (overduePayment == null)
            {
                return new SettleOverduePaymentResponse
                {
                    IsSuccessful = true,
                    Message = "No overdue payment to settle"
                };
            }

            var order = _orderDao.FindById(overduePayment.OrderId);
            var accountDetail = _accountDao.FindById(accountId);

            var bookingFees = 0m;
            if (overduePayment.ContainFees)
            {
                // this assumes it's a booking fee, otherwise it will not work
                bookingFees = order.BookingFees;
                if (bookingFees > 0)
                {
                    var feesSettled = SettleOverduePayment(order.Id, accountDetail, bookingFees, null, false);
                    if (!feesSettled)
                    {
                        return new SettleOverduePaymentResponse
                        {
                            IsSuccessful = false
                        };
                    }
                }
            }
            
            var paymentSettled = SettleOverduePayment(order.Id, accountDetail, overduePayment.OverdueAmount - bookingFees, order.CompanyKey, false);
            return new SettleOverduePaymentResponse
            {
                IsSuccessful = paymentSettled
            };
        }

        private bool SettleOverduePayment(Guid orderId, AccountDetail accountDetail, decimal amount, string companyKey, bool isFee /* TODO: fix*/)
        {
            var payment = _orderPaymentDao.FindByOrderId(orderId, companyKey);
            var reAuth = payment != null;

            var preAuthResponse = _paymentService.PreAuthorize(companyKey, orderId, accountDetail, amount, reAuth, true);
            if (preAuthResponse.IsSuccessful)
            {
                // Wait for payment to be created
                Thread.Sleep(500);

                var commitResponse = _paymentService.CommitPayment(
                    companyKey,
                    orderId,
                    accountDetail,
                    amount,
                    amount,
                    amount,
                    0,
                    preAuthResponse.TransactionId,
                    preAuthResponse.ReAuthOrderId);

                if (commitResponse.IsSuccessful)
                {
                    // Go fetch declined order, and send its receipt
                    var paymentDetail = _orderPaymentDao.FindByOrderId(orderId, companyKey);
                    var promotion = _promotionDao.FindByOrderId(orderId);

                    var pairingInfo = _orderDao.FindOrderPairingById(orderId);
                    var tipAmount = FareHelper.GetTipAmountFromTotalIncludingTip(amount, pairingInfo.AutoTipPercentage ?? _serverSettings.ServerData.DefaultTipPercentage);
                    
                    var meterAmount = isFee ? amount : amount - tipAmount;

                    var fareObject = FareHelper.GetFareFromAmountInclTax(meterAmount,
                        _serverSettings.ServerData.VATIsEnabled
                            ? _serverSettings.ServerData.VATPercentage
                            : 0);

                    var orderDetail = _orderDao.FindById(orderId);

                    _commandBus.Send(new CaptureCreditCardPayment
                    {
                        IsSettlingOverduePayment = true,
                        NewCardToken = paymentDetail.CardToken,
                        AccountId = accountDetail.Id,
                        PaymentId = paymentDetail.PaymentId,
                        Provider = _paymentService.ProviderType(companyKey, orderId),
                        TotalAmount = amount,
                        MeterAmount = fareObject.AmountExclTax,
                        TipAmount = tipAmount,
                        TaxAmount = fareObject.TaxAmount,
                        TollAmount = Convert.ToDecimal(orderDetail.Toll ?? 0),
                        SurchargeAmount = Convert.ToDecimal(orderDetail.Surcharge ?? 0),
                        AuthorizationCode = commitResponse.AuthorizationCode,
                        TransactionId = commitResponse.TransactionId,
                        PromotionUsed = promotion != null ? promotion.PromoId : default(Guid?),
                        AmountSavedByPromotion = promotion != null ? promotion.AmountSaved : 0,
                        IsBookingFee = isFee
                    });

                    _commandBus.Send(new SettleOverduePayment
                    {
                        AccountId = accountDetail.Id,
                        OrderId = orderId
                    });

                    return true;
                }

                // Payment failed, void preauth
                _paymentService.VoidPreAuthorization(companyKey, orderId);
            }

            return false;
        }
    }
}