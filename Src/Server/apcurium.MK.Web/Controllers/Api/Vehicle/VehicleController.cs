using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Web.Security;
using CustomerPortal.Client;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.Vehicle
{
    [RoutePrefix("api")]
    public class VehicleController : BaseApiController
    {
        public VehicleService VehicleService { get; }
        public VehicleController(
            IIBSServiceProvider ibsServiceProvider,
            IVehicleTypeDao dao,
            ICommandBus commandBus,
            ReferenceDataService referenceDataService,
            ITaxiHailNetworkServiceClient taxiHailNetworkServiceClient,
            IServerSettings serverSettings,
            ILogger logger,
            IOrderDao orderDao)
        {
            VehicleService = new VehicleService(ibsServiceProvider, dao, commandBus, referenceDataService, taxiHailNetworkServiceClient, serverSettings, logger, orderDao );
        }

        [HttpPost]
        public async Task<IHttpActionResult> AvailableVehicles([FromBody]AvailableVehicles request)
        {
            var result = await VehicleService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpGet, Auth,Route("admin/vehicletypes/{id:Guid?}")]
        public IHttpActionResult GetVehicleType(Guid? id)
        {
            var result = VehicleService.Get(id ?? Guid.Empty);

            return GenerateActionResult(result);
        }

        [HttpPost, Auth(Role = RoleName.Admin), Route("admin/vehicletypes")]
        public IHttpActionResult CreateVehicleType([FromBody]VehicleTypeRequest request)
        {
            var result = VehicleService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPut, Auth(Role = RoleName.Admin), Route("admin/vehicletypes")]
        public IHttpActionResult UpdateVehicleType([FromBody]VehicleTypeRequest request)
        {
            var result = VehicleService.Put(request);

            return GenerateActionResult(result);
        }

        [HttpDelete, Auth(Role = RoleName.Admin), Route("admin/vehicletypes/{id}")]
        public IHttpActionResult UpdateVehicleType(Guid id)
        {
            VehicleService.DeleteVehicleType(id);

            return Ok();
        }

        [HttpGet, NoCache, Route("admin/vehicletypes/unassignednetworkvehicletype/{networkVehicleId:int?}")]
        public async Task<IHttpActionResult> GetUnassignedNetworkVehicleType(int? networkVehicleId)
        {
            var result = await VehicleService.GetUnassignedNetworkVehicleType(networkVehicleId);

            return GenerateActionResult(result);
        }
        
        [HttpGet, NoCache, Route("admin/vehicletypes/unassignedreference/{vehicleBeingEdited:int?}")]
        public IHttpActionResult GetUnassignedReferenceDataVehicles(int? vehicleBeingEdited)
        {
            var result = VehicleService.GetUnassignedReferenceDataVehicles(vehicleBeingEdited);

            return GenerateActionResult(result);
        }

        [HttpGet, NoCache, Route("taxilocation/{orderId}")]
        public async Task<IHttpActionResult> GetTaxiLocation(Guid orderId, [FromUri] string medallion)
        {
            var result = await VehicleService.Get(new TaxiLocationRequest
            {
                OrderId = orderId,
                Medallion = medallion
            });

            return GenerateActionResult(result);
        }
        [HttpPost, Route("vehicles/eta")]
        public async Task<IHttpActionResult> EtaForPickupRequest(EtaForPickupRequest request)
        {
            var result = await VehicleService.Post(request);

            return GenerateActionResult(result);
        }

    }
}
