using System;
using apcurium.MK.Booking.IBS;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.Api.Contract.Requests;
using Infrastructure.Messaging;
using System.Threading;

namespace apcurium.MK.Booking.Api.Services
{
    public class TestOnlyReqGetTestAccountService : RestServiceBase<TestOnlyReqGetTestAccount>
    {
        private readonly ICommandBus _commandBus;
        private readonly IAccountWebServiceClient _accountWebServiceClient;
        private readonly IAccountDao _dao;

        public TestOnlyReqGetTestAccountService(IAccountDao dao, ICommandBus commandBus,IAccountWebServiceClient accountWebServiceClient)
        {
            _dao = dao;
            _commandBus = commandBus;
            _accountWebServiceClient = accountWebServiceClient;
        }
        protected string TestUserEmail { get { return "apcurium.test{0}@apcurium.com"; } }
        protected string TestUserPassword { get { return "password1"; } }
        public override object OnGet(TestOnlyReqGetTestAccount request)
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
            var ibsAccountId = _accountWebServiceClient.CreateAccount(accountId,
                                                                            testEmail,
                                                                            "",
                                                                            "Test",
                                                                            "5144567890");
            var command = new Commands.RegisterAccount
            {
                AccountId = accountId,
                Email = testEmail,
                Password = TestUserPassword,
                Name = "Test",
                Phone = "5144567890",
                IbsAccountId = ibsAccountId,
                ConfimationToken = accountId.ToString("N"),
                Language = "en",
                IsAdmin = false
            };

            _commandBus.Send(command);

            Thread.Sleep(400);
            // Confirm account immediately
            _commandBus.Send(new Commands.ConfirmAccount
            {
                 AccountId = command.AccountId,
                 ConfimationToken = command.ConfimationToken
            });

            return _dao.FindByEmail(testEmail);
        }

    }
}
