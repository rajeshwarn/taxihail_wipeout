#region

using System;
using System.Threading;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using RegisterAccount = apcurium.MK.Booking.Commands.RegisterAccount;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class TestOnlyReqGetTestAccountService : Service
    {
        private readonly ICommandBus _commandBus;
        private readonly IAccountDao _dao;

        public TestOnlyReqGetTestAccountService(IAccountDao dao, ICommandBus commandBus)
        {
            _dao = dao;
            _commandBus = commandBus;
        }

        protected string TestUserEmail
        {
            get { return "apcurium.test{0}@apcurium.com"; }
        }

        protected string TestUserPassword
        {
            get { return "password1"; }
        }

        public object Get(TestOnlyReqGetTestAccount request)
        {
            //This method can only be used for unit test.  
            if ((RequestContext.EndpointAttributes & EndpointAttributes.Localhost) != EndpointAttributes.Localhost)
            {
                throw HttpError.NotFound("This method can only be called from the server");
            }

            var testEmail = String.Format(TestUserEmail, request.Index);
            var testAccount = _dao.FindByEmail(testEmail);

            if (testAccount != null)
            {
                return testAccount;
            }

            var accountId = Guid.NewGuid();

            var command = new RegisterAccount
            {
                AccountId = accountId,
                Email = testEmail,
                Password = TestUserPassword,
                Name = "Test",
                Phone = "5144567890",
                Country = new CountryISOCode("CA"),
                ConfimationToken = accountId.ToString("N"),
                Language = "en",
                IsAdmin = false
            };

            _commandBus.Send(command);

            Thread.Sleep(400);
            // Confirm account immediately
            _commandBus.Send(new ConfirmAccount
            {
                AccountId = command.AccountId,
                ConfimationToken = command.ConfimationToken
            });

            return _dao.FindByEmail(testEmail);
        }
    }
}