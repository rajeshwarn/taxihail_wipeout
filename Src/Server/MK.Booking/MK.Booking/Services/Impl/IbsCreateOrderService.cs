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

namespace apcurium.MK.Booking.Services.Impl
{
    public class IbsCreateOrderService : IIbsCreateOrderService
    {
        private readonly IServerSettings _serverSettings;
        private readonly IVehicleTypeDao _vehicleTypeDao;
        private readonly IAccountDao _accountDao;
        private readonly ILogger _logger;
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly IUpdateOrderStatusJob _updateOrderStatusJob;

        public IbsCreateOrderService(IServerSettings serverSettings,
            IVehicleTypeDao vehicleTypeDao,
            IAccountDao accountDao,
            ILogger logger,
            IIBSServiceProvider ibsServiceProvider,
            IUpdateOrderStatusJob updateOrderStatusJob)
        {
            _serverSettings = serverSettings;
            _vehicleTypeDao = vehicleTypeDao;
            _accountDao = accountDao;
            _logger = logger;
            _ibsServiceProvider = ibsServiceProvider;
            _updateOrderStatusJob = updateOrderStatusJob;
        }

        public IBSOrderResult CreateIbsOrder(Guid orderId, Address pickupAddress, Address dropOffAddress, string accountNumberString, string customerNumberString,
            string companyKey, int ibsAccountId, string name, string phone, string email, int passengers, int? vehicleTypeId, string ibsInformationNote, bool isFutureBooking,
            DateTime pickupDate, string[] prompts, int?[] promptsLength, IList<ListItem> referenceDataCompanyList, string market, int? chargeTypeId,
            int? requestProviderId, Fare fare, double? tipIncentive, int? tipPercent, bool isHailRequest = false, int? companyFleetId = null)
        {
            if (_serverSettings.ServerData.IBS.FakeOrderStatusUpdate)
            {
                // Wait 15 seconds to reproduce what happens in real life with the "Finding you a taxi"
                Thread.Sleep(TimeSpan.FromSeconds(15));

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

            //if we are in external market or local market but in a different company
            var providerId = (market.HasValue() || companyKey.HasValue())
                && referenceDataCompanyList.Any() && defaultCompany != null
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
                ibsHailResult = Hail(orderId, providerId, market, companyKey, companyFleetId, pickupAddress, ibsAccountId, name, phone,
                    email, passengers, vehicleTypeId, ibsChargeTypeId, ibsInformationNote, pickupDate, ibsPickupAddress,
                    ibsDropOffAddress, accountNumberString, customerNumber, prompts, promptsLength, defaultVehicleTypeId,
                    tipIncentive, tipPercent, fare);
            }
            else
            {
                createOrderResult = _ibsServiceProvider.Booking(companyKey).CreateOrder(
                    providerId,
                    ibsAccountId,
                    name,
                    phone,
                    email,
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
                    tipPercent,
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
            if (_serverSettings.ServerData.IBS.FakeOrderStatusUpdate)
            {
                return;
            }

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

        private IbsHailResponse Hail(Guid orderId, int? providerId, string market, string companyKey, int? companyFleetId, Address pickupAddress, int ibsAccountId,
            string name, string phone, string email, int passengers, int? vehicleTypeId, int? ibsChargeTypeId, string ibsInformationNote, DateTime pickupDate, IbsAddress ibsPickupAddress,
            IbsAddress ibsDropOffAddress, string accountNumberString, int? customerNumber, string[] prompts, int?[] promptsLength, int defaultVehicleTypeId, double? tipIncentive, int? tipPercent, Fare fare)
        {
            // Query only the avaiable vehicles from the selected company for the order
            var availableVehicleService = GetAvailableVehiclesServiceClient(market);
            var availableVehicles = availableVehicleService.GetAvailableVehicles(market, pickupAddress.Latitude, pickupAddress.Longitude)
                .Where(v => v.FleetId == companyFleetId).ToArray();

            if (!availableVehicles.Any())
            {
                // Don't query IBS if we don't find any vehicles
                return new IbsHailResponse
                {
                    OrderKey = new IbsOrderKey { IbsOrderId = -1, TaxiHailOrderId = orderId }
                };
            }

            var vehicleCandidates = availableVehicles.Select(vehicle => new IbsVehicleCandidate
            {
                CandidateType = VehicleCandidateTypes.VctPimId,
                VehicleId = vehicle.DeviceName,
                ETADistance = (int?)vehicle.DistanceToArrival ?? 0,
                ETATime = (int?)vehicle.Eta ?? 0
            });

            var ibsHailResult = _ibsServiceProvider.Booking(companyKey).Hail(
                orderId,
                providerId,
                ibsAccountId,
                name,
                phone,
                email,
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
                tipPercent,
                fare);

            // Fetch vehicle candidates (who have accepted the hail request) only if order was successfully created on IBS
            if (ibsHailResult.OrderKey.IbsOrderId > -1)
            {
                // Need to wait for vehicles to receive hail request
                Thread.Sleep(30000);

                var candidates = _ibsServiceProvider.Booking(companyKey).GetVehicleCandidates(ibsHailResult.OrderKey);
                ibsHailResult.VehicleCandidates = candidates;
            }

            return ibsHailResult;
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
