using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Data;
using apcurium.MK.Booking.Helpers;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Resources;
using AutoMapper;
using CustomerPortal.Client;
using CustomerPortal.Contract.Response;
using Infrastructure.Messaging;
using Newtonsoft.Json;

namespace apcurium.MK.Booking.Services.Impl
{
    public class DispatcherService : IDispatcherService
    {
        private readonly ILogger _logger;
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly IServerSettings _serverSettings;
        private readonly ICommandBus _commandBus;
        private readonly ITaxiHailNetworkServiceClient _taxiHailNetworkServiceClient;
        private readonly IAccountDao _accountDao;
        private readonly TaxiHailNetworkHelper _taxiHailNetworkHelper;

        private static readonly Dictionary<Guid, List<Tuple<string, string>>> LegacyVehicleIdMapping = new Dictionary<Guid, List<Tuple<string, string>>>();

        public DispatcherService(
            ILogger logger,
            IIBSServiceProvider ibsServiceProvider,
            IServerSettings serverSettings,
            ICommandBus commandBus,
            ITaxiHailNetworkServiceClient taxiHailNetworkServiceClient,
            IAccountDao accountDao)
        {
            _logger = logger;
            _ibsServiceProvider = ibsServiceProvider;
            _serverSettings = serverSettings;
            _commandBus = commandBus;
            _taxiHailNetworkServiceClient = taxiHailNetworkServiceClient;
            _accountDao = accountDao;

            _taxiHailNetworkHelper = new TaxiHailNetworkHelper(accountDao, ibsServiceProvider, serverSettings, taxiHailNetworkServiceClient, commandBus, _logger);
        }

