﻿using System;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using ServiceStack.ServiceInterface;
using ServiceStack.Common.Web;
using System.Net;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Api.Services
{
    public class CurrentAccountService : Service
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

        public object Get(CurrentAccount request)
        {
            var session = this.GetSession();
            
            // Just in case someone was able to authenticate with a null session. 
            if (!session.Id.HasValueTrimmed())
            {
                throw new HttpError(HttpStatusCode.Forbidden, "NoSession");
            }

            var account = _accountDao.FindById(new Guid(session.UserAuthId));

            var creditCard = account.DefaultCreditCard.HasValue
                ? _creditCardDao.FindById(account.DefaultCreditCard.Value)
                : null;

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
                        Label = creditCard.Label,
                        ZipCode = creditCard.ZipCode,
                        StreetNumber = creditCard.StreetNumber,
                        StreetName = creditCard.StreetName,
                        Email = creditCard.Email,
                        Phone = creditCard.Phone
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

		public object Get(CurrentAccountPhoneRequest currentAccountPhoneRequest)
		{
			var account = _accountDao.FindByEmail(currentAccountPhoneRequest.Email);

			if (account == null)
			{
				throw new HttpError(HttpStatusCode.NotFound, "No account matching this email address");
			}

			if (account.IsConfirmed)
			if (account.IsConfirmed)
			{
				throw new HttpError(HttpStatusCode.PreconditionFailed, "To get phone number the account should not be confirmed");
			}

			return new CurrentAccountPhoneResponse()
			{
				CountryCode = account.Settings.Country,
				PhoneNumber = account.Settings.Phone
			};
		}
    }
}