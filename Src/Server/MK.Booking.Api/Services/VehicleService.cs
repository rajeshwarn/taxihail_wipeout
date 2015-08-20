#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using AutoMapper;
using CustomerPortal.Client;
using CustomerPortal.Contract.Resources;
using CustomerPortal.Contract.Response;
using HoneyBadger;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class VehicleService : Service
    {
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly IVehicleTypeDao _dao;
        private readonly ICommandBus _commandBus;
        private readonly ReferenceDataService _referenceDataService;
        private readonly HoneyBadgerServiceClient _honeyBadgerServiceClient;
        private readonly ITaxiHailNetworkServiceClient _taxiHailNetworkServiceClient;
        private readonly IServerSettings _serverSettings;
        private readonly ILogger _logger;

        public VehicleService(IIBSServiceProvider ibsServiceProvider,
            IVehicleTypeDao dao,
            ICommandBus commandBus,
            ReferenceDataService referenceDataService,
            HoneyBadgerServiceClient honeyBadgerServiceClient,
            ITaxiHailNetworkServiceClient taxiHailNetworkServiceClient,
            IServerSettings serverSettings,
            ILogger logger)
        {
            _ibsServiceProvider = ibsServiceProvider;
            _dao = dao;
            _commandBus = commandBus;
            _referenceDataService = referenceDataService;
            _honeyBadgerServiceClient = honeyBadgerServiceClient;
            _taxiHailNetworkServiceClient = taxiHailNetworkServiceClient;
            _serverSettings = serverSettings;
            _logger = logger;
        }

        public AvailableVehiclesResponse Post(AvailableVehicles request)
        {
            var vehicleType = _dao.GetAll().FirstOrDefault(v => v.ReferenceDataVehicleId == request.VehicleTypeId);
            string logoName = vehicleType != null ? vehicleType.LogoName : null;

            IbsVehiclePosition[] vehicles;
            string market = null;

            try
            {
                market = _taxiHailNetworkServiceClient.GetCompanyMarket(request.Latitude, request.Longitude);
            }
            catch
            {
                // Do nothing. If we fail to contact Customer Portal, we continue as if we are in a local market.
                _logger.LogMessage("VehicleService: Error while trying to get company Market.");
            }
            
            if (!market.HasValue()
                && _serverSettings.ServerData.AvailableVehiclesMode == AvailableVehiclesModes.IBS)
            {
                // LOCAL market IBS
                vehicles = _ibsServiceProvider.Booking().GetAvailableVehicles(request.Latitude, request.Longitude, request.VehicleTypeId);
            }
            else
            {
                string availableVehiclesMarket;
                IList<int> availableVehiclesFleetIds = null;

                if (!market.HasValue()
                    && _serverSettings.ServerData.AvailableVehiclesMode == AvailableVehiclesModes.HoneyBadger)
                {
                    // LOCAL market Honey Badger
                    availableVehiclesMarket = _serverSettings.ServerData.HoneyBadger.AvailableVehiclesMarket;

                    if (request.FleetIds != null)
                    {
                        // Use fleet ids specified in the request first
                        availableVehiclesFleetIds = request.FleetIds;
                    }
                    else if (_serverSettings.ServerData.HoneyBadger.AvailableVehiclesFleetId.HasValue)
                    {
                        // Or fleet id specified in the settings
                        availableVehiclesFleetIds = new[] { _serverSettings.ServerData.HoneyBadger.AvailableVehiclesFleetId.Value };
                    }
                }
                else
                {
                    // EXTERNAL market Honey Badger
                    availableVehiclesMarket = market;

                    try
                    {
                        // Only get available vehicles for dispatchable companies in market
                        var roamingCompanies = _taxiHailNetworkServiceClient.GetMarketFleets(_serverSettings.ServerData.TaxiHail.ApplicationKey, market);
                        if (roamingCompanies != null)
                        {
                            var roamingFleetIds = roamingCompanies.Select(r => r.FleetId);

                            if (request.FleetIds != null)
                            {
                                // From the fleets accessible by that company, only return vehicles from the fleets specified in the request
                                availableVehiclesFleetIds = roamingFleetIds
                                    .Where(fleetId => request.FleetIds.Contains(fleetId))
                                    .ToArray();
                            }
                            else
                            {
                                // Return vehicles from all fleets accessible by that company
                                availableVehiclesFleetIds = roamingFleetIds.ToArray();
                            }
                        }
                        else
                        {
                            availableVehiclesFleetIds = request.FleetIds;
                        }
                    }
                    catch
                    {
                        // Do nothing. If we fail to contact Customer Portal, we return an unfiltered list of available vehicles.
                        _logger.LogMessage("VehicleService: Error while trying to get Market fleets.");
                    }
                }

                var vehicleResponse = _honeyBadgerServiceClient.GetAvailableVehicles(
                    availableVehiclesMarket,
                    request.Latitude,
                    request.Longitude,
                    request.SearchRadius,
                    availableVehiclesFleetIds);

                vehicles = vehicleResponse.Select(v => new IbsVehiclePosition
                {
                    Latitude = v.Latitude,
                    Longitude = v.Longitude,
                    PositionDate = v.Timestamp,
                    VehicleNumber = v.Medallion,
                    FleetId = v.FleetId,
                    VehicleType = v.VehicleType
                }).ToArray();
            }

            var availableVehicles = vehicles.Select(Mapper.Map<AvailableVehicle>).ToArray();
                
            foreach (var vehicle in availableVehicles)
            {
                vehicle.LogoName = logoName;
            }
            return new AvailableVehiclesResponse(availableVehicles);   
        }

        public object Get(VehicleTypeRequest request)
        {
            if (request.Id == Guid.Empty)
            {
                return _dao.GetAll();
            }

            var vehicleType = _dao.FindById(request.Id);
            if (vehicleType == null)
            {
                throw new HttpError(HttpStatusCode.NotFound, "Vehicle Type Not Found");
            }
            
            return vehicleType;
        }

        public object Post(VehicleTypeRequest request)
        {
            var command = new AddUpdateVehicleType
            {
                VehicleTypeId = Guid.NewGuid(),
                Name = request.Name,
                LogoName = request.LogoName,
                ReferenceDataVehicleId = request.ReferenceDataVehicleId,
                CompanyId = AppConstants.CompanyId,
                MaxNumberPassengers = request.MaxNumberPassengers,
                ReferenceNetworkVehicleTypeId = request.ReferenceNetworkVehicleTypeId
            };

            if (_serverSettings.ServerData.Network.Enabled)
            {
                _taxiHailNetworkServiceClient.UpdateMarketVehicleType(_serverSettings.ServerData.TaxiHail.ApplicationKey,
                    new CompanyVehicleType
                    {
                        Id = command.VehicleTypeId,
                        LogoName = command.LogoName,
                        MaxNumberPassengers = command.MaxNumberPassengers,
                        Name = command.Name,
                        ReferenceDataVehicleId = command.ReferenceDataVehicleId,
                        NetworkVehicleId = command.ReferenceNetworkVehicleTypeId
                    })
                    .HandleErrors();
            }

            _commandBus.Send(command);

            return new
            {
                Id = command.VehicleTypeId
            };
        }


        public object Put(VehicleTypeRequest request)
        {
            var existing = _dao.FindById(request.Id);
            if (existing == null)
            {
                throw new HttpError(HttpStatusCode.NotFound, "Vehicle Type Not Found");
            }

            var command = new AddUpdateVehicleType
            {
                VehicleTypeId = request.Id,
                Name = request.Name,
                LogoName = request.LogoName,
                ReferenceDataVehicleId = request.ReferenceDataVehicleId,
                CompanyId = AppConstants.CompanyId,
                MaxNumberPassengers = request.MaxNumberPassengers,
                ReferenceNetworkVehicleTypeId = request.ReferenceNetworkVehicleTypeId
            };

            _commandBus.Send(command);

            if (_serverSettings.ServerData.Network.Enabled)
            {
                _taxiHailNetworkServiceClient.UpdateMarketVehicleType(_serverSettings.ServerData.TaxiHail.ApplicationKey,
                        new CompanyVehicleType
                        {
                            Id = command.VehicleTypeId,
                            LogoName = command.LogoName,
                            MaxNumberPassengers = command.MaxNumberPassengers,
                            Name = command.Name,
                            ReferenceDataVehicleId = command.ReferenceDataVehicleId,
                            NetworkVehicleId = command.ReferenceNetworkVehicleTypeId
                        })
                        .HandleErrors();    
            }

            return new
            {
                Id = command.VehicleTypeId
            };
        }

        public object Delete(VehicleTypeRequest request)
        {
            var existing = _dao.FindById(request.Id);
            if (existing == null)
            {
                throw new HttpError(HttpStatusCode.NotFound, "Vehicle Type Not Found");
            }

            var command = new DeleteVehicleType
            {
                VehicleTypeId = existing.Id,
                CompanyId = AppConstants.CompanyId
            };

            _commandBus.Send(command);

            if (_serverSettings.ServerData.Network.Enabled)
            {
                _taxiHailNetworkServiceClient.DeleteMarketVehicleMapping(_serverSettings.ServerData.TaxiHail.ApplicationKey, request.Id)
                    .HandleErrors();
            }
            
            return new HttpResult(HttpStatusCode.OK, "OK");
        }

        public object Get(UnassignedNetworkVehicleTypeRequest request)
        {
            try
            {
                // We fetch the currently assigned networkVehicleTypeIds.
                var allAssigned = _dao.GetAll()
                    .Select(x => x.ReferenceNetworkVehicleTypeId)
                    .Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .ToArray();

                //We remove from consideration the current vehicle type id.
                if (request.NetworkVehicleId.HasValue)
                {
                    allAssigned = allAssigned.Where(x => x != request.NetworkVehicleId.Value).ToArray();
                }

                var networkVehicleType = _taxiHailNetworkServiceClient.GetMarketVehicleTypes(_serverSettings.ServerData.TaxiHail.ApplicationKey);

                //We filter out every market vehicle type that are currently in use.
                return networkVehicleType
                    .Where(x => !allAssigned.Any(id => id == x.ReferenceDataVehicleId))
                    .Select(x => new
                    {
                        Id = x.ReferenceDataVehicleId,
                        Name = x.Name,
                        MaxNumberPassengers = x.MaxNumberPassengers
                    })
                    .ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);

                return new object[0];
            }   
        }

        public object Get(UnassignedReferenceDataVehiclesRequest request)
        {
            var referenceData = (ReferenceData)_referenceDataService.Get(new ReferenceDataRequest());
            var allAssigned = _dao.GetAll().Select(x => x.ReferenceDataVehicleId).ToList();
            if (request.VehicleBeingEdited.HasValue)
            {
                allAssigned = allAssigned.Where(x => x != request.VehicleBeingEdited.Value).ToList();
            }

            return referenceData.VehiclesList.Where(x => x.Id != null && !allAssigned.Contains(x.Id.Value)).Select(x => new { x.Id, x.Display }).ToArray();
        }
    }
}