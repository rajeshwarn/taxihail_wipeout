#region

using System;
using System.Linq;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;

using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using AutoMapper;
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

        public VehicleService(IIBSServiceProvider ibsServiceProvider, IVehicleTypeDao dao, ICommandBus commandBus, ReferenceDataService referenceDataService)
        {
            _ibsServiceProvider = ibsServiceProvider;
            _dao = dao;
            _commandBus = commandBus;
            _referenceDataService = referenceDataService;
        }

        public AvailableVehiclesResponse Post(AvailableVehicles request)
        {
            var vehicles = _ibsServiceProvider.Booking().GetAvailableVehicles(request.Latitude, request.Longitude, request.VehicleTypeId, request.Market);
            var vehicleType = _dao.GetAll().FirstOrDefault(v => v.ReferenceDataVehicleId == request.VehicleTypeId);
            string logoName = vehicleType != null ? vehicleType.LogoName : null;

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