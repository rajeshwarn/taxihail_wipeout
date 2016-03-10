using System;
using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.OrderCreation;
using apcurium.MK.Booking.Calculator;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using CustomerPortal.Client;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.Account.Orders
{
    [RoutePrefix("api/v2/account/orders")]
    public class CreateOrderController : BaseApiController
    {
        public CreateOrderService CreateOrderService { get; }

        public CreateOrderController(IServerSettings serverSettings,
            ICommandBus commandBus,
            IAccountChargeDao accountChargeDao,
            IPaymentService paymentService,
            ICreditCardDao creditCardDao,
            IIBSServiceProvider ibsServiceProvider,
            IPromotionDao promotionDao,
            IEventSourcedRepository<Promotion> promoRepository,
            IOrderPaymentDao orderPaymentDao,
            IAccountDao accountDao,
            IPayPalServiceFactory payPalServiceFactory,
            ILogger logger,
            ITaxiHailNetworkServiceClient taxiHailNetworkServiceClient,
            IRuleCalculator ruleCalculator,
            IFeesDao feesDao,
            ReferenceDataService referenceDataService,
            IOrderDao orderDao,
            IIbsCreateOrderService ibsCreateOrderService)
        {
            CreateOrderService = new CreateOrderService(
                commandBus, 
                accountDao,
                serverSettings,
                referenceDataService,
                ibsServiceProvider,
                ruleCalculator,
                accountChargeDao,
                creditCardDao,
                orderDao,
                promotionDao,
                promoRepository,
                taxiHailNetworkServiceClient,
                paymentService,
                payPalServiceFactory,
                orderPaymentDao, 
                feesDao,
                logger, 
                ibsCreateOrderService);
        }

        [HttpPost]
        public IHttpActionResult CreateOrder([FromBody]CreateOrderRequest request)
        {
            var result = CreateOrderService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPost, Route("{orderId}/switchDispatchCompany")]
        public async Task<IHttpActionResult> SwitchOrderToNextDispatchCompany(Guid orderId, [FromBody] SwitchOrderToNextDispatchCompanyRequest request)
        {
            request.OrderId = orderId;

            var result = await CreateOrderService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPost, Route("{orderId}/ignoreDispatchCompanySwitch")]
        public IHttpActionResult IgnoreDispatchCompanySwitch(Guid orderId, [FromBody] IgnoreDispatchCompanySwitchRequest request)
        {
            request.OrderId = orderId;

            CreateOrderService.Post(request);

            return Ok();
        }
    }
}