        public IBSOrderResult Dispatch(Guid accountId, Guid orderId, IbsOrderParams ibsOrderParams, BestAvailableCompany initialBestAvailableCompany,
            DispatcherSettingsResponse dispatcherSettings, string accountNumberString, int initialIbsAccountId, string name, string phone, int passengers,
            int? vehicleTypeId, string ibsInformationNote, DateTime pickupDate, string[] prompts, int?[] promptsLength, string market, Fare fare,
            double? tipIncentive, bool isHailRequest = false, List<string> driverIdsToExclude = null)
        {
            IbsResponse orderResult = null;
            var vehicleAssigned = false;

            var vehicleCandidatesOfferedTheJob = new List<string>();

            if (driverIdsToExclude != null)
            {
                vehicleCandidatesOfferedTheJob.AddRange(driverIdsToExclude);
            }

            initialBestAvailableCompany.FleetId = initialBestAvailableCompany.FleetId ?? _serverSettings.ServerData.CmtGeo.AvailableVehiclesFleetId;

            var bestAvailableCompany = initialBestAvailableCompany;
            var providerId = ibsOrderParams.ProviderId;
            var ibsAccountId = initialIbsAccountId;
            var account = _accountDao.FindById(accountId);

            var availableFleetsInMarket = market.HasValue()
                ? _taxiHailNetworkServiceClient.GetMarketFleets(_serverSettings.ServerData.TaxiHail.ApplicationKey, market).ToArray()
                : _taxiHailNetworkServiceClient.GetNetworkFleet(_serverSettings.ServerData.TaxiHail.ApplicationKey, ibsOrderParams.IbsPickupAddress.Latitude, ibsOrderParams.IbsPickupAddress.Longitude).ToArray();

            _logger.LogMessage("Dispatch called");

            for (var i = 0; i < dispatcherSettings.NumberOfCycles; i++)
            {
                _logger.LogMessage("Starting new dispatch cycle (Cycle #{0}, Company {1}, FleetId: {2})", i, bestAvailableCompany.CompanyKey, bestAvailableCompany.FleetId);

                // Call geo to get a list of available vehicles
                var vehicleCandidates = GetVehicleCandidates(
                    orderId,
                    bestAvailableCompany,
                    dispatcherSettings.Market,
                    ibsOrderParams.IbsPickupAddress.Latitude,
                    ibsOrderParams.IbsPickupAddress.Longitude,
                    availableFleetsInMarket);

                // Filter vehicle list to remove vehicle already sent offer
                vehicleCandidates = FilterOutVehiclesAlreadyOfferedTheJob(vehicleCandidates, vehicleCandidatesOfferedTheJob, dispatcherSettings, true);

                if (!vehicleCandidates.Any())
                {
                    // Don't query IBS if we don't find any vehicles
                    Thread.Sleep(TimeSpan.FromSeconds(dispatcherSettings.DurationOfOfferInSeconds));
                    continue;
                }

                // Call CreateIbsOrder from IbsCreateOrderService
                var ibsVehicleCandidates = Mapper.Map<IbsVehicleCandidate[]>(vehicleCandidates);

                orderResult = _ibsServiceProvider.Booking(bestAvailableCompany.CompanyKey).CreateOrder(
                    orderId,
                    providerId,
                    ibsAccountId,
                    name,
                    phone,
                    passengers,
                    vehicleTypeId,
                    ibsOrderParams.IbsChargeTypeId,
                    ibsInformationNote,
                    pickupDate,
                    ibsOrderParams.IbsPickupAddress,
                    ibsOrderParams.IbsDropOffAddress,
                    accountNumberString,
                    ibsOrderParams.CustomerNumber,
                    prompts,
                    promptsLength,
                    ibsOrderParams.DefaultVehicleTypeId,
                    tipIncentive,
                    account.DefaultTipPercent,
                    dispatcherSettings.DurationOfOfferInSeconds,
                    fare,
                    ibsVehicleCandidates);

                // Fetch vehicle candidates (who have accepted the hail request) only if order was successfully created on IBS
                var candidatesResponse = WaitForCandidatesResponse(bestAvailableCompany.CompanyKey, orderResult.OrderKey, dispatcherSettings).ToArray();

                if (isHailRequest)
                {
                    orderResult.VehicleCandidates = Mapper.Map<IbsVehicleCandidate[]>(candidatesResponse);
                    break;
                }

                if (candidatesResponse.Any())
                {
                    // Pick best vehicle candidate based on ETA
                    var bestVehicle = Mapper.Map<IbsVehicleCandidate>(FindBestVehicleCandidate(candidatesResponse));

                    try
                    {
                        // Update job to Vehicle
                        AssignJobToVehicle(bestAvailableCompany.CompanyKey, orderResult.OrderKey, bestVehicle);

                        vehicleAssigned = true;

                        Tuple<string, string> vehicleMapping = null;

                        // Get proper vehicle id mapping (using device name or ldi) depending on candidate type
                        if (bestVehicle.CandidateType == VehicleCandidateTypes.VctPimId)
                        {
                            vehicleMapping = LegacyVehicleIdMapping[orderId].FirstOrDefault(m => m.Item1 == bestVehicle.VehicleId);
                        }
                        else if (bestVehicle.CandidateType == VehicleCandidateTypes.VctNumber)
                        {
                            vehicleMapping = LegacyVehicleIdMapping[orderId].FirstOrDefault(m => m.Item2 == bestVehicle.VehicleId);
                        }

                        // Send vehicle mapping command
                        _commandBus.Send(new AddOrUpdateVehicleIdMapping
                        {
                            OrderId = orderResult.OrderKey.TaxiHailOrderId,
                            DeviceName = vehicleMapping.Item1,
                            LegacyDispatchId = vehicleMapping.Item2
                        });

                        break;
                    }
                    catch (Exception)
                    {
                        // Do nothing
                    }
                }

                // 9. If nothing found: cancel ibs order + find best available company based on ETA + start over
                CancelIbsOrder(orderResult.OrderKey.IbsOrderId, bestAvailableCompany.CompanyKey, phone, ibsAccountId);

                var dispatcherContext = PrepareForNewDispatchLoop(
                    account,
                    orderId,
                    dispatcherSettings,
                    ibsOrderParams.IbsPickupAddress.Latitude,
                    ibsOrderParams.IbsPickupAddress.Longitude,
                    ibsOrderParams.ProviderId,
                    vehicleCandidatesOfferedTheJob,
                    availableFleetsInMarket);

                if (dispatcherContext != null)
                {
                    bestAvailableCompany = dispatcherContext.BestAvailableCompany;
                    providerId = dispatcherContext.ProviderId;
                    ibsAccountId = dispatcherContext.IbsAccountId;
                }
            }

            if (!vehicleAssigned)
            {
                // Order couldn't be assigned to any vehicles
                return new IBSOrderResult
                {
                    OrderKey = new OrderKey { IbsOrderId = -1, TaxiHailOrderId = orderId },
                    DispatcherTimedOut = true
                };
            }

            var result = Mapper.Map<IBSOrderResult>(orderResult
                ?? new IbsResponse
                {
                    OrderKey = new IbsOrderKey { IbsOrderId = -1, TaxiHailOrderId = orderId }
                });

            result.CompanyKey = bestAvailableCompany.CompanyKey;

            return result;
        }

        public void CancelIbsOrder(int? ibsOrderId, string companyKey, string phone, int ibsAccountId)
        {
            // Cancel order on current company IBS
            if (ibsOrderId.HasValue)
            {
                _logger.LogMessage(string.Format("Cancelling IBSOrder {0}, on company {1}", ibsOrderId, companyKey));

                // We need to try many times because sometime the IBS cancel method doesn't return an error but doesn't cancel the ride...
                // After 5 time, we are giving up. But we assume the order is completed.
                Task.Factory.StartNew(() =>
                {
                    Func<bool> cancelOrder = () => _ibsServiceProvider.Booking(companyKey)
                        .CancelOrder(ibsOrderId.Value, ibsAccountId, phone);

                    cancelOrder.Retry(new TimeSpan(0, 0, 0, 10), 5);
                });
            }
        }

