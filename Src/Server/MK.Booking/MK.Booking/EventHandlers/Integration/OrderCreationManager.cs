using System;
using System.Linq;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Data;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;
using ServiceStack.Text;

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class OrderCreationManager : IIntegrationEventHandler,
        IEventHandler<OrderCreated>,
        IEventHandler<IbsOrderSwitchInitiated>,
        IEventHandler<PrepaidOrderPaymentInfoUpdated>
    {
        private readonly Func<BookingDbContext> _contextFactory;
        private readonly ICommandBus _commandBus;
        private readonly ILogger _logger;
        private readonly IOrderDao _orderDao;
        private readonly IAccountDao _accountDao;
        private readonly IIbsCreateOrderService _ibsCreateOrderService;
        private readonly Resources.Resources _resources;

        public OrderCreationManager(
            Func<BookingDbContext> contextFactory,
            ICommandBus commandBus,
            IServerSettings serverSettings,
            ILogger logger,
            IOrderDao orderDao,
            IAccountDao accountDao,
            IIbsCreateOrderService ibsCreateOrderService)
        {
            _contextFactory = contextFactory;
            _commandBus = commandBus;
            _logger = logger;
            _orderDao = orderDao;
            _accountDao = accountDao;
            _ibsCreateOrderService = ibsCreateOrderService;

            _resources = new Resources.Resources(serverSettings);
        }

        public void Handle(OrderCreated @event)
        {
            // Normal order flow

            var isPaypalPrepaid = @event.IsPrepaid
                && @event.Settings.ChargeTypeId == ChargeTypes.PayPal.Id;

            if (isPaypalPrepaid)
            {
                // Paypal orders are handled in their own methods
                return;
            }

            var ibsOrderId = @event.IBSOrderId;

            if (!ibsOrderId.HasValue)
            {
                // If order wasn't already created on IBS (which should be most of the time), we create it here
                var result = _ibsCreateOrderService.CreateIbsOrder(@event.SourceId, @event.PickupAddress, @event.DropOffAddress, @event.Settings.AccountNumber,
                    @event.Settings.CustomerNumber, @event.CompanyKey, @event.IbsAccountId,
                    @event.Settings.Name, @event.Settings.Phone, @event.Settings.Passengers, @event.Settings.VehicleTypeId,
                    @event.IbsInformationNote, @event.PickupDate, @event.Prompts, @event.PromptsLength,
                    @event.ReferenceDataCompanyList.ToList(), @event.Market, @event.Settings.ChargeTypeId, @event.Settings.ProviderId, @event.Fare,
                    @event.TipIncentive);

                ibsOrderId = result.CreateOrderResult;
            }

            var success = SendOrderCreationCommands(@event.SourceId, ibsOrderId, @event.IsPrepaid, @event.ClientLanguageCode);
            if (success)
            {
                SendConfirmationEmail(ibsOrderId.Value, @event.AccountId, @event.Settings, @event.ChargeTypeEmail,
                    @event.PickupAddress, @event.DropOffAddress, @event.PickupDate, @event.UserNote, @event.ClientLanguageCode);

                ApplyPromotionIfNecessary(@event);
            }

            _ibsCreateOrderService.UpdateOrderStatusAsync(@event.SourceId);
        }

        public void Handle(IbsOrderSwitchInitiated @event)
        {
            // Order switched to another company

            var result = _ibsCreateOrderService.CreateIbsOrder(@event.SourceId, @event.PickupAddress, @event.DropOffAddress, @event.Settings.AccountNumber,
                @event.Settings.CustomerNumber, @event.CompanyKey, @event.IbsAccountId,
                @event.Settings.Name, @event.Settings.Phone, @event.Settings.Passengers, @event.Settings.VehicleTypeId,
                @event.IbsInformationNote, @event.PickupDate, null, null,
                @event.ReferenceDataCompanyList.ToList(), @event.Market, @event.Settings.ChargeTypeId, @event.Settings.ProviderId, @event.Fare,
                @event.TipIncentive);

            SendOrderCreationCommands(@event.SourceId, result.CreateOrderResult, @event.IsPrepaid, @event.ClientLanguageCode, true, @event.CompanyKey, @event.CompanyName, @event.Market);
        }

        public void Handle(PrepaidOrderPaymentInfoUpdated @event)
        {
            // Paypal web (prepaid) flow

            var temporaryInfo = _orderDao.GetTemporaryInfo(@event.OrderId);
            var orderInfo = JsonSerializer.DeserializeFromString<TemporaryOrderCreationInfo>(temporaryInfo.SerializedOrderCreationInfo);

            DeleteTempOrderData(@event.OrderId);

            var result = _ibsCreateOrderService.CreateIbsOrder(orderInfo.Request.OrderId, orderInfo.Request.PickupAddress, orderInfo.Request.DropOffAddress, orderInfo.Request.Settings.AccountNumber,
                orderInfo.Request.Settings.CustomerNumber, orderInfo.Request.CompanyKey, orderInfo.Request.IbsAccountId,
                orderInfo.Request.Settings.Name, orderInfo.Request.Settings.Phone, orderInfo.Request.Settings.Passengers, orderInfo.Request.Settings.VehicleTypeId,
                orderInfo.Request.IbsInformationNote, orderInfo.Request.PickupDate, orderInfo.Request.Prompts, orderInfo.Request.PromptsLength,
                orderInfo.Request.ReferenceDataCompanyList.ToList(), orderInfo.Request.Market, orderInfo.Request.Settings.ChargeTypeId,
                orderInfo.Request.Settings.ProviderId, orderInfo.Request.Fare, orderInfo.Request.TipIncentive);

            var success = SendOrderCreationCommands(@event.SourceId, result.CreateOrderResult, true, orderInfo.Request.ClientLanguageCode);
            if (success)
            {
                SendConfirmationEmail(result.CreateOrderResult.Value, orderInfo.AccountId, orderInfo.Request.Settings, orderInfo.ChargeTypeEmail,
                    orderInfo.Request.PickupAddress, orderInfo.Request.DropOffAddress, orderInfo.Request.PickupDate, orderInfo.Request.UserNote, orderInfo.Request.ClientLanguageCode);
            }

            _ibsCreateOrderService.UpdateOrderStatusAsync(@event.SourceId);
        }

        public bool SendOrderCreationCommands(Guid orderId, int? ibsOrderId, bool isPrepaid, string clientLanguageCode, bool switchedCompany = false, string newCompanyKey = null, string newCompanyName = null, string market = null)
        {
            if (!ibsOrderId.HasValue
                || ibsOrderId <= 0)
            {
                var code = !ibsOrderId.HasValue || (ibsOrderId.Value >= -1) ? string.Empty : "_" + Math.Abs(ibsOrderId.Value);
                var errorCode = "CreateOrder_CannotCreateInIbs" + code;

                var errorCommand = new CancelOrderBecauseOfError
                {
                    OrderId = orderId,
                    WasPrepaid = isPrepaid,
                    ErrorCode = errorCode,
                    ErrorDescription = _resources.Get(errorCode, clientLanguageCode)
                };

                _commandBus.Send(errorCommand);

                return false;
            }
            else if (switchedCompany)
            {
                var orderDetail = _orderDao.FindById(orderId);

                // Cancel order on current company IBS
                _ibsCreateOrderService.CancelIbsOrder(orderDetail.IBSOrderId, orderDetail.CompanyKey, orderDetail.Settings.Phone, orderDetail.AccountId);

                _commandBus.Send(new SwitchOrderToNextDispatchCompany
                {
                    OrderId = orderId,
                    IBSOrderId = ibsOrderId.Value,
                    CompanyKey = newCompanyKey,
                    CompanyName = newCompanyName,
                    Market = market
                });

                return true;
            }
            else
            {
                _logger.LogMessage(string.Format("Adding IBSOrderId {0} to order {1}", ibsOrderId, orderId));

                var ibsCommand = new AddIbsOrderInfoToOrder
                {
                    OrderId = orderId,
                    IBSOrderId = ibsOrderId.Value
                };
                _commandBus.Send(ibsCommand);

                return true;
            }
        }

        public void SendConfirmationEmail(int ibsOrderId, Guid accountId, BookingSettings bookingSettings, string chargeTypeEmail,
            Address pickupAddress, Address dropOffAddress, DateTime pickupDate, string userNote, string clientLanguage)
        {
            var accountDetail = _accountDao.FindById(accountId);

            chargeTypeEmail = chargeTypeEmail ?? GetChargeTypeEmail(bookingSettings.ChargeTypeId, clientLanguage);

            var emailCommand = new SendBookingConfirmationEmail
            {
                IBSOrderId = ibsOrderId,
                EmailAddress = accountDetail.Email,
                Settings = new SendBookingConfirmationEmail.BookingSettings
                {
                    ChargeType = bookingSettings.ChargeType,
                    LargeBags = bookingSettings.LargeBags,
                    Name = bookingSettings.Name,
                    Passengers = bookingSettings.Passengers,
                    Phone = bookingSettings.Phone,
                    VehicleType = bookingSettings.VehicleType
                },
                ClientLanguageCode = clientLanguage,
                DropOffAddress = dropOffAddress,
                Note = userNote,
                PickupAddress = pickupAddress,
                PickupDate = pickupDate
            };

            emailCommand.IBSOrderId = ibsOrderId;
            emailCommand.EmailAddress = accountDetail.Email;
            emailCommand.Settings.ChargeType = chargeTypeEmail;
            emailCommand.Settings.VehicleType = bookingSettings.VehicleType;

            _commandBus.Send(emailCommand);
        }

        private void ApplyPromotionIfNecessary(OrderCreated @event)
        {
            if (@event.PromotionId.HasValue)
            {
                var applyPromotionCommand = new ApplyPromotion
                {
                    PromoId = @event.PromotionId.Value,
                    AccountId = @event.AccountId,
                    OrderId = @event.SourceId,
                    PickupDate = @event.PickupDate,
                    IsFutureBooking = @event.IsFutureBooking
                };

                _commandBus.Send(applyPromotionCommand);
            }
        }

        private void DeleteTempOrderData(Guid orderId)
        {
            try
            {
                using (var context = _contextFactory.Invoke())
                {
                    context.RemoveWhere<TemporaryOrderCreationInfoDetail>(x => x.OrderId == orderId);
                    context.SaveChanges();

                    _logger.LogMessage(string.Format("Temporary data for order {0} deleted", orderId));
                }
            }
            catch (Exception ex)
            {
                _logger.LogMessage(string.Format("Unable to delete temporary data for order {0}", orderId));
                _logger.LogError(ex);
            }
        }

        private string GetChargeTypeEmail(int? chargeTypeId, string clientLanguageCode)
        {
            var chargeTypeKey = ChargeTypes.GetList()
                    .Where(x => x.Id == chargeTypeId)
                    .Select(x => x.Display)
                    .FirstOrDefault();

            return _resources.Get(chargeTypeKey, clientLanguageCode);
        }
    }
}
