using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Data;
using apcurium.MK.Booking.Helpers;
using apcurium.MK.Booking.IBS;
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
        
        private static readonly Dictionary<string, DispatcherSettingsResponse> DispatcherSettings = new Dictionary<string, DispatcherSettingsResponse>();
        private static readonly Dictionary<Guid, List<Tuple<string, string>>> LegacyVehicleIdMapping = new Dictionary<Guid, List<Tuple<string, string>>>();

        private BestAvailableCompany _bestAvailableCompany;
        private int? _providerId;
        private int _ibsAccountId;

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

        public IBSOrderResult Dispatch(Guid accountId, Guid orderId, BestAvailableCompany initialBestAvailableCompany, DispatcherSettingsResponse dispatcherSettings,
            IbsAddress pickupAddress, IbsAddress dropOffAddress, string accountNumberString, int? customerNumber,
            int initialIbsAccountId, string name, string phone, int passengers, int vehicleTypeId, string ibsInformationNote,
            DateTime pickupDate, string[] prompts, int?[] promptsLength, IList<ListItem> initialReferenceDataCompanyList, string market, int? chargeTypeId,
            int? initialProviderId, int? homeMarketProviderId, Fare fare, double? tipIncentive, bool isHailRequest = false)
        {
            IbsResponse orderResult = null;

            var vehicleCandidatesOfferedTheJob = new List<VehicleCandidate>();

            initialBestAvailableCompany.FleetId = initialBestAvailableCompany.FleetId ?? _serverSettings.ServerData.CmtGeo.AvailableVehiclesFleetId;

            _bestAvailableCompany = initialBestAvailableCompany;
            _providerId = initialProviderId;
            _ibsAccountId = initialIbsAccountId;

            var availableFleetsInMarket = market.HasValue()
                ? _taxiHailNetworkServiceClient.GetMarketFleets(_serverSettings.ServerData.TaxiHail.ApplicationKey, market).ToArray()
                : _taxiHailNetworkServiceClient.GetNetworkFleet(_serverSettings.ServerData.TaxiHail.ApplicationKey, pickupAddress.Latitude, pickupAddress.Longitude).ToArray();
            
            for (var i = 0; i < dispatcherSettings.NumberOfCycles; i++)
            {
                // 1. Call geo
                var vehicleCandidates = GetVehicleCandidates(
                    orderId,
                    _bestAvailableCompany,
                    dispatcherSettings,
                    pickupAddress.Latitude,
                    pickupAddress.Longitude);

                // 2. Filter vehicle list to remove vehicle already sent offer
                vehicleCandidates = FilterOutVehiclesAlreadyOfferedTheJob(vehicleCandidates, vehicleCandidatesOfferedTheJob, dispatcherSettings, true);

                if (!vehicleCandidates.Any())
                {
                    // Don't query IBS if we don't find any vehicles
                    Thread.Sleep(TimeSpan.FromSeconds(dispatcherSettings.DurationOfOfferInSeconds));
                    continue;
                }

                // 3. Call CreateIbsOrder from IbsCreateOrderService
                var ibsVehicleCandidates = Mapper.Map<IbsVehicleCandidate[]>(vehicleCandidates);

                orderResult = _ibsServiceProvider.Booking(_bestAvailableCompany.CompanyKey).CreateOrder(
                    orderId,
                    _providerId,
                    initialIbsAccountId,
                    name,
                    phone,
                    passengers,
                    vehicleTypeId,
                    chargeTypeId,
                    ibsInformationNote,
                    pickupDate,
                    pickupAddress,
                    dropOffAddress,
                    accountNumberString,
                    customerNumber,
                    prompts,
                    promptsLength,
                    vehicleTypeId,
                    tipIncentive,
                    fare,
                    ibsVehicleCandidates);

                // 4. Wait D time + padding
                // 5. GetVehicle candidates
                // Fetch vehicle candidates (who have accepted the hail request) only if order was successfully created on IBS
                var candidatesResponse = WaitForCandidatesResponse(_bestAvailableCompany.CompanyKey, orderResult.OrderKey, dispatcherSettings).ToArray();

                if (isHailRequest)
                {
                    orderResult.VehicleCandidates = Mapper.Map<IbsVehicleCandidate[]>(candidatesResponse);
                    break;
                }

                if (candidatesResponse.Any())
                {
                    // 6. Pick best vehicle candidate based on ETA
                    var bestVehicle = Mapper.Map<IbsVehicleCandidate>(FindBestVehicleCandidate(candidatesResponse));

                    try
                    {
                        // 7. Update job to Vehicle
                        AssignJobToVehicle(_bestAvailableCompany.CompanyKey, orderResult.OrderKey, bestVehicle);

                        Tuple<string, string> vehicleMapping = null;
                        if (bestVehicle.CandidateType == VehicleCandidateTypes.VctPimId)
                        {
                            vehicleMapping =
                                GetLegacyVehicleIdMapping()[orderId].FirstOrDefault(
                                    m => m.Item1 == bestVehicle.VehicleId);
                        }
                        else if (bestVehicle.CandidateType == VehicleCandidateTypes.VctNumber)
                        {
                            vehicleMapping =
                                GetLegacyVehicleIdMapping()[orderId].FirstOrDefault(
                                    m => m.Item2 == bestVehicle.VehicleId);
                        }

                        // 8. Send vehicle mapping command
                        _commandBus.Send(new AddVehicleIdMapping
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
                CancelIbsOrder(orderResult.OrderKey.IbsOrderId, _bestAvailableCompany.CompanyKey, phone, _ibsAccountId);

                PrepareForNewLoop(
                    accountId,
                    orderId,
                    dispatcherSettings,
                    pickupAddress.Latitude,
                    pickupAddress.Longitude,
                    homeMarketProviderId,
                    vehicleCandidatesOfferedTheJob,
                    availableFleetsInMarket);
            }

            return Mapper.Map<IBSOrderResult>(orderResult
                ?? new IbsResponse
                {
                    OrderKey = new IbsOrderKey { IbsOrderId = -1, TaxiHailOrderId = orderId }
                });
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

        private void PrepareForNewLoop(
            Guid accountId,
            Guid orderId,
            DispatcherSettingsResponse dispatcherSettings,
            double pickupLatitude,
            double pickupLongitude,
            int? homeMarketProviderId,
            List<VehicleCandidate> vehicleCandidatesOfferedTheJob,
            NetworkFleetResponse[] availableFleetsInMarket)
        {
            var vehicleCandidates = GetVehicleCandidates(
                    orderId,
                    _bestAvailableCompany,
                    dispatcherSettings,
                    pickupLatitude,
                    pickupLongitude);

            // 2. Filter vehicle list to remove vehicle already sent offer
            vehicleCandidates = FilterOutVehiclesAlreadyOfferedTheJob(vehicleCandidates, vehicleCandidatesOfferedTheJob, dispatcherSettings, false);

            var newBestAvailableVehicle = vehicleCandidates.FirstOrDefault();
            if (newBestAvailableVehicle == null)
            {
                return;
            }

            var newBestFleet = availableFleetsInMarket.FirstOrDefault(fleet => fleet.FleetId == newBestAvailableVehicle.FleetId);
            if (newBestFleet == null)
            {
                return;
            }

            var referenceDataCompanyList = _ibsServiceProvider.StaticData(newBestFleet.CompanyKey).GetCompaniesList();

            var defaultCompany = referenceDataCompanyList.FirstOrDefault(x => x.IsDefault.HasValue && x.IsDefault.Value)
                    ?? referenceDataCompanyList.FirstOrDefault();

            var providerId = dispatcherSettings.Market.HasValue() && referenceDataCompanyList.Any() && defaultCompany != null
                    ? defaultCompany.Id
                    : homeMarketProviderId;

            var accountDetail = _accountDao.FindById(accountId);
            _ibsAccountId = _taxiHailNetworkHelper.CreateIbsAccountIfNeeded(accountDetail, newBestFleet.CompanyKey);
            _providerId = providerId;

            _bestAvailableCompany = new BestAvailableCompany
            {
                CompanyKey = newBestFleet.CompanyKey,
                CompanyName = newBestFleet.CompanyName,
                FleetId = newBestFleet.FleetId
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

        public IEnumerable<VehicleCandidate> GetVehicleCandidates(Guid orderId, BestAvailableCompany bestAvailableCompany, DispatcherSettingsResponse dispatcherSettings, double pickupLatitude, double pickupLongitude)
        {
            if (bestAvailableCompany.CompanyKey.HasValue() && !bestAvailableCompany.FleetId.HasValue)
            {
                return new VehicleCandidate[0];
            }

            int[] fleetIds = null;
            if (bestAvailableCompany.FleetId.HasValue)
            {
                fleetIds = new[] { bestAvailableCompany.FleetId.Value };
            }

            var availableVehicleService = _taxiHailNetworkHelper.GetAvailableVehiclesServiceClient(dispatcherSettings.Market);

            // Query only the avaiable vehicles from the selected company for the order
            var availableVehicles = availableVehicleService.GetAvailableVehicles(
                dispatcherSettings.Market ?? _serverSettings.ServerData.CmtGeo.AvailableVehiclesMarket,
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

        public IEnumerable<VehicleCandidate> WaitForCandidatesResponse(string companyKey, IbsOrderKey ibsOrderKey, DispatcherSettingsResponse dispatcherSettings)
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
                VehicleId = vehicle.VehicleId
            });
        }

        public DispatcherSettingsResponse GetSettings(string market, double? latitude = null, double? longitude = null, bool isHailRequest = false)
        {

            if (isHailRequest)
            {
                return GetHailDispatcherSettings(market);
            }

            market = market ?? string.Empty;

            if (!DispatcherSettings.ContainsKey(market))
            {
                if (latitude.HasValue && longitude.HasValue)
                {
                    return GetSettings(latitude.Value, longitude.Value);
                }

                return null;
            }

            return DispatcherSettings[market];
        }

        public DispatcherSettingsResponse GetSettings(double latitude, double longitude, bool isHailRequest = false)
        {
            var response = _taxiHailNetworkServiceClient.GetCompanyMarketSettings(latitude, longitude);

            if (isHailRequest)
            {
                return GetHailDispatcherSettings(response.Market);
            }

            var dispatcherSettings = new DispatcherSettingsResponse
            {
                Market = response.Market,
                DurationOfOfferInSeconds = response.DispatcherSettings.DurationOfOfferInSeconds,
                NumberOfCycles = response.DispatcherSettings.NumberOfCycles,
                NumberOfOffersPerCycle = response.DispatcherSettings.NumberOfOffersPerCycle
            };

            DispatcherSettings[response.Market ?? string.Empty] = dispatcherSettings;

            return dispatcherSettings;
        }

        public Dictionary<Guid, List<Tuple<string, string>>> GetLegacyVehicleIdMapping()
        {
            return LegacyVehicleIdMapping;
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
            List<VehicleCandidate> vehicleCandidatesOfferedTheJob,
            DispatcherSettingsResponse dispatcherSettings,
            bool addToFilteredVehicleList)
        {
            var filteredList = vehicleCandidates
                .Where(vehicleCandidate => !vehicleCandidatesOfferedTheJob.Exists(x => x.VehicleId == vehicleCandidate.VehicleId))
                .Take(dispatcherSettings.NumberOfOffersPerCycle)
                .ToList();

            if (addToFilteredVehicleList)
            {
                vehicleCandidatesOfferedTheJob.AddRange(filteredList);
            }

            return filteredList;
        }
    }
}
