using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using apcurium.MK.Booking.Data;
using apcurium.MK.Booking.Helpers;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Resources;
using CustomerPortal.Client;
using Infrastructure.Messaging;

namespace apcurium.MK.Booking.Services.Impl
{
    public class DispatcherService : IDispatcherService
    {
        private readonly ILogger _logger;
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly IServerSettings _serverSettings;
        private readonly ITaxiHailNetworkServiceClient _taxiHailNetworkServiceClient;
        private readonly TaxiHailNetworkHelper _taxiHailNetworkHelper;
        
        private static readonly Dictionary<string, DispatcherSettingsResponse> DispatcherSettings = new Dictionary<string, DispatcherSettingsResponse>();
        private static readonly Dictionary<Guid, List<Tuple<string, string>>> LegacyVehicleIdMapping = new Dictionary<Guid, List<Tuple<string, string>>>();

        public DispatcherService(
            ILogger logger,
            IIBSServiceProvider ibsServiceProvider,
            IServerSettings serverSettings,
            ICommandBus commandBus,
            ITaxiHailNetworkServiceClient taxiHailNetworkServiceClient)
        {
            _logger = logger;
            _ibsServiceProvider = ibsServiceProvider;
            _serverSettings = serverSettings;
            _taxiHailNetworkServiceClient = taxiHailNetworkServiceClient;

            _taxiHailNetworkHelper = new TaxiHailNetworkHelper(serverSettings, taxiHailNetworkServiceClient, commandBus, _logger);
        }

        public void Dispatch(BestAvailableCompany bestAvailableCompany, DispatcherSettingsResponse dispatcherSettings)
        {
            throw new NotImplementedException();
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
                    ETATime = (int?) vehicle.Eta ?? 0
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
    }
}
