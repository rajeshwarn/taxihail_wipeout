using System;
using System.Net;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api.Account
{
    [Auth]
    public class AddressController : BaseApiController
    {
        public AddressesService AddressesService { get; private set; }
        public AddressHistoryService AddressHistoryService { get; private set; }
        public SaveAddressService SaveAddressService { get; private set; }

        public AddressController(AddressesService addressesService, AddressHistoryService addressHistoryService, SaveAddressService saveAddressService)
        {
            AddressesService = addressesService;
            AddressHistoryService = addressHistoryService;
            SaveAddressService = saveAddressService;
        }

        [HttpGet, Route("api/accounts/addresses")]
        public IHttpActionResult GetAddresses()
        {
            return GenerateActionResult(AddressesService.Get());
        }

        [HttpGet, Route("api/accounts/addresses/history")]
        public IHttpActionResult GetAddressHistory()
        {
            var result = AddressHistoryService.Get();

            return GenerateActionResult(result);
        }

        [HttpDelete, Route("api/accounts/addresses/history/{addressId}")]
        public IHttpActionResult Delete(Guid addressId)
        {
            AddressHistoryService.Delete(new AddressHistoryRequest() {AddressId = addressId});

            return Ok();
        }

        [HttpPost, Route("api/accounts/addresses")]
        public IHttpActionResult CreateAddress([FromBody] SaveAddress request)
        {
            SaveAddressService.Post(request);

            return StatusCode(HttpStatusCode.Created);
        }

        [HttpPut, Route("api/accounts/addresses/{id}")]
        public IHttpActionResult UpdateAddress([FromBody] SaveAddress request, Guid id)
        {
            request.Id = id;

            SaveAddressService.Put(request);

            return Ok();
        }

        [HttpDelete, Route("api/accounts/addresses/{id}")]
        public IHttpActionResult DeleteAddress(Guid id)
        {
            SaveAddressService.Delete(id);

            return Ok();
        }
    }
}
