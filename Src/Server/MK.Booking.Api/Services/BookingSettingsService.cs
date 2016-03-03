#region

using System;
using System.Net;
using System.Web;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using AutoMapper;
using Infrastructure.Messaging;
using apcurium.MK.Common;
using apcurium.MK.Common.Helpers;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class BookingSettingsService : BaseApiService
    {
        private readonly IAccountChargeDao _accountChargeDao;
		private readonly IAccountDao _accountDao;
		private readonly ICommandBus _commandBus;
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly IServerSettings _serverSettings;
        private readonly Resources.Resources _resources;

		public BookingSettingsService(IAccountChargeDao accountChargeDao, IAccountDao accountDao, ICommandBus commandBus, IIBSServiceProvider ibsServiceProvider, IServerSettings serverSettings)
        {
            _accountChargeDao = accountChargeDao;
			_accountDao = accountDao;
            _commandBus = commandBus;
            _ibsServiceProvider = ibsServiceProvider;
            _serverSettings = serverSettings;
            _resources = new Resources.Resources(serverSettings);
        }

		public void Put(AccountUpdateRequest accountUpdateRequest)
        {
			var accountId = accountUpdateRequest.AccountId;
			var request = accountUpdateRequest.BookingSettingsRequest;

			var existingEmailAccountDetail = _accountDao.FindByEmail(request.Email);
			var currentAccountDetail = _accountDao.FindById(accountId);

			if (currentAccountDetail.Email != request.Email && currentAccountDetail.FacebookId.HasValue())
			{
				throw new HttpException((int)HttpStatusCode.BadRequest, _resources.Get("EmailChangeWithFacebookAccountErrorMessage"));
			}

			if (existingEmailAccountDetail != null && existingEmailAccountDetail.Email == request.Email && existingEmailAccountDetail.Id != accountId)
			{
			    throw new HttpListenerException((int)ErrorCode.EmailAlreadyUsed, _resources.Get("EmailUsedMessage"));
			}

            var countryCode = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(request.Country));

            if (PhoneHelper.IsPossibleNumber(countryCode, request.Phone))
            {
                request.Phone = PhoneHelper.GetDigitsFromPhoneNumber(request.Phone);
            }
            else
            {
                throw new HttpException((int)HttpStatusCode.InternalServerError,string.Format(_resources.Get("PhoneNumberFormat")), new Exception(countryCode.GetPhoneExample()));
            }

            var isChargeAccountEnabled = _serverSettings.GetPaymentSettings().IsChargeAccountPaymentEnabled;

            // Validate account number if charge account is enabled and account number is set.
            if (isChargeAccountEnabled && !string.IsNullOrWhiteSpace(request.AccountNumber))
            {
                if (!request.CustomerNumber.HasValue())
                {
                    throw new HttpException((int)HttpStatusCode.Forbidden, ErrorCode.AccountCharge_InvalidAccountNumber.ToString());
                }

                // Validate locally that the account exists
                var account = _accountChargeDao.FindByAccountNumber(request.AccountNumber);
                if (account == null)
                {
                    throw new HttpException((int)HttpStatusCode.Forbidden, ErrorCode.AccountCharge_InvalidAccountNumber.ToString());
                }

                // Validate with IBS to make sure the account/customer is still active
                var ibsChargeAccount = _ibsServiceProvider.ChargeAccount().GetIbsAccount(request.AccountNumber, request.CustomerNumber);
                if (!ibsChargeAccount.IsValid())
                {
                    throw new HttpException((int)HttpStatusCode.Forbidden, ErrorCode.AccountCharge_InvalidAccountNumber.ToString());
                }
            }

            var command = new UpdateBookingSettings();
            Mapper.Map(request, command);

			command.AccountId = accountId;

            _commandBus.Send(command);
        }
    }
}