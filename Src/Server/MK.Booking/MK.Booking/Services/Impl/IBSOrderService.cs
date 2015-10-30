#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using apcurium.MK.Booking.Data;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Jobs;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using AutoMapper;
using CMTServices;


#endregion

namespace apcurium.MK.Booking.Services.Impl
{
    public class IbsOrderService : IIbsOrderService
    {
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly IServerSettings _serverSettings;
        private readonly ILogger _logger;
        private readonly IUpdateOrderStatusJob _updateOrderStatusJob;
        private readonly IVehicleTypeDao _vehicleTypeDao;
        private readonly IAccountDao _accountDao;
        private readonly Resources.Resources _resources;

        public IbsOrderService(
            IIBSServiceProvider ibsServiceProvider,
            IServerSettings serverSettings,
            ILogger logger,
            IUpdateOrderStatusJob updateOrderStatusJob,
            IVehicleTypeDao vehicleTypeDao,
            IAccountDao accountDao)
        {
            _ibsServiceProvider = ibsServiceProvider;
            _logger = logger;
            _updateOrderStatusJob = updateOrderStatusJob;
            _vehicleTypeDao = vehicleTypeDao;
            _accountDao = accountDao;
            _serverSettings = serverSettings;

            _resources = new Resources.Resources(_serverSettings);
        }

        public IBSOrderResult CreateIbsOrder(Guid orderId, Address pickupAddress, Address dropOffAddress, string accountNumberString, string customerNumberString, string companyKey,
            int ibsAccountId, string name, string phone, int passengers, int? vehicleTypeId, string ibsInformationNote,
            DateTime pickupDate, string[] prompts, int?[] promptsLength, IList<ListItem> referenceDataCompanyList, string market, int? chargeTypeId,
            int? requestProviderId, Fare fare, double? tipIncentive, bool isHailRequest = false)
        {
            if (_serverSettings.ServerData.IBS.FakeOrderStatusUpdate)
            {
                // Fake IBS order id
                return new IBSOrderResult
                {
                    CreateOrderResult = new Random(Guid.NewGuid().GetHashCode()).Next(90000, 90000000),
                    IsHailRequest = isHailRequest
                };
            }

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

            int? createOrderResult = null;
            var defaultVehicleType = _vehicleTypeDao.GetAll().FirstOrDefault();
            var defaultVehicleTypeId = defaultVehicleType != null ? defaultVehicleType.ReferenceDataVehicleId : -1;

            IbsHailResponse ibsHailResult = null;

            if (isHailRequest)
            {
                // Query avaiable vehicles
                var availableVehicleService = GetAvailableVehiclesServiceClient(market);
                var availableVehicles = availableVehicleService.GetAvailableVehicles(market, pickupAddress.Latitude, pickupAddress.Longitude).ToArray();

                var vehicleCandidates = availableVehicles.Select(vehicle => new IbsVehicleCandidate
                {
                    CandidateType = VehicleCandidateTypes.VctPimId,
                    VehicleId = vehicle.DeviceName,
                    ETADistance = (int?)vehicle.DistanceToArrival ?? 0,
                    ETATime = (int?)vehicle.Eta ?? 0
                });

                ibsHailResult = _ibsServiceProvider.Booking(companyKey).Hail(
                    orderId,
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
                    defaultVehicleTypeId,
                    vehicleCandidates,
                    tipIncentive,
                    fare);

                // Fetch vehicle candidates (who have accepted the hail request) only if order was successfully created on IBS
                if (ibsHailResult.OrderKey.IbsOrderId > -1)
                {
                    // TODO: replace hardcoded value by timeout returned by IBS
                    // Need to wait for vehicles to receive hail request
                    Thread.Sleep(25000);

                    var candidates = _ibsServiceProvider.Booking(companyKey).GetVehicleCandidates(ibsHailResult.OrderKey);
                    ibsHailResult.VehicleCandidates = candidates;
                }
            }
            else
            {
                createOrderResult = _ibsServiceProvider.Booking(companyKey).CreateOrder(
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
                    defaultVehicleTypeId,
                    tipIncentive,
                    fare);
            }

            var hailResult = Mapper.Map<OrderHailResult>(ibsHailResult);

            return new IBSOrderResult
            {
                CreateOrderResult = createOrderResult,
                HailResult = hailResult,
                IsHailRequest = isHailRequest
            };
        }

