using System;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using System.Net;
using System.Web;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Api.Services
{
    public class CurrentAccountService : BaseApiService
    {
        private readonly IAccountDao _accountDao;
        private readonly ICreditCardDao _creditCardDao;
        private readonly IServerSettings _serverSettings;

        public CurrentAccountService(IAccountDao accountDao, ICreditCardDao creditCardDao, IServerSettings serverSettings)
        {
            _creditCardDao = creditCardDao;
            _accountDao = accountDao;
            _serverSettings = serverSettings;
        }

        public CurrentAccountResponse Get(CurrentAccount request)
        {
            // Just in case someone was able to authenticate with a null session. 
            if (!Session.SessionId.HasValueTrimmed())
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, "NoSession");
            }

            var account = _accountDao.FindById(Session.UserId);

            var creditCard = account.DefaultCreditCard.HasValue
                ? _creditCardDao.FindById(account.DefaultCreditCard.Value)
                : null;

            var creditCardLabel = CreditCardLabelConstants.Personal;
            if (creditCard != null)
            {
                Enum.TryParse(creditCard.Label, out creditCardLabel);
            }

            var creditCardResource = creditCard != null
                ? new CreditCardDetails
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
                    }
                : null;

            var currentAccount = new CurrentAccountResponse
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
                DefaultCreditCard = creditCardResource,
                DefaultTipPercent = account.DefaultTipPercent,
                IsPayPalAccountLinked = account.IsPayPalAccountLinked
            };

            currentAccount.Settings.ChargeTypeId = account.Settings.ChargeTypeId ?? _serverSettings.ServerData.DefaultBookingSettings.ChargeTypeId;
            currentAccount.Settings.VehicleTypeId = account.Settings.VehicleTypeId ?? _serverSettings.ServerData.DefaultBookingSettings.VehicleTypeId;
            currentAccount.Settings.ProviderId = account.Settings.ProviderId ?? _serverSettings.ServerData.DefaultBookingSettings.ProviderId;

            return currentAccount;
        }

		public CurrentAccountPhoneResponse Get(CurrentAccountPhoneRequest currentAccountPhoneRequest)
		{
			var account = _accountDao.FindByEmail(currentAccountPhoneRequest.Email);

			if (account == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "No account matching this email address");
			}

			if (account.IsConfirmed)
			{
				throw new HttpException((int)HttpStatusCode.PreconditionFailed, "To get phone number the account should not be confirmed");
			}

			return new CurrentAccountPhoneResponse()
			{
				CountryCode = account.Settings.Country,
				PhoneNumber = account.Settings.Phone
			};
		}
    }
}