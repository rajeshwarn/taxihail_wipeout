using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Data;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.IBS.Impl;
using apcurium.MK.Booking.Jobs;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using AutoMapper;
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
        private readonly ICommandBus _commandBus;
        private readonly ILogger _logger;
        private readonly IAccountDao _accountDao;
        private readonly IOrderDao _orderDao;
        private readonly IServerSettings _serverSettings;
        private readonly IBSServiceProvider _ibsServiceProvider;
        private readonly IUpdateOrderStatusJob _updateOrderStatusJob;

        private readonly Resources.Resources _resources;

        public OrderCreationManager(ICommandBus commandBus,
            ILogger logger,
            IAccountDao accountDao,
            IOrderDao orderDao,
            IServerSettings serverSettings,
            IBSServiceProvider ibsServiceProvider,
            IUpdateOrderStatusJob updateOrderStatusJob)
        {
            _commandBus = commandBus;
            _logger = logger;
            _accountDao = accountDao;
            _orderDao = orderDao;
            _serverSettings = serverSettings;
            _ibsServiceProvider = ibsServiceProvider;
            _updateOrderStatusJob = updateOrderStatusJob;

            _resources = new Resources.Resources(_serverSettings);
        }

        public void Handle(OrderCreated @event)
        {
            // Normal order flow

            var ibsOrderId = CreateIbsOrder(@event.PickupAddress, @event.DropOffAddress, @event.Settings.AccountNumber,
                @event.Settings.CustomerNumber, @event.CompanyKey, @event.IbsAccountId,
                @event.Settings.Name, @event.Settings.Phone, @event.Settings.Passengers, @event.Settings.VehicleTypeId,
                @event.IbsInformationNote, @event.PickupDate, @event.Prompts, @event.PromptsLength,
                @event.ReferenceDataCompanyList.ToList(), @event.Market, @event.Settings.ChargeTypeId, @event.Settings.ProviderId, @event.Fare);

            var success = SendOrderCreationCommands(@event.SourceId, ibsOrderId, @event.IsPrepaid, @event.ClientLanguageCode);
            if (success)
            {
                SendConfirmationEmail(ibsOrderId.Value, @event);

                ApplyPromotionIfNecessary(@event);
            }

            UpdateStatusAsync(@event.SourceId);
        }

        public void Handle(IbsOrderSwitchInitiated @event)
        {
            // Order switched to another company

            var newIbsOrderId = CreateIbsOrder(@event.PickupAddress, @event.DropOffAddress, @event.Settings.AccountNumber,
                @event.Settings.CustomerNumber, @event.CompanyKey, @event.IbsAccountId,
                @event.Settings.Name, @event.Settings.Phone, @event.Settings.Passengers, @event.Settings.VehicleTypeId,
                @event.IbsInformationNote, @event.PickupDate, null, null,
                @event.ReferenceDataCompanyList.ToList(), @event.Market, @event.Settings.ChargeTypeId, @event.Settings.ProviderId, @event.Fare);

            SendOrderCreationCommands(@event.SourceId, newIbsOrderId, @event.IsPrepaid, @event.ClientLanguageCode);
        }

        public void Handle(PrepaidOrderPaymentInfoUpdated @event)
        {
            // Paypal web (prepaid) flow

            var temporaryInfo = _orderDao.GetTemporaryInfo(@event.OrderId);
            var orderInfo = JsonSerializer.DeserializeFromString<TemporaryOrderCreationInfo>(temporaryInfo.SerializedOrderCreationInfo);

            var ibsOrderId = CreateIbsOrder(orderInfo.Request.PickupAddress, orderInfo.Request.DropOffAddress, orderInfo.Request.Settings.AccountNumber,
                orderInfo.Request.Settings.CustomerNumber, orderInfo.Request.CompanyKey, orderInfo.Request.IbsAccountId,
                orderInfo.Request.Settings.Name, orderInfo.Request.Settings.Phone, orderInfo.Request.Settings.Passengers, orderInfo.Request.Settings.VehicleTypeId,
                orderInfo.Request.IbsInformationNote, orderInfo.Request.PickupDate, orderInfo.Request.Prompts, orderInfo.Request.PromptsLength,
                orderInfo.Request.ReferenceDataCompanyList.ToList(), orderInfo.Request.Market, orderInfo.Request.Settings.ChargeTypeId, orderInfo.Request.Settings.ProviderId, orderInfo.Request.Fare);

            SendOrderCreationCommands(@event.SourceId, ibsOrderId, true, orderInfo.Request.ClientLanguageCode);

            UpdateStatusAsync(@event.SourceId);
        }

        private int? CreateIbsOrder(Address pickupAddress, Address dropOffAddress, string accountNumberString, string customerNumberString, string companyKey,
            int ibsAccountId, string name, string phone, int passengers, int? vehicleTypeId, string ibsInformationNote,
            DateTime pickupDate, string[] prompts, int?[] promptsLength, IList<ListItem> referenceDataCompanyList, string market, int? chargeTypeId,
            int? requestProviderId, Fare fare)
        {
            int? ibsChargeTypeId;

            if (chargeTypeId == ChargeTypes.CardOnFile.Id
                || chargeTypeId == ChargeTypes.PayPal.Id)
            {
                ibsChargeTypeId = _serverSettings.ServerData.IBS.PaymentTypeCardOnFileId;
            }
            else if (chargeTypeId == ChargeTypes.Account.Id)
            {
                ibsChargeTypeId = _serverSettings.ServerData.IBS.PaymentTypeChargeAccountId;
            }
            else
            {
                ibsChargeTypeId = _serverSettings.ServerData.IBS.PaymentTypePaymentInCarId;
            }

            var defaultCompany = referenceDataCompanyList.FirstOrDefault(x => x.IsDefault.HasValue && x.IsDefault.Value)
                    ?? referenceDataCompanyList.FirstOrDefault();

            var providerId = market.HasValue() && referenceDataCompanyList.Any() && defaultCompany != null
                    ? defaultCompany.Id
                    : requestProviderId;

            var ibsPickupAddress = Mapper.Map<IbsAddress>(pickupAddress);
            var ibsDropOffAddress = dropOffAddress != null && dropOffAddress.IsValid()
                ? Mapper.Map<IbsAddress>(dropOffAddress)
                : null;

            var customerNumber = GetCustomerNumber(accountNumberString, customerNumberString);

            int? ibsOrderId;

            if (_serverSettings.ServerData.IBS.FakeOrderStatusUpdate)
            {
                // Fake IBS order id
                ibsOrderId = new Random(Guid.NewGuid().GetHashCode()).Next(90000, 90000000);
            }
            else
            {
                ibsOrderId = _ibsServiceProvider.Booking(companyKey).CreateOrder(
                    providerId,
                    ibsAccountId,
                    name,
                    phone,
                    passengers,
                    vehicleTypeId,
                    ibsChargeTypeId,
                    ibsInformationNote,
                    pickupDate,
                    ibsPickupAddress,
                    ibsDropOffAddress,
                    accountNumberString,
                    customerNumber,
                    prompts,
                    promptsLength,
                    fare);
            }

            return ibsOrderId;
        }

        private bool SendOrderCreationCommands(Guid orderId, int? ibsOrderId, bool isPrepaid, string clientLanguageCode,
            bool switchedCompany = false, string newCompanyKey = null, string newCompanyName = null, string market = null)
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
                CancelIbsOrder(orderDetail.IBSOrderId, orderDetail.CompanyKey, orderDetail.Settings.Phone, orderDetail.AccountId);

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
                var ibsCommand = new AddIbsOrderInfoToOrder
                {
                    OrderId = orderId,
                    IBSOrderId = ibsOrderId.Value
                };
                _commandBus.Send(ibsCommand);

                return true;
            }
        }

        private void SendConfirmationEmail(int ibsOrderId, OrderCreated @event)
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

        private void UpdateStatusAsync(Guid orderId)
        {
            new TaskFactory().StartNew(() => _updateOrderStatusJob.CheckStatus(orderId));
        }

        private int? GetCustomerNumber(string accountNumber, string customerNumber)
        {
            if (!accountNumber.HasValue() || !customerNumber.HasValue())
            {
                return null;
            }

            int result;
            if (int.TryParse(customerNumber, out result))
            {
                return result;
            }

            return null;
        }

        private void CancelIbsOrder(int? ibsOrderId, string companyKey, string phone, Guid accountId)
        {
            // Cancel order on current company IBS
            if (ibsOrderId.HasValue)
            {
                var currentIbsAccountId = _accountDao.GetIbsAccountId(accountId, companyKey);
                if (currentIbsAccountId.HasValue)
                {
                    // We need to try many times because sometime the IBS cancel method doesn't return an error but doesn't cancel the ride...
                    // After 5 time, we are giving up. But we assume the order is completed.
                    Task.Factory.StartNew(() =>
                    {
                        Func<bool> cancelOrder = () => _ibsServiceProvider.Booking(companyKey)
                            .CancelOrder(ibsOrderId.Value, currentIbsAccountId.Value, phone);

                        cancelOrder.Retry(new TimeSpan(0, 0, 0, 10), 5);
                    });
                }
            }
        }
    }
}