        public void CancelIbsOrder(int? ibsOrderId, string companyKey, string phone, Guid accountId)
        {
            // Cancel order on current company IBS
            if (ibsOrderId.HasValue)
            {
                var currentIbsAccountId = _accountDao.GetIbsAccountId(accountId, companyKey);
                if (currentIbsAccountId.HasValue)
                {
                    _logger.LogMessage(string.Format("Cancelling IBSOrder {0}, on company {1}", ibsOrderId, companyKey));

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

        public void UpdateOrderStatusAsync(Guid orderId)
        {
            _logger.LogMessage(string.Format("Starting status updater for order {0}", orderId));

            new TaskFactory().StartNew(() => _updateOrderStatusJob.CheckStatus(orderId));
        }

        public void ConfirmExternalPayment(Guid orderId, int ibsOrderId, decimal totalAmount, decimal tipAmount, decimal meterAmount, string type, string provider, string transactionId,
                                           string authorizationCode, string cardToken, int accountId, string name, string phone, string email, string os, string userAgent, string companyKey)
        {
            if (companyKey.HasValue())
            {
                _logger.LogMessage(string.Format("Confirming external payment on external company '{0}'", companyKey));
            }

            if (!_ibsServiceProvider.Booking(companyKey).ConfirmExternalPayment(orderId, ibsOrderId, totalAmount, tipAmount, meterAmount, type, provider, transactionId,
                            authorizationCode, cardToken, accountId, name, phone, email, os, userAgent))
            {
                throw new Exception("Cannot send payment information to dispatch.");
            }
        }

        public void SendPaymentNotification(double totalAmount, double meterAmount, double tipAmount, string authorizationCode, string vehicleNumber, string companyKey)
        {
            if (companyKey.HasValue())
            {
                _logger.LogMessage(string.Format("Sending payment notification on external company '{0}'", companyKey));
            }

            var amountString = _resources.FormatPrice(totalAmount);
            var meterString = _resources.FormatPrice(meterAmount);
            var tipString = _resources.FormatPrice(tipAmount);

            // Padded with 32 char because the MDT displays line of 32 char.  This will cause to write each string on a new line
            var line1 = string.Format(_resources.Get("PaymentConfirmationToDriver1"));
            line1 = line1.PadRight(32, ' ');
            var line2 = string.Format(_resources.Get("PaymentConfirmationToDriver2"), meterString, tipString);
            line2 = line2.PadRight(32, ' ');
            var line3 = string.Format(_resources.Get("PaymentConfirmationToDriver3"), amountString);
            line3 = line3.PadRight(32, ' ');

            var line4 = string.IsNullOrWhiteSpace(authorizationCode)
                ? string.Empty
                : string.Format(_resources.Get("PaymentConfirmationToDriver4"), authorizationCode);

            if (!_ibsServiceProvider.Booking(companyKey).SendMessageToDriver(line1 + line2 + line3 + line4, vehicleNumber))
            {
                throw new Exception("Cannot send the payment notification.");
            }
        }

        public void SendMessageToDriver(string message, string vehicleNumber, string companyKey)
        {
            if (companyKey.HasValue())
            {
                _logger.LogMessage(string.Format("Sending message to driver on external company '{0}'", companyKey));
            }

            if (!_ibsServiceProvider.Booking(companyKey).SendMessageToDriver(message, vehicleNumber))
            {
                throw new Exception("Cannot send message to driver.");
            }
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

        private BaseAvailableVehicleServiceClient GetAvailableVehiclesServiceClient(string market)
        {
            if (IsCmtGeoServiceMode(market))
            {
                return new CmtGeoServiceClient(_serverSettings, _logger);
            }

            return new HoneyBadgerServiceClient(_serverSettings, _logger);
        }

        private bool IsCmtGeoServiceMode(string market)
        {
            var externalMarketMode = market.HasValue() && _serverSettings.ServerData.ExternalAvailableVehiclesMode == ExternalAvailableVehiclesModes.Geo;

            var internalMarketMode = !market.HasValue() && _serverSettings.ServerData.LocalAvailableVehiclesMode == LocalAvailableVehiclesModes.Geo;

            return internalMarketMode || externalMarketMode;
        }
    }
}