        private DispatcherContext PrepareForNewDispatchLoop(
            AccountDetail account,
            Guid orderId,
            DispatcherSettingsResponse dispatcherSettings,
            double pickupLatitude,
            double pickupLongitude,
            int? homeMarketProviderId,
            List<string> vehicleCandidatesOfferedTheJob,
            NetworkFleetResponse[] availableFleetsInMarket)
        {
            var vehicleCandidates = GetVehicleCandidates(
                    orderId,
                    null,
                    dispatcherSettings.Market,
                    pickupLatitude,
                    pickupLongitude,
                    availableFleetsInMarket);

            _logger.LogMessage("Preparing for next cycle, vehicles of market received by geo: {0}", JsonConvert.SerializeObject(vehicleCandidates));

            // Filter vehicle list to remove vehicle already sent offer
            vehicleCandidates = FilterOutVehiclesAlreadyOfferedTheJob(vehicleCandidates, vehicleCandidatesOfferedTheJob, dispatcherSettings, false);

            _logger.LogMessage("Preparing for next cycle, vehicles of market received by geo filtered: {0}", JsonConvert.SerializeObject(vehicleCandidates));

            var newBestAvailableVehicle = vehicleCandidates.FirstOrDefault();
            if (newBestAvailableVehicle == null)
            {
                return null;
            }

            var newBestFleet = availableFleetsInMarket.FirstOrDefault(fleet => fleet.FleetId == newBestAvailableVehicle.FleetId);
            if (newBestFleet == null)
            {
                return null;
            }

            var referenceDataCompanyList = _ibsServiceProvider.StaticData(newBestFleet.CompanyKey).GetCompaniesList();

            var defaultCompany = referenceDataCompanyList.FirstOrDefault(x => x.IsDefault.HasValue && x.IsDefault.Value)
                    ?? referenceDataCompanyList.FirstOrDefault();

            var providerId = dispatcherSettings.Market.HasValue() && referenceDataCompanyList.Any() && defaultCompany != null
                    ? defaultCompany.Id
                    : homeMarketProviderId;

            return new DispatcherContext
            {
                IbsAccountId = _taxiHailNetworkHelper.CreateIbsAccountIfNeeded(account, newBestFleet.CompanyKey),
                ProviderId = providerId,
                BestAvailableCompany = new BestAvailableCompany
                {
                    CompanyKey = newBestFleet.CompanyKey,
                    CompanyName = newBestFleet.CompanyName,
                    FleetId = newBestFleet.FleetId
                }
            };
        }
        
        private VehicleCandidate FindBestVehicleCandidate(IEnumerable<VehicleCandidate> vehicleCandidates)
        {
            // Order by ETA
            return vehicleCandidates.OrderBy(vehicle => vehicle.ETATime).First();
        }

        public void AssignJobToVehicle(string companyKey, IbsOrderKey ibsOrderKey, IbsVehicleCandidate ibsVehicleCandidate)
        {
            if (ibsOrderKey.IbsOrderId < 0)
            {
                throw new Exception(string.Format("Cannot confirm hail because IBS returned error code {0}", ibsOrderKey.IbsOrderId));
            }

            if (ibsVehicleCandidate == null)
            {
                throw new Exception("You need to specify a vehicle in order to confirm the hail.");
            }

            var confirmResult = _ibsServiceProvider.Booking(companyKey).ConfirmHail(ibsOrderKey, ibsVehicleCandidate);
            if (confirmResult == null || confirmResult < 0)
            {
                var errorMessage = string.Format("Error while trying to confirm the hail. IBS response code : {0}", confirmResult);
                _logger.LogMessage(errorMessage);

                throw new Exception(errorMessage);
            }

            _logger.LogMessage("Hail request confirmed");
        }

