using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using apcurium.MK.Booking.Api.Contract.Requests.Payment;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Web.App_Start;
using apcurium.MK.Web.Security;
using Infrastructure.Messaging;
using Microsoft.Practices.Unity;

namespace apcurium.MK.Web.Controllers.Api.Account
{
    [RoutePrefix("api/v2/accounts/manualridelinq")]
    public class ManualRideLinqController : BaseApiController
    {
        private readonly ManualRidelinqOrderService _manualRidelinqOrderService;

        public ManualRideLinqController(
            ICommandBus commandBus, 
            IOrderDao orderDao,
            IAccountDao accountDao,
            ICreditCardDao creditcardDao,
            IServerSettings serverSettings, 
            ILogger logger, 
            INotificationService notification)
        {
            _manualRidelinqOrderService = new ManualRidelinqOrderService(commandBus, orderDao, accountDao, creditcardDao, serverSettings, logger, notification);
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            PrepareApiServices(_manualRidelinqOrderService);
        }

        [HttpGet, Route("{orderId}"), Auth]
        public IHttpActionResult GetManualRideling(Guid orderId)
        {
            var result = _manualRidelinqOrderService.Get(new ManualRideLinqRequest() {OrderId = orderId});

            return GenerateActionResult(result);
        }

        [HttpDelete, Route("{orderId}"), Auth]
        public async Task<IHttpActionResult> DeleteManualRideling(Guid orderId)
        {
            await _manualRidelinqOrderService.Delete(new ManualRideLinqRequest() { OrderId = orderId });

            return Ok();
        }

        [HttpPost, Auth]
        public async Task<IHttpActionResult> PairWithManualRidelinq([FromBody] ManualRideLinqPairingRequest request)
        {
            var result = await _manualRidelinqOrderService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPost, Route("{orderId}/tip"), Auth]
        public async Task<IHttpActionResult> UpdateAutoTip(Guid orderId, [FromBody] ManualRideLinqUpdateAutoTipRequest request)
        {
            request.OrderId = orderId;

            var result = await _manualRidelinqOrderService.Put(request);

            return GenerateActionResult(result);
        }
    }
}
