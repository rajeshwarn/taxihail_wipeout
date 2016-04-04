﻿using System.Web.Http;
using System.Web.Http.Controllers;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Web.Security;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api
{
    public class LogMetricsController : BaseApiController
    {
        private readonly LogMetricsService _logMetricsService;

        public LogMetricsController(ICommandBus commandBus, IAccountDao accountDao, IOrderDao orderDao)
        {
            _logMetricsService = new LogMetricsService(commandBus, accountDao, orderDao);
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            PrepareApiServices(_logMetricsService);
        }

        [HttpPost, Auth, Route("api/v2/accounts/logstartup")]
        public IHttpActionResult LogApplicationStartUp([FromBody] LogApplicationStartUpRequest request)
        {
            _logMetricsService.Post(request);

            return Ok();
        }
        [HttpPost, Auth, Route("api/v2/orders/logeta")]
        public IHttpActionResult LogApplicationStartUp([FromBody] LogOriginalEtaRequest request)
        {
            _logMetricsService.Post(request);

            return Ok();
        }

    }
}
