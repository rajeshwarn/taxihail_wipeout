using System;
using System.Threading;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Resources;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Api.Services.Payment
{
    public class OverduePaymentService : BaseApiService
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

        public OverduePaymentDetail Get()
        {
            var accountId = Session.UserId;

            var overduePayment = _overduePaymentDao.FindNotPaidByAccountId(accountId);
            if (overduePayment != null)
            {
                // Client app can crash if this value is null. Make sure that it doesn't happen.
                overduePayment.IBSOrderId = overduePayment.IBSOrderId ?? 0;
            }

            return overduePayment;
        }

        public SettleOverduePaymentResponse Post()
        {
            var accountId = Session.UserId;

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

            // since a preauth at start of ride will never trigger a overdue payment, if we get a noshow/cancel to settle, it will be the only payment to settle
            // in the case of a succesful ride with another company, the overdue amount will contain the trip amount + booking fee so we have to separate them
            var fees = 0m;
            if (overduePayment.ContainBookingFees || overduePayment.ContainStandaloneFees)
            {
                fees = overduePayment.ContainBookingFees ? order.BookingFees : overduePayment.OverdueAmount;
                if (fees > 0)
                {
                    var feesSettled = SettleOverduePayment(order.Id, accountDetail, fees, null, true);
                    if (!feesSettled)
                    {
                        return new SettleOverduePaymentResponse
                        {
                            IsSuccessful = false
                        };
                    }
                }
            }

            var remainingToSettle = overduePayment.OverdueAmount - fees;
            if (remainingToSettle <= 0)
            {
                return new SettleOverduePaymentResponse
                {
                    IsSuccessful = true
                };
            }
                
            var paymentSettled = SettleOverduePayment(order.Id, accountDetail, remainingToSettle, order.CompanyKey, false);
            return new SettleOverduePaymentResponse
            {
                IsSuccessful = paymentSettled
            };
        }

        private bool SettleOverduePayment(Guid orderId, AccountDetail accountDetail, decimal amount, string companyKey, bool isFee)
        {
            var payment = _orderPaymentDao.FindByOrderId(orderId, companyKey);
            var reAuth = payment != null;

            var preAuthResponse = _paymentService.PreAuthorize(companyKey, orderId, accountDetail, amount, reAuth, isSettlingOverduePayment: true);
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

                    decimal meterAmount = amount;
                    decimal tipAmount = 0;

                    if (!isFee)
                    {
                        var pairingInfo = _orderDao.FindOrderPairingById(orderId);
                        tipAmount = FareHelper.GetTipAmountFromTotalIncludingTip(amount, pairingInfo.AutoTipPercentage ?? _serverSettings.ServerData.DefaultTipPercentage);
                        meterAmount = meterAmount - tipAmount;
                    }

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
                        FeeType = isFee ? FeeTypes.Booking : FeeTypes.None
                    });

                    _commandBus.Send(new SettleOverduePayment
                    {
                        AccountId = accountDetail.Id,
                        OrderId = orderId,
                        CreditCardId = accountDetail.DefaultCreditCard.GetValueOrDefault()
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