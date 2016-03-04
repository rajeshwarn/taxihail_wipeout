using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Entity;
using apcurium.MK.Web.Security;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.Account
{
    [RoutePrefix("account/addresses"), Auth]
    public class AddressController : BaseApiController
    {
        public AddressesService AddressesService { get; }
        public AddressHistoryService AddressHistoryService { get;  }
        public SaveAddressService SaveAddressService { get;  }

        public AddressController(IAddressDao addressDao, IAccountDao accountDao, ICommandBus commandBus)
        {
            AddressesService = new AddressesService(addressDao);

            AddressHistoryService = new AddressHistoryService(addressDao,commandBus, accountDao );

            SaveAddressService = new SaveAddressService(commandBus);
        }

        [HttpGet, Route]
        public IHttpActionResult GetAddresses()
        {
            return GenerateActionResult(AddressesService.Get());
        }

        [HttpGet, Route("history")]
        public IHttpActionResult GetAddressHistory()
        {
            var result = AddressHistoryService.Get();

            return GenerateActionResult(result);
        }

        [HttpDelete, Route("history/{addressId}")]
        public IHttpActionResult Delete(Guid addressId)
        {
            AddressHistoryService.Delete(new AddressHistoryRequest() {AddressId = addressId});

            return Ok();
        }

        [HttpPost]
        public IHttpActionResult CreateAddress([FromBody] SaveAddress request)
        {
            SaveAddressService.Post(request);

            return StatusCode(HttpStatusCode.Created);
        }

        [HttpPut, Route("{id}")]
        public IHttpActionResult UpdateAddress([FromBody] SaveAddress request, Guid id)
        {
            request.Id = id;

            SaveAddressService.Put(request);

            return Ok();
        }

        [HttpDelete, Route("{id}")]
        public IHttpActionResult DeleteAddress(Guid id)
        {
            SaveAddressService.Delete(id);

            return Ok();
        }
    }
}
