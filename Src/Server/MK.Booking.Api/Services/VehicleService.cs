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
using apcurium.MK.Common.Extensions;
using AutoMapper;
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

        public VehicleService(IIBSServiceProvider ibsServiceProvider,
            IVehicleTypeDao dao,
            ICommandBus commandBus,
            ReferenceDataService referenceDataService,
            HoneyBadgerServiceClient honeyBadgerServiceClient)
        {
            _ibsServiceProvider = ibsServiceProvider;
            _dao = dao;
            _commandBus = commandBus;
            _referenceDataService = referenceDataService;
            _honeyBadgerServiceClient = honeyBadgerServiceClient;
        }

        public AvailableVehiclesResponse Post(AvailableVehicles request)
        {
            var vehicleType = _dao.GetAll().FirstOrDefault(v => v.ReferenceDataVehicleId == request.VehicleTypeId);
            string logoName = vehicleType != null ? vehicleType.LogoName : null;

            var vehicles = new IbsVehiclePosition[0];

            if (!request.Market.HasValue())
            {
                vehicles = _ibsServiceProvider.Booking()
                    .GetAvailableVehicles(request.Latitude, request.Longitude, request.VehicleTypeId);
            }
            else
            {
                var vehicleResponse = _honeyBadgerServiceClient.GetAvailableVehicles(request.Market, request.Latitude, request.Longitude);
                vehicles = vehicleResponse.Select(v => new IbsVehiclePosition
                {
                    Latitude = v.Latitude,
                    Longitude = v.Longitude,
                    PositionDate = v.Timestamp,
                    VehicleNumber = v.Medaillon
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
                CompanyId = AppConstants.CompanyId
            };

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
                CompanyId = AppConstants.CompanyId
            };

            _commandBus.Send(command);

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

            return new HttpResult(HttpStatusCode.OK, "OK");
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