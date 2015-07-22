﻿#region

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
using CMTServices;
using CustomerPortal.Client;
using CustomerPortal.Contract.Resources;
using CustomerPortal.Contract.Response;
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
        private readonly ITaxiHailNetworkServiceClient _taxiHailNetworkServiceClient;
        private readonly IServerSettings _serverSettings;
        private readonly ILogger _logger;
        private readonly IOrderDao _orderDao;

        public VehicleService(IIBSServiceProvider ibsServiceProvider,
            IVehicleTypeDao dao,
            ICommandBus commandBus,
            ReferenceDataService referenceDataService,
            ITaxiHailNetworkServiceClient taxiHailNetworkServiceClient,
            IServerSettings serverSettings,
            ILogger logger,
            IOrderDao orderDao)
        {
            _serverSettings = serverSettings;
            _ibsServiceProvider = ibsServiceProvider;
            _dao = dao;
            _commandBus = commandBus;
            _referenceDataService = referenceDataService;
            _taxiHailNetworkServiceClient = taxiHailNetworkServiceClient;
            _logger = logger;
            _orderDao = orderDao;
        }

        public AvailableVehiclesResponse Post(AvailableVehicles request)
        {
            var vehicleType = _dao.GetAll().FirstOrDefault(v => v.ReferenceDataVehicleId == request.VehicleTypeId);
            var logoName = vehicleType != null ? vehicleType.LogoName : null;

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

                if (!market.HasValue() && _serverSettings.ServerData.AvailableVehiclesMode == AvailableVehiclesModes.HoneyBadger)
                {
                    // LOCAL market Honey Badger
                    availableVehiclesMarket = _serverSettings.ServerData.HoneyBadger.AvailableVehiclesMarket;                   

                    if (_serverSettings.ServerData.HoneyBadger.AvailableVehiclesFleetId.HasValue)
                    {
                        availableVehiclesFleetIds = new[] { _serverSettings.ServerData.HoneyBadger.AvailableVehiclesFleetId.Value };
                    }
                }
                else if (!market.HasValue() && _serverSettings.ServerData.AvailableVehiclesMode == AvailableVehiclesModes.Geo)
                {
                    // LOCAL market Geo
                    availableVehiclesMarket = _serverSettings.ServerData.CmtGeo.AvailableVehiclesMarket;

                    if (_serverSettings.ServerData.CmtGeo.AvailableVehiclesFleetId.HasValue)
                    {
                        availableVehiclesFleetIds = new[] { _serverSettings.ServerData.CmtGeo.AvailableVehiclesFleetId.Value };
                    }
                }
                else
                {
                    // EXTERNAL market Honey Badger or Geo
                    availableVehiclesMarket = market;

                    try
                    {
                        // Only get available vehicles for dispatchable companies in market
                        var roamingCompanies = _taxiHailNetworkServiceClient.GetMarketFleets(_serverSettings.ServerData.TaxiHail.ApplicationKey, market);
                        if (roamingCompanies != null)
                        {
                            availableVehiclesFleetIds = roamingCompanies.Select(r => r.FleetId).ToArray();
                        }
                    }
                    catch
                    {
                        // Do nothing. If we fail to contact Customer Portal, we return an unfiltered list of available vehicles.
                        _logger.LogMessage("VehicleService: Error while trying to get Market fleets.");
                    }
                }

                var vehicleResponse = GetAvailableVehiclesServiceClient().GetAvailableVehicles(
                    market: availableVehiclesMarket,
                    latitude: request.Latitude,
                    longitude: request.Longitude,
                    searchRadius: null,
                    fleetIds: availableVehiclesFleetIds,
                    wheelchairAccessibleOnly: (vehicleType != null && vehicleType.IsWheelchairAccessible));

                vehicles = vehicleResponse.Select(v => new IbsVehiclePosition
                {
                    Latitude = v.Latitude,
                    Longitude = v.Longitude,
                    PositionDate = v.Timestamp,
                    VehicleNumber = v.Medallion,
                    FleetId = v.FleetId,
                    Eta = (int?)v.Eta
                }).ToArray();
            }

            var availableVehicles = vehicles
                .Select(v =>
                {
                    var availableVehicle = Mapper.Map<AvailableVehicle>(v);

                    availableVehicle.LogoName = logoName;

                    return availableVehicle;
                });

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
                ReferenceNetworkVehicleTypeId = request.ReferenceNetworkVehicleTypeId,
                IsWheelchairAccessible = request.IsWheelchairAccessible
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
                ReferenceNetworkVehicleTypeId = request.ReferenceNetworkVehicleTypeId,
                IsWheelchairAccessible = request.IsWheelchairAccessible
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

        public object Post(EtaForPickupRequest request)
        {
            if (_serverSettings.ServerData.AvailableVehiclesMode != AvailableVehiclesModes.Geo)
            {
                return new HttpResult(HttpStatusCode.BadRequest, "Api cannot be used unless available mode is set to Geo");
            }

            if (!request.Latitude.HasValue || !request.Longitude.HasValue || !request.VehicleNumber.HasValue())
            {
                return new HttpResult(HttpStatusCode.BadRequest, "Longitude, latitude and vehicle number are required.");
            }

            var geoService = (CmtGeoServiceClient)GetAvailableVehiclesServiceClient();

            var result = geoService.GetEta(request.Latitude.Value, request.Longitude.Value, request.VehicleNumber);

            var order = _orderDao.FindOrderStatusById(request.OrderId);

            if (order != null && !order.OriginalEta.HasValue && result.Eta.HasValue)
            {
                _commandBus.Send(new SaveOriginalEta
                {
                    OriginalEta = result.Eta.Value
                });
            }

            return new EtaForPickupResponse
            {
                Eta = result.Eta,
                Latitude = result.Latitude,
                Longitude = result.Longitude
            };
        }

        private BaseAvailableVehicleServiceClient GetAvailableVehiclesServiceClient()
        {
            switch (_serverSettings.ServerData.AvailableVehiclesMode)
            {
                case AvailableVehiclesModes.Geo:
                    return new CmtGeoServiceClient(_serverSettings, _logger);
                case AvailableVehiclesModes.HoneyBadger:
                    return new HoneyBadgerServiceClient(_serverSettings, _logger);
            }

            throw new InvalidOperationException("{0} not supported".InvariantCultureFormat(_serverSettings.ServerData.AvailableVehiclesMode.ToString()));
        }
    }
}