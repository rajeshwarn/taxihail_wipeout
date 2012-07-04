using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.Api.Contract.Requests;
using ServiceStack.ServiceInterface.Auth;
using apcurium.MK.Booking.Api.Contract.Requests;
using Infrastructure.Messaging;
using System.Threading;
using ServiceStack.ServiceHost;
using ServiceStack.Common.Web;


namespace apcurium.MK.Booking.Api.Services
{
    public class TestOnlyReqGetTestAccountService : RestServiceBase<TestOnlyReqGetTestAccount>
    {

        private ICommandBus _commandBus;
        private IAccountDao _dao;

        public TestOnlyReqGetTestAccountService(IAccountDao dao, ICommandBus commandBus)
        {
            _dao = dao;
            _commandBus = commandBus;
        }

        protected string TestUserEmail { get { return "apcurium.test{0}@apcurium.com"; } }

        protected string TestUserPassword { get { return "password1"; } }




        public override object OnGet(TestOnlyReqGetTestAccount request)
        {
            //This method can only be used for unit test.  
            if (!((RequestContext.EndpointAttributes & EndpointAttributes.Localhost) == EndpointAttributes.Localhost))
            {
                throw HttpError.NotFound("This method can only be called from the server");
            }

            string testEmail = String.Format(TestUserEmail, request.Index);

            var testAccount = _dao.FindByEmail(testEmail);
            if (testAccount != null)
            {
                return testAccount;
            }


            var command = new Commands.RegisterAccount();
            command.Id = Guid.NewGuid();
            command.AccountId = Guid.NewGuid();
            command.Email = testEmail;
            command.Password = TestUserPassword;
            command.FirstName = "Test";
            command.Phone = "123456";
            command.LastName = "User #" + request.Index.ToString();
            _commandBus.Send(command);


            Thread.Sleep(400);
            var account = _dao.FindByEmail(TestUserEmail);
            if (account != null)
            {
                return account;
            }
            return new HttpResult(HttpStatusCode.RequestTimeout);
        }

    }
}
