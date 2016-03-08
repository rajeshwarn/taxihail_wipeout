using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests.Client;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.OrderCreation;
using apcurium.MK.Booking.Calculator;
using apcurium.MK.Booking.Domain;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Web.Security;
using CustomerPortal.Client;
using Infrastructure.EventSourcing;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.Account.Orders
{
    [RoutePrefix("api/v2/client/hail"), Auth]
    public class HailController : BaseApiController
    {
        public HailService HailService { get; }

        public HailController(IServerSettings serverSettings,
            ICommandBus commandBus,
            ILogger logger,
            IAccountChargeDao accountChargeDao,
            ICreditCardDao creditCardDao,
            IIBSServiceProvider ibsServiceProvider,
            IPromotionDao promotionDao,
            IAccountDao accountDao,
            IFeesDao feesDao,
            IOrderDao orderDao,
            IOrderPaymentDao orderPaymentDao,
            IPaymentService paymentService,
            ReferenceDataService referenceDataService,
            IIbsCreateOrderService ibsCreateOrderService,
            IEventSourcedRepository<Promotion> promoRepository,
            IPayPalServiceFactory payPalServiceFactory,
            ITaxiHailNetworkServiceClient taxiHailNetworkServiceClient,
            IRuleCalculator ruleCalculator,
            Booking.Resources.Resources resources)
        {
            HailService = new HailService(
                serverSettings, 
                commandBus,
                logger,
                accountChargeDao, 
                creditCardDao, 
                ibsServiceProvider, 
                promotionDao, 
                accountDao, 
                feesDao, 
                orderDao, 
                orderPaymentDao, 
                paymentService, 
                referenceDataService, 
                ibsCreateOrderService, 
                promoRepository, 
                payPalServiceFactory, 
                taxiHailNetworkServiceClient, 
                ruleCalculator, 
                resources);
        }

        [HttpPost]
        public async Task<IHttpActionResult> Hail([FromBody]HailRequest request)
        {
            var result = await HailService.Post(request);

            return GenerateActionResult(result);
        }

        [HttpPost, Route("confirm")]
        public IHttpActionResult ConfirmHail([FromBody]ConfirmHailRequest request)
        {
            var result = HailService.Post(request);

            return GenerateActionResult(result);
        }
    }
}
