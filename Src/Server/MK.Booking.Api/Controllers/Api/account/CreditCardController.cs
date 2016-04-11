using System;
using System.Web.Http;
using System.Web.Http.Controllers;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Web.Security;

namespace apcurium.MK.Web.Controllers.Api.Account
{
    public class CreditCardController : BaseApiController
    {
        public CreditCardService CardService { get; private set; }

        public CreditCardController(CreditCardService cardService)
        {
            CardService = cardService;
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            PrepareApiServices(CardService);
        }

        [HttpGet, Auth, Route("api/v2/accounts/creditcards")]
        public IHttpActionResult GetCreditCards()
        {
            var result = CardService.Get();

            return GenerateActionResult(result);
        }

        [HttpGet, Auth, Route("api/v2/accounts/creditcardinfo/{creditCardId}")]
        public IHttpActionResult GetCreditCardInfo(Guid creditCardId)
        {
            var result = CardService.Get(new CreditCardInfoRequest() {CreditCardId = creditCardId});

            return GenerateActionResult(result);
        }
        [HttpPost, Auth, Route("api/v2/accounts/creditcards")]
        public IHttpActionResult Post([FromBody]CreditCardRequest request)
        {
            CardService.Post(request);

            return Ok();
        }

        [HttpPost, Auth, Route("api/v2/accounts/creditcards/updatedefault")]
        public IHttpActionResult Post(DefaultCreditCardRequest request)
        {
            CardService.Post(request);

            return Ok();
        }

        [HttpPost, Auth, Route("api/v2/accounts/creditcards/updatelabel")]
        public IHttpActionResult Post(UpdateCreditCardLabelRequest request)
        {
            CardService.Post(request);

            return Ok();
        }

        [HttpDelete, Auth, Route("api/v2/accounts/creditcards/{creditCardId}")]
        public IHttpActionResult DeleteCreditCard(Guid creditCardId)
        {
            CardService.Delete(new CreditCardRequest() {CreditCardId = creditCardId});

            return Ok();
        }
    }
}
