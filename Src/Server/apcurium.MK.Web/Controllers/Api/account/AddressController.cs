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
        private readonly AddressesService _addressesService;
        private readonly AddressHistoryService _addressHistoryService;

        public AddressController(IAddressDao addressDao, IAccountDao accountDao, ICommandBus commandBus)
        {
            _addressesService = new AddressesService(addressDao);

            _addressHistoryService = new AddressHistoryService(addressDao,commandBus, accountDao );
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            PrepareApiServices(_addressHistoryService, _addressesService);
        }

        [HttpGet, Route]
        public IHttpActionResult GetAddresses()
        {
            return GenerateActionResult(_addressesService.Get());
        }

        [HttpGet, Route("history")]
        public IHttpActionResult GetAddressHistory()
        {
            var result = _addressHistoryService.Get();

            return GenerateActionResult(result);
        }

        [HttpDelete, Route("history/{addressId}")]
        public IHttpActionResult Delete(Guid addressId)
        {
            _addressHistoryService.Delete(new AddressHistoryRequest() {AddressId = addressId});

            return Ok();
        }


        
    }
}
