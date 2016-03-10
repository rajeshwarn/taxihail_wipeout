using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.Payment;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.Security;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.Account
{
    [RoutePrefix("/api/v2/accounts")]
    public class OverduePaymentsController : BaseApiController
    {
        public OverduePaymentService OverduePaymentService { get; }


        public OverduePaymentsController(
            ICommandBus commandBus,
            IOverduePaymentDao overduePaymentDao,
            IAccountDao accountDao,
            IOrderDao orderDao,
            IOrderPaymentDao orderPaymentDao,
            IPromotionDao promotionDao,
            IPaymentService paymentService,
            IServerSettings serverSettings)
        {
            OverduePaymentService = new OverduePaymentService(commandBus, overduePaymentDao, accountDao, orderDao,orderPaymentDao,promotionDao,paymentService,serverSettings);
        }

        [Route("overduepayment"), HttpGet, Auth]
        public IHttpActionResult GetOverduePayment()
        {
            var result = OverduePaymentService.Get();

            return GenerateActionResult(result);
        }

        [Route("settleoverduepayment"), HttpPost, Auth]
        public IHttpActionResult SettleOverduePayment()
        {
            var result = OverduePaymentService.Post();

            return GenerateActionResult(result);
        }
    }
}
