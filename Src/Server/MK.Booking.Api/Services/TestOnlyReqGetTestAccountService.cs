#region

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common;
using Infrastructure.Messaging;
using CreditCardDetails = apcurium.MK.Booking.Api.Contract.Resources.CreditCardDetails;
using RegisterAccount = apcurium.MK.Booking.Commands.RegisterAccount;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class TestOnlyReqGetTestAccountService : BaseApiService
    {
        protected readonly ICommandBus CommandBus;
        protected readonly ICreditCardDao CreditCardDao;
        protected readonly IAccountDao Dao;

        public TestOnlyReqGetTestAccountService(IAccountDao dao, ICommandBus commandBus, ICreditCardDao creditCardDao)
        {
            Dao = dao;
            CommandBus = commandBus;
            CreditCardDao = creditCardDao;
        }

        protected string TestUserEmail
        {
            get { return "apcurium.test{0}@apcurium.com"; }
        }

        protected string TestUserPassword
        {
            get { return "password1"; }
        }

        public async Task<Account> GetTestAccount(string index)
        {
            //This method can only be used for unit test. 
            if (!HttpRequestContext.IsLocal)
            {
                throw GenerateException(HttpStatusCode.NotFound, "This method can only be called from the server");
            }

            var testEmail = string.Format(TestUserEmail, index);
            var testAccount = Dao.FindByEmail(testEmail);

            if (testAccount != null)
            {
                return MapAccount(testAccount);
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

            CommandBus.Send(command);

            await Task.Delay(400);
            // Confirm account immediately
            CommandBus.Send(new ConfirmAccount
            {
                AccountId = command.AccountId,
                ConfimationToken = command.ConfimationToken
            });

            return MapAccount(Dao.FindByEmail(testEmail));
        }

        protected Account MapAccount(AccountDetail account)
        {
            var creditCard = account.DefaultCreditCard.HasValue
                ? CreditCardDao.FindById(account.DefaultCreditCard.Value)
                : null;

            return new Account()
            {
                Id = account.Id,
                Email = account.Email,
                Name = account.Name,
                IbsAccountid = account.IBSAccountId ?? 0,
                FacebookId = account.FacebookId,
                TwitterId = account.TwitterId,
                Settings = account.Settings,
                Language = account.Language,
                HasAdminAccess = account.HasAdminAccess,
                IsSuperAdmin = account.RoleNames.Contains(RoleName.SuperAdmin),
                DefaultCreditCard = MapCreditCard(creditCard),
                DefaultTipPercent = account.DefaultTipPercent,
                IsPayPalAccountLinked = account.IsPayPalAccountLinked
            };
        }

        private CreditCardDetails MapCreditCard(ReadModel.CreditCardDetails creditCard)
        {
            if (creditCard == null)
            {
                return null;
            }
            
            CreditCardLabelConstants creditCardLabel;
            Enum.TryParse(creditCard.Label, out creditCardLabel);

            return new CreditCardDetails
            {
                CreditCardId = creditCard.CreditCardId,
                AccountId = creditCard.AccountId,
                NameOnCard = creditCard.NameOnCard,
                Token = creditCard.Token,
                Last4Digits = creditCard.Last4Digits,
                CreditCardCompany = creditCard.CreditCardCompany,
                ExpirationMonth = creditCard.ExpirationMonth,
                ExpirationYear = creditCard.ExpirationYear,
                IsDeactivated = creditCard.IsDeactivated,
                Label = creditCardLabel,
                ZipCode = creditCard.ZipCode
            };
        }
    }
}