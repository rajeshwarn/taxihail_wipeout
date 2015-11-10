using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Data;
using apcurium.MK.Booking.Helpers;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Jobs;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Resources;
using AutoMapper;
using CMTServices;
using CustomerPortal.Client;
using Infrastructure.Messaging;

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
        private readonly IDispatcherService _dispatcherService;
        private readonly ICommandBus _commandBus;

        public IbsCreateOrderService(IServerSettings serverSettings,
            IVehicleTypeDao vehicleTypeDao,
            IAccountDao accountDao,
            ILogger logger,
            IIBSServiceProvider ibsServiceProvider,
            IUpdateOrderStatusJob updateOrderStatusJob,
            IDispatcherService dispatcherService,
            ICommandBus commandBus)
        {
            _serverSettings = serverSettings;
            _vehicleTypeDao = vehicleTypeDao;
            _accountDao = accountDao;
            _logger = logger;
            _ibsServiceProvider = ibsServiceProvider;
            _updateOrderStatusJob = updateOrderStatusJob;
            _dispatcherService = dispatcherService;
            _commandBus = commandBus;
        }

        public IBSOrderResult CreateIbsOrder(Guid orderId, Address pickupAddress, Address dropOffAddress, string accountNumberString, string customerNumberString,
            string companyKey, int ibsAccountId, string name, string phone, int passengers, int? vehicleTypeId, string ibsInformationNote,
            DateTime pickupDate, string[] prompts, int?[] promptsLength, IList<ListItem> referenceDataCompanyList, string market, int? chargeTypeId,
            int? requestProviderId, Fare fare, double? tipIncentive, bool isHailRequest = false, int? companyFleetId = null)
        {
            if (_serverSettings.ServerData.IBS.FakeOrderStatusUpdate)
            {
                // Fake IBS order id
                return new IBSOrderResult
                {
                    OrderKey = new OrderKey
                    {
                        IbsOrderId = new Random(Guid.NewGuid().GetHashCode()).Next(90000, 90000000),
                        TaxiHailOrderId = orderId
                    }
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
            
            var defaultVehicleType = _vehicleTypeDao.GetAll().FirstOrDefault();
            var defaultVehicleTypeId = defaultVehicleType != null ? defaultVehicleType.ReferenceDataVehicleId : -1;

            var dispatcherSettings = _dispatcherService.GetSettings(market, isHailRequest: isHailRequest);
            IbsResponse orderResult = null;

            // TODO Keep track of vehicles that were already called
            for (int i = 0; i < dispatcherSettings.NumberOfCycles; i++)
            {
                var vehicleCandidates = _dispatcherService.GetVehicleCandidates(
                    orderId,
                    new BestAvailableCompany
                    {
                        CompanyKey = companyKey,
                        FleetId = companyFleetId
                    },
                    dispatcherSettings,
                    pickupAddress.Latitude,
                    pickupAddress.Longitude);

                if (!vehicleCandidates.Any())
                {
                    // Don't query IBS if we don't find any vehicles
                    Thread.Sleep(dispatcherSettings.DurationOfOfferInSeconds);
                    continue;
                }

                var ibsVehicleCandidates = Mapper.Map<IbsVehicleCandidate[]>(vehicleCandidates);

                orderResult = _ibsServiceProvider.Booking(companyKey).CreateOrder(
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
                    tipIncentive,
                    fare,
                    ibsVehicleCandidates);

                // Fetch vehicle candidates (who have accepted the hail request) only if order was successfully created on IBS
                var candidatesResponse = _dispatcherService.WaitForCandidatesResponse(companyKey, orderResult.OrderKey, dispatcherSettings).ToArray();

                if (candidatesResponse.Any())
                {
                    if (isHailRequest)
                    {
                        orderResult.VehicleCandidates = Mapper.Map<IbsVehicleCandidate[]>(candidatesResponse);
                    }
                    else
                    {
                        // TODO: find best vehicle
                        var bestVehicle = Mapper.Map<IbsVehicleCandidate>(candidatesResponse.FirstOrDefault());

                        try
                        {
                            _dispatcherService.AssignJobToVehicle(companyKey, orderResult.OrderKey, bestVehicle);

                            var vehicleMapping = _dispatcherService.GetLegacyVehicleIdMapping()[orderId];

                            _commandBus.Send(new AddVehicleIdMapping
                            {
                                OrderId = orderResult.OrderKey.TaxiHailOrderId,
                                DeviceName = vehicleMapping.Item1,
                                LegacyDispatchId = vehicleMapping.Item2
                            });
                        }
                        catch (Exception)
                        {
                            // Do nothing    
                        }
                    }
                }
            }

            return Mapper.Map<IBSOrderResult>(orderResult);
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
    }
}
