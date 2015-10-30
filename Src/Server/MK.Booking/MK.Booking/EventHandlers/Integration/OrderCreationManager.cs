using System;
using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Data;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.IBS.Impl;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
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
        private readonly IIbsOrderService _ibsOrderService;
        private readonly IBSServiceProvider _ibsServiceProvider;
        private readonly Resources.Resources _resources;

        public OrderCreationManager(
            Func<BookingDbContext> contextFactory,
            ICommandBus commandBus,
            IServerSettings serverSettings,
            ILogger logger,
            IOrderDao orderDao,
            IAccountDao accountDao,
            IIbsOrderService ibsOrderService,
            IBSServiceProvider ibsServiceProvider)
        {
            _contextFactory = contextFactory;
            _commandBus = commandBus;
            _logger = logger;
            _orderDao = orderDao;
            _accountDao = accountDao;
            _ibsOrderService = ibsOrderService;
            _ibsServiceProvider = ibsServiceProvider;

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
                var result = _ibsOrderService.CreateIbsOrder(@event.SourceId, @event.PickupAddress, @event.DropOffAddress, @event.Settings.AccountNumber,
                    @event.Settings.CustomerNumber, @event.CompanyKey, @event.IbsAccountId,
                    @event.Settings.Name, @event.Settings.Phone, @event.Settings.Passengers, @event.Settings.VehicleTypeId,
                    @event.IbsInformationNote, @event.PickupDate, @event.Prompts, @event.PromptsLength,
                    @event.ReferenceDataCompanyList.ToList(), @event.Market, @event.Settings.ChargeTypeId, @event.Settings.ProviderId, @event.Fare,
                    @event.TipIncentive, @event.IsHailRequest);

                ibsOrderId = result.CreateOrderResult;
            }

            var success = SendOrderCreationCommands(@event.SourceId, ibsOrderId, @event.IsPrepaid, @event.ClientLanguageCode);
            if (success)
            {
                SendConfirmationEmail(ibsOrderId.Value, @event);

                ApplyPromotionIfNecessary(@event);
            }

            _ibsOrderService.UpdateOrderStatusAsync(@event.SourceId);
        }

        public void Handle(IbsOrderSwitchInitiated @event)
        {
            // Order switched to another company

            var result = _ibsOrderService.CreateIbsOrder(@event.SourceId, @event.PickupAddress, @event.DropOffAddress, @event.Settings.AccountNumber,
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

            var result = _ibsOrderService.CreateIbsOrder(orderInfo.Request.OrderId, orderInfo.Request.PickupAddress, orderInfo.Request.DropOffAddress, orderInfo.Request.Settings.AccountNumber,
                orderInfo.Request.Settings.CustomerNumber, orderInfo.Request.CompanyKey, orderInfo.Request.IbsAccountId,
                orderInfo.Request.Settings.Name, orderInfo.Request.Settings.Phone, orderInfo.Request.Settings.Passengers, orderInfo.Request.Settings.VehicleTypeId,
                orderInfo.Request.IbsInformationNote, orderInfo.Request.PickupDate, orderInfo.Request.Prompts, orderInfo.Request.PromptsLength,
                orderInfo.Request.ReferenceDataCompanyList.ToList(), orderInfo.Request.Market, orderInfo.Request.Settings.ChargeTypeId,
                orderInfo.Request.Settings.ProviderId, orderInfo.Request.Fare, orderInfo.Request.TipIncentive);

            SendOrderCreationCommands(@event.SourceId, result.CreateOrderResult, true, orderInfo.Request.ClientLanguageCode);

            _ibsOrderService.UpdateOrderStatusAsync(@event.SourceId);
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
                _ibsOrderService.CancelIbsOrder(orderDetail.IBSOrderId, orderDetail.CompanyKey, orderDetail.Settings.Phone, orderDetail.AccountId);

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

        public void SendConfirmationEmail(int ibsOrderId, OrderCreated @event)
        {
            var chargeTypeKey = ChargeTypes.GetList()
                    .Where(x => x.Id == @event.Settings.ChargeTypeId)
                    .Select(x => x.Display)
                    .FirstOrDefault();

            var accountDetail = _accountDao.FindById(@event.AccountId);
            var chargeTypeEmail = _resources.Get(chargeTypeKey, @event.ClientLanguageCode);

            var emailCommand = new SendBookingConfirmationEmail
            {
                IBSOrderId = ibsOrderId,
                EmailAddress = accountDetail.Email,
                Settings = new SendBookingConfirmationEmail.BookingSettings
                {
                    ChargeType = @event.Settings.ChargeType,
                    LargeBags = @event.Settings.LargeBags,
                    Name = @event.Settings.Name,
                    Passengers = @event.Settings.Passengers,
                    Phone = @event.Settings.Phone,
                    VehicleType = @event.Settings.VehicleType
                },
                ClientLanguageCode = @event.ClientLanguageCode,
                DropOffAddress = @event.DropOffAddress,
                Note = @event.UserNote,
                PickupAddress = @event.PickupAddress,
                PickupDate = @event.PickupDate
            };

            emailCommand.IBSOrderId = ibsOrderId;
            emailCommand.EmailAddress = accountDetail.Email;
            emailCommand.Settings.ChargeType = chargeTypeEmail;
            emailCommand.Settings.VehicleType = @event.Settings.VehicleType;

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
    }
}
