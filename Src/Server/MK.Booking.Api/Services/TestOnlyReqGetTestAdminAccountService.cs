#region

using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using Infrastructure.Messaging;
using RegisterAccount = apcurium.MK.Booking.Commands.RegisterAccount;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class TestOnlyReqGetTestAdminAccountService : BaseApiService
    {
        private readonly ICommandBus _commandBus;
        private readonly IAccountDao _dao;

        public TestOnlyReqGetTestAdminAccountService(IAccountDao dao, ICommandBus commandBus)
        {
            _dao = dao;
            _commandBus = commandBus;
        }

        protected string TestUserEmail
        {
            get { return "apcurium.testadmin{0}@apcurium.com"; }
        }

        protected string TestUserPassword
        {
            get { return "password1"; }
        }


        public async Task<AccountDetail> Get(string index)
        {
            //This method can only be used for unit test. 
            if (!HttpRequestContext.IsLocal)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "This method can only be called from the server");
            }

            var testEmail = string.Format(TestUserEmail, index);

            var testAccount = _dao.FindByEmail(testEmail);
            if (testAccount != null)
            {
                return testAccount;
            }

            var command = new RegisterAccount
            {
                AccountId = Guid.NewGuid(),
                Email = testEmail,
                Password = TestUserPassword,
                Name = "Test",
                Phone = "5141234567",
                Country = new CountryISOCode("CA"),
                ConfimationToken = Guid.NewGuid().ToString("N"),
                Language = "en",
                IsAdmin = true
            };

            _commandBus.Send(command);

            await Task.Delay(400);
            // Confirm account immediately
            _commandBus.Send(new ConfirmAccount
            {
                AccountId = command.AccountId,
                ConfimationToken = command.ConfimationToken
            });

            _commandBus.Send(new AddOrUpdateCreditCard
            {
                AccountId = command.AccountId,
                CreditCardCompany = "Visa",
                CreditCardId = Guid.NewGuid(),
                ExpirationMonth = DateTime.Now.AddYears(1).ToString("MM"),
                ExpirationYear = DateTime.Now.AddYears(1).ToString("yy"),
                Last4Digits = "1234",
                NameOnCard = "test test",
                Token = "123456",
                Label = CreditCardLabelConstants.Personal.ToString()
            });

            return _dao.FindByEmail(testEmail);
        }
    }
}