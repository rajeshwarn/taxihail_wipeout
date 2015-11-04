using System;
using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using CMTPayment;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;
using ServiceStack.ServiceClient.Web;

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class OrderPaymentManager :
        IIntegrationEventHandler,
        IEventHandler<CreditCardPaymentCaptured_V2>,
        IEventHandler<OrderCancelled>,
        IEventHandler<OrderSwitchedToNextDispatchCompany>,
        IEventHandler<OrderStatusChanged>,
        IEventHandler<OrderCancelledBecauseOfError>,
        IEventHandler<ManualRideLinqTripInfoUpdated>
    {
        private readonly IOrderDao _dao;
        private readonly IIbsOrderService _ibs;
        private readonly IServerSettings _serverSettings;
        private readonly IPaymentService _paymentService;
        private readonly IFeeService _feeService;
        private readonly ILogger _logger;
        private readonly IOrderPaymentDao _paymentDao;
        private readonly ICreditCardDao _creditCardDao;
        private readonly IAccountDao _accountDao;
        private readonly IOrderDao _orderDao;
        private readonly ICommandBus _commandBus;

        private CmtTripInfoServiceHelper _cmtTripInfoServiceHelper;

        public OrderPaymentManager(IOrderDao dao, IOrderPaymentDao paymentDao, IAccountDao accountDao, IOrderDao orderDao, ICommandBus commandBus,
            ICreditCardDao creditCardDao, IIbsOrderService ibs, IServerSettings serverSettings, IPaymentService paymentService, IFeeService feeService, ILogger logger)
        {
            _accountDao = accountDao;
            _orderDao = orderDao;
            _commandBus = commandBus;
            _dao = dao;
            _paymentDao = paymentDao;
            _creditCardDao = creditCardDao;
            _ibs = ibs;
            _serverSettings = serverSettings;
            _paymentService = paymentService;
            _feeService = feeService;
            _logger = logger;
        }

        public void Handle(CreditCardPaymentCaptured_V2 @event)
        {
            // Migration
            @event.MigrateFees();

            if (@event.FeeType != FeeTypes.None)
            {
                // Don't message driver
                return;
            }

            var taxedMeterAmount = @event.Meter + @event.Tax;

            if (_serverSettings.ServerData.SendDetailedPaymentInfoToDriver
                && !@event.IsSettlingOverduePayment) // Don't send notification to driver when user settles overdue payment
            {
                // To prevent driver confusion we will not send the discounted total amount for the fare.
                // We will also not send booking fee since it could be from a market company and the driver would not know where it's coming from.
                var totalAmountBeforePromotionAndBookingFees = @event.Amount + @event.AmountSavedByPromotion - @event.BookingFees;
                SendPaymentConfirmationToDriver(@event.OrderId, totalAmountBeforePromotionAndBookingFees, taxedMeterAmount, @event.Tip, @event.Provider.ToString(), @event.AuthorizationCode);
            }

            if (@event.PromotionUsed.HasValue)
            {
                var redeemPromotion = new RedeemPromotion
                {
                    OrderId = @event.OrderId,
                    PromoId = @event.PromotionUsed.Value,
                    TaxedMeterAmount = taxedMeterAmount, // MK: Booking fees don't count towards promo rebate (2015/05/25)
                    TipAmount = @event.Tip
                };

                var envelope = (Envelope<ICommand>)redeemPromotion;

                _commandBus.Send(envelope);
            }
        }

        private void SendPaymentConfirmationToDriver(Guid orderId, decimal totalAmountBeforePromotion, decimal taxedMeterAmount, decimal tipAmount, string provider,  string authorizationCode)
        {
            // Send message to driver
            var orderStatusDetail = _dao.FindOrderStatusById(orderId);
            if (orderStatusDetail == null) throw new InvalidOperationException("Order Status not found");

            // Confirm to IBS that order was payed
            var orderDetail = _dao.FindById(orderId);
            if (orderDetail == null) throw new InvalidOperationException("Order not found");

            var payment = _paymentDao.FindByOrderId(orderId, orderDetail.CompanyKey);
            if (payment == null) throw new InvalidOperationException("Payment info not found");

            var account = _accountDao.FindById(orderDetail.AccountId);
            if (account == null) throw new InvalidOperationException("Order account not found");

            if ( provider == PaymentType.CreditCard.ToString () )
            {
                var card = _creditCardDao.FindByToken(payment.CardToken);
                if (card == null) throw new InvalidOperationException("Credit card not found");
            }

            _ibs.SendPaymentNotification((double)totalAmountBeforePromotion, (double)taxedMeterAmount, (double)tipAmount, authorizationCode, orderStatusDetail.VehicleNumber, orderStatusDetail.ServiceType, orderStatusDetail.CompanyKey);
        }

        public void Handle(OrderCancelled @event)
        {
            var orderDetail = _orderDao.FindOrderStatusById(@event.SourceId);
            if (orderDetail.IsPrepaid)
            {
                var response = _paymentService.RefundPayment(orderDetail.CompanyKey, @event.SourceId);

                if (response.IsSuccessful)
                {
                    _commandBus.Send(new UpdateRefundedOrder
                    {
                        OrderId = @event.SourceId,
                        IsSuccessful = response.IsSuccessful,
                        Message = response.Message
                    });
                }
            }
            else
            {
                var feeCharged = _feeService.ChargeCancellationFeeIfNecessary(orderDetail);

                if (orderDetail.CompanyKey != null)
                {
                    // Company not-null will never (so far) perceive no show fees, so we need to void its preauth
                    _paymentService.VoidPreAuthorization(orderDetail.CompanyKey, orderDetail.OrderId);
                }
                else
                {
                    if (!feeCharged.HasValue)
                    {
                        // No fees were charged on company null, void the preauthorization to prevent misuse fees
                        _paymentService.VoidPreAuthorization(orderDetail.CompanyKey, @event.SourceId);
                    }
                }
            }
        }

        public void Handle(OrderSwitchedToNextDispatchCompany @event)
        {
            if (@event.HasChangedBackToPaymentInCar)
            {
                var orderStatus = _orderDao.FindOrderStatusById(@event.SourceId);
                if (orderStatus.IsPrepaid)
                {
                    _paymentService.RefundPayment(orderStatus.CompanyKey, @event.SourceId);
                }
                else
                {
                    // void the preauthorization to prevent misuse fees
                    _paymentService.VoidPreAuthorization(orderStatus.CompanyKey, @event.SourceId);
                }
            }
        }

        public void Handle(OrderCancelledBecauseOfError @event)
        {
            var orderDetail = _orderDao.FindOrderStatusById(@event.SourceId);
            if (orderDetail.IsPrepaid)
            {
                var response = _paymentService.RefundPayment(orderDetail.CompanyKey, @event.SourceId);

                if (response.IsSuccessful)
                {
                    _commandBus.Send(new UpdateRefundedOrder
                    {
                        OrderId = @event.SourceId,
                        IsSuccessful = response.IsSuccessful,
                        Message = response.Message
                    });
                }
            }
            else
            {
                // void the preauthorization to prevent misuse fees
                _paymentService.VoidPreAuthorization(orderDetail.CompanyKey, @event.SourceId);
            }
        }

        public void Handle(OrderStatusChanged @event)
        {
            if (@event.IsCompleted)
            {
                var order = _orderDao.FindById(@event.SourceId);
                var orderStatus = _orderDao.FindOrderStatusById(@event.SourceId);
                var pairingInfo = _orderDao.FindOrderPairingById(@event.SourceId);

                if (_serverSettings.GetPaymentSettings(order.CompanyKey).PaymentMode == PaymentMethod.RideLinqCmt)
                {
                    // Check if card declined
                    InitializeCmtServiceClient();

                    var trip = _cmtTripInfoServiceHelper.CheckForTripEndErrors(pairingInfo.PairingToken);

                    if (trip != null && trip.ErrorCode == CmtErrorCodes.CardDeclined)
                    {
                        _commandBus.Send(new ReactToPaymentFailure
                        {
                            AccountId = order.AccountId,
                            OrderId = order.Id,
                            IBSOrderId = order.IBSOrderId,
                            TransactionId = orderStatus.OrderId.ToString().Split('-').FirstOrDefault(), // Use first part of GUID to display to user
                            OverdueAmount = Convert.ToDecimal(@event.Fare + @event.Tax + @event.Tip + @event.Toll),
                            TransactionDate = @event.EventDate
                        });

                        return;
                    }

                    // Since RideLinqCmt payment is processed automatically by CMT, we have to charge booking fees separately
                    _feeService.ChargeBookingFeesIfNecessary(orderStatus);
                }

                // If the user has decided not to pair (paying the ride in car instead),
                // we have to void the amount that was preauthorized
                if (_serverSettings.GetPaymentSettings(order.CompanyKey).PaymentMode != PaymentMethod.RideLinqCmt
                    && (order.Settings.ChargeTypeId == ChargeTypes.CardOnFile.Id || order.Settings.ChargeTypeId == ChargeTypes.PayPal.Id)
                    && (pairingInfo == null || pairingInfo.WasUnpaired)
                    && !orderStatus.IsPrepaid) //prepaid order will never have a pairing info
                {
                    // void the preauthorization to prevent misuse fees
                    _paymentService.VoidPreAuthorization(order.CompanyKey, @event.SourceId);
                }
            }
        }

        public void Handle(ManualRideLinqTripInfoUpdated @event)
        {
            if (@event.EndTime.HasValue)
            {
                var orderStatus = _orderDao.FindOrderStatusById(@event.SourceId);
                if (orderStatus != null)
                {
                    // Check if card declined
                    InitializeCmtServiceClient();

                    var trip = _cmtTripInfoServiceHelper.CheckForTripEndErrors(@event.PairingToken);

                    if (trip != null && trip.ErrorCode == CmtErrorCodes.CardDeclined)
                    {
                        _commandBus.Send(new ReactToPaymentFailure
                        {
                            AccountId = orderStatus.AccountId,
                            OrderId = orderStatus.OrderId,
                            IBSOrderId = orderStatus.IBSOrderId,
                            TransactionId = orderStatus.OrderId.ToString().Split('-').FirstOrDefault(), // Use first part of GUID to display to user
                            OverdueAmount = Convert.ToDecimal(@event.Fare + @event.Tax + @event.Tip + @event.Toll),
                            TransactionDate = @event.EventDate
                        });

                        return;
                    }

                    // Since RideLinqCmt payment is processed automatically by CMT, we have to charge booking fees separately
                    _feeService.ChargeBookingFeesIfNecessary(orderStatus);
                }
            }
        }

        private void InitializeCmtServiceClient()
        {
            var cmtMobileServiceClient = new CmtMobileServiceClient(_serverSettings.GetPaymentSettings().CmtPaymentSettings, null, null);
            _cmtTripInfoServiceHelper = new CmtTripInfoServiceHelper(cmtMobileServiceClient, _logger);
        }
    }
}