        private IEnumerable<VehicleCandidate> GetVehicleCandidates(Guid orderId, BestAvailableCompany bestAvailableCompany, string market, double pickupLatitude, double pickupLongitude, NetworkFleetResponse[] availableFleetsInMarket)
        {
            int[] fleetIds = null;
            if (bestAvailableCompany != null)
            {
                if (bestAvailableCompany.CompanyKey.HasValue() && !bestAvailableCompany.FleetId.HasValue)
                {
                    return new VehicleCandidate[0];
                }

                if (bestAvailableCompany.FleetId.HasValue)
                {
                    fleetIds = new[] {bestAvailableCompany.FleetId.Value};
                }
            }
            else
            {
                fleetIds = availableFleetsInMarket.Select(x => x.FleetId).Distinct().ToArray();
            }
            
            var availableVehicleService = _taxiHailNetworkHelper.GetAvailableVehiclesServiceClient(market);

            // Query only the avaiable vehicles from the selected company for the order
            var availableVehicles = availableVehicleService.GetAvailableVehicles(
                market ?? _serverSettings.ServerData.CmtGeo.AvailableVehiclesMarket,
                pickupLatitude,
                pickupLongitude,
                fleetIds: fleetIds,
                returnAll: true)
                .OrderBy(v => v.Eta.HasValue)
                .ThenBy(v => v.Eta)
                .ToArray();

            LegacyVehicleIdMapping[orderId] = new List<Tuple<string, string>>();

            return availableVehicles.Select(vehicle =>
            {
                LegacyVehicleIdMapping[orderId].Add(Tuple.Create(vehicle.DeviceName, vehicle.LegacyDispatchId));

                return new VehicleCandidate
                {
                    CandidateType =
                        vehicle.LegacyDispatchId.HasValue()
                            ? VehicleCandidateTypes.VctNumber
                            : VehicleCandidateTypes.VctPimId,
                    VehicleId = vehicle.LegacyDispatchId.HasValue() ? vehicle.LegacyDispatchId : vehicle.DeviceName,
                    ETADistance = (int?) vehicle.DistanceToArrival ?? 0,
                    ETATime = (int?) vehicle.Eta ?? 0,
                    FleetId = vehicle.FleetId
                };
            });
        }

        private IEnumerable<VehicleCandidate> WaitForCandidatesResponse(string companyKey, IbsOrderKey ibsOrderKey, DispatcherSettingsResponse dispatcherSettings)
        {
            if (ibsOrderKey.IbsOrderId < 0)
            {
                return new VehicleCandidate[0];
            }

            // Need to wait for vehicles to receive hail request (5 seconds of padding)
            Thread.Sleep(TimeSpan.FromSeconds(dispatcherSettings.DurationOfOfferInSeconds + 5));

            var candidates = _ibsServiceProvider.Booking(companyKey).GetCandidatesResponse(ibsOrderKey);
            return candidates.Select(vehicle => new VehicleCandidate
            {
                CandidateType = vehicle.CandidateType,
                ETADistance = vehicle.ETADistance,
                ETATime = vehicle.ETATime,
                Rating = vehicle.Rating,
                VehicleId = vehicle.VehicleId.Trim() // Need to trim because IBS can return us junk trailling or leading spaces in that field...
            });
        }

        public DispatcherSettingsResponse GetSettings(string market, double latitude, double longitude, bool isHailRequest = false)
        {
            if (isHailRequest)
            {
                return GetHailDispatcherSettings(market);
            }

            return GetSettings(latitude, longitude);
        }

        private DispatcherSettingsResponse GetSettings(double latitude, double longitude)
        {
            var response = _taxiHailNetworkServiceClient.GetCompanyMarketSettings(latitude, longitude);

            return new DispatcherSettingsResponse
            {
                Market = response.Market,
                DurationOfOfferInSeconds = response.DispatcherSettings.DurationOfOfferInSeconds,
                NumberOfCycles = response.DispatcherSettings.NumberOfCycles,
                NumberOfOffersPerCycle = response.DispatcherSettings.NumberOfOffersPerCycle
            };
        }

        private DispatcherSettingsResponse GetHailDispatcherSettings(string market)
        {
            return new DispatcherSettingsResponse
            {
                NumberOfOffersPerCycle = 12,
                NumberOfCycles = 1,
                DurationOfOfferInSeconds = 30,
                Market = market
            };
        }

        private IEnumerable<VehicleCandidate> FilterOutVehiclesAlreadyOfferedTheJob(
            IEnumerable<VehicleCandidate> vehicleCandidates,
            List<string> vehicleCandidatesOfferedTheJob,
            DispatcherSettingsResponse dispatcherSettings,
            bool addToFilteredVehicleList)
        {
            var filteredList = vehicleCandidates
                .Where(vehicleCandidate => !vehicleCandidatesOfferedTheJob.Exists(excludedVehicleId => vehicleCandidate.VehicleId == excludedVehicleId))
                .Take(dispatcherSettings.NumberOfOffersPerCycle)
                .ToList();

            if (addToFilteredVehicleList)
            {
                vehicleCandidatesOfferedTheJob.AddRange(filteredList.Select(v => v.VehicleId));
            }

            return filteredList;
        }

        private class DispatcherContext
        {
            public int IbsAccountId { get; set; }
            public int? ProviderId { get; set; }
            public BestAvailableCompany BestAvailableCompany { get; set; }
        }
    }
}
