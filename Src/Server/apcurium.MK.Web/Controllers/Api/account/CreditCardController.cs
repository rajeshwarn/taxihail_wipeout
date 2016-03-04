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
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.Security;
using AutoMapper;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.Account
{
    [RoutePrefix("account")]
    public class CreditCardController : BaseApiController
    {
        private readonly CreditCardService _creditCardService;

        public CreditCardController(IOrderDao orderDao, ICreditCardDao creditCardDao, ICommandBus commandBus, IServerSettings serverSettings)
        {
            _creditCardService = new CreditCardService(creditCardDao, commandBus, orderDao, serverSettings);
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            PrepareApiServices(_creditCardService);
        }

        [HttpGet, Auth, Route("creditcards")]
        public IHttpActionResult GetCreditCards()
        {
            var result = _creditCardService.Get();

            return GenerateActionResult(result);
        }

        [HttpGet, Auth, Route("creditcardinfo/{creditCardId}")]
        public IHttpActionResult GetCreditCardInfo(Guid creditCardId)
        {
            var result = _creditCardService.Get(new CreditCardInfoRequest() {CreditCardId = creditCardId});

            return GenerateActionResult(result);
        }
        [HttpPost, Auth, Route("creditcards")]
        public IHttpActionResult Post([FromBody]CreditCardRequest request)
        {
            _creditCardService.Post(request);

            return Ok();
        }

        [HttpPost, Auth, Route("creditcard/updatedefault")]
        public IHttpActionResult Post(DefaultCreditCardRequest request)
        {
            _creditCardService.Post(request);

            return Ok();
        }

        [HttpPost, Auth, Route("creditcard/updatelabel")]
        public IHttpActionResult Post(UpdateCreditCardLabelRequest request)
        {
            _creditCardService.Post(request);

            return Ok();
        }

        [HttpDelete, Auth, Route("creditcards/{creditCardId}")]
        public IHttpActionResult DeleteCreditCard(Guid creditCardId)
        {
            _creditCardService.Delete(new CreditCardRequest() {CreditCardId = creditCardId});

            return Ok();
        }
    }
}
