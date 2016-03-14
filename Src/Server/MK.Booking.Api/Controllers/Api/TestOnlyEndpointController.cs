﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.IoC;
using Infrastructure.Messaging;
using Microsoft.Practices.Unity;
using UnityServiceLocator = apcurium.MK.Common.IoC.UnityServiceLocator;

namespace apcurium.MK.Web.Controllers.Api
{
    [RoutePrefix("api/v2/accounts/test")]
    public class TestOnlyEndpointController : BaseApiController
    {
        public TestOnlyReqGetTestAccountService TestOnlyReqGetTestAccountService { get; }
        public TestOnlyReqGetTestAdminAccountService TestOnlyReqGetTestAdminAccountService { get; set; }

        public TestOnlyEndpointController()
        {
            var accountDao = UnityServiceLocator.Instance.Resolve<IAccountDao>();
            var commandBus = UnityServiceLocator.Instance.Resolve<ICommandBus>();

            TestOnlyReqGetTestAccountService = new TestOnlyReqGetTestAccountService(accountDao, commandBus);
            TestOnlyReqGetTestAdminAccountService = new TestOnlyReqGetTestAdminAccountService(accountDao, commandBus);
        }

        [HttpGet, Route("{index}")]
        public async Task<IHttpActionResult> GetTestAccount(string index)
        {
            var result = await TestOnlyReqGetTestAccountService.Get(index);

            return GenerateActionResult(result);
        }

        [HttpGet, Route("admin/{index}")]
        public async Task<IHttpActionResult> GetAdminTestAccount(string index)
        {
            var result = await TestOnlyReqGetTestAdminAccountService.Get(index);

            return GenerateActionResult(result);
        }

    }
}
