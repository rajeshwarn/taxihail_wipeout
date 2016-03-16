﻿using System;
using System.Web.Http;
using System.Web.Http.Controllers;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.Security;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.Admin
{
    public class AdminCreditCardController : BaseApiController
    {
        private readonly CreditCardService _creditCardService;

        public AdminCreditCardController(IServerSettings serverSettings, ICreditCardDao creditCardDao, ICommandBus commandBus, IOrderDao orderDao)
        {
            _creditCardService = new CreditCardService(creditCardDao,commandBus, orderDao, serverSettings);
        }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

            PrepareApiServices(_creditCardService);
        }

        [HttpDelete, Auth(Role = RoleName.Support), Route("api/v2/admin/deleteAllCreditCards/{accountId}")]
        public IHttpActionResult Delete(Guid accountId)
        {
            _creditCardService.Delete(new DeleteCreditCardsWithAccountRequest() {AccountID = accountId});

            return Ok();
        }
    }
}
