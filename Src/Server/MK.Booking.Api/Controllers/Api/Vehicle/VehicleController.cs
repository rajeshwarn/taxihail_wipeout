using System;
using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Security;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api.Vehicle
{
    public class VehicleController : BaseApiController
    {
        public VehicleService VehicleService { get; private set; }

        public VehicleController(VehicleService vehicleService)
        {
            VehicleService = vehicleService;
        }

        [HttpPost, Route("api/v2/vehicles")]
        public async Task<IHttpActionResult> AvailableVehicles([FromBody]AvailableVehicles request)
        {
            var result = await VehicleService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpGet, Auth, Route("api/v2/admin/vehicletypes")]
        public IHttpActionResult GetVehicleType()
        {
            return GetVehicleType(null);
        }

        [HttpGet, Auth, Route("api/v2/admin/vehicletypes/{id}")]
        public IHttpActionResult GetVehicleType(Guid? id)
        {
            var result = VehicleService.Get(id ?? Guid.Empty);

            return GenerateActionResult(result);
        }

        [HttpPost, Auth(Role = RoleName.Admin), Route("api/v2/admin/vehicletypes")]
        public IHttpActionResult CreateVehicleType([FromBody]VehicleTypeRequest request)
        {
            var result = VehicleService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPut, Auth(Role = RoleName.Admin), Route("api/v2/admin/vehicletypes")]
        public IHttpActionResult UpdateVehicleType([FromBody]VehicleTypeRequest request)
        {
            var result = VehicleService.Put(request);

            return GenerateActionResult(result);
        }

        [HttpDelete, Auth(Role = RoleName.Admin), Route("api/v2/admin/vehicletypes/{id}")]
        public IHttpActionResult UpdateVehicleType(Guid id)
        {
            VehicleService.DeleteVehicleType(id);

            return Ok();
        }

        [HttpGet, NoCache, Route("api/v2/admin/vehicletypes/unassignednetworkvehicletype")]
        public Task<IHttpActionResult> GetUnassignedNetworkVehicleType()
        {
            return GetUnassignedNetworkVehicleType(null);
        }

        [HttpGet, NoCache, Route("api/v2/admin/vehicletypes/unassignednetworkvehicletype/{networkVehicleId:int?}")]
        public async Task<IHttpActionResult> GetUnassignedNetworkVehicleType(int? networkVehicleId)
        {
            var result = await VehicleService.GetUnassignedNetworkVehicleType(networkVehicleId);

            return GenerateActionResult(result);
        }
        
        [HttpGet, NoCache, Route("api/v2/admin/vehicletypes/unassignedreference/{vehicleBeingEdited:int?}")]
        public IHttpActionResult GetUnassignedReferenceDataVehicles(int? vehicleBeingEdited)
        {
            var result = VehicleService.GetUnassignedReferenceDataVehicles(vehicleBeingEdited);

            return GenerateActionResult(result);
        }

        [HttpGet, NoCache, Route("api/v2/taxilocation/{orderId}")]
        public async Task<IHttpActionResult> GetTaxiLocation(Guid orderId, [FromUri] string medallion)
        {
            var result = await VehicleService.Get(new TaxiLocationRequest
            {
                OrderId = orderId,
                Medallion = medallion
            });

            return GenerateActionResult(result);
        }
        [HttpPost, Route("api/v2/vehicles/eta")]
        public async Task<IHttpActionResult> EtaForPickupRequest(EtaForPickupRequest request)
        {
            var result = await VehicleService.Post(request);

            return GenerateActionResult(result);
        }

    }
}
