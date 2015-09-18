#region

using System;
using System.Net;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using AutoMapper;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Common;
using apcurium.MK.Common.Helpers;
using apcurium.MK.Booking.ReadModel;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class BookingSettingsService : Service
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

        public object Put(BookingSettingsRequest request)
        {
			Guid accountID = Guid.Parse(request.AccountId);

			AccountDetail accountDetail = _accountDao.FindByEmail(request.Email);

			if (accountDetail != null && accountDetail.Id != accountID && accountDetail.Email == request.Email)
			{
				throw new HttpError(_resources.Get("EmailUsedMessage"));
			}

            CountryCode countryCode = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(request.Country));

            if (countryCode.IsNumberPossible(request.Phone))
            {
                request.Phone = PhoneHelper.GetDigitsFromPhoneNumber(request.Phone);
            }
            else
            {
                throw new HttpError(string.Format(_resources.Get("PhoneNumberFormat"), countryCode.GetPhoneExample()));
            }

            var isChargeAccountEnabled = _serverSettings.GetPaymentSettings().IsChargeAccountPaymentEnabled;

            // Validate account number if charge account is enabled and account number is set.
            if (isChargeAccountEnabled && !string.IsNullOrWhiteSpace(request.AccountNumber))
            {
                if (!request.CustomerNumber.HasValue())
                {
                    throw new HttpError(HttpStatusCode.Forbidden, ErrorCode.AccountCharge_InvalidAccountNumber.ToString());
                }

                // Validate locally that the account exists
                var account = _accountChargeDao.FindByAccountNumber(request.AccountNumber);
                if (account == null)
                {
                    throw new HttpError(HttpStatusCode.Forbidden, ErrorCode.AccountCharge_InvalidAccountNumber.ToString());
                }

                // Validate with IBS to make sure the account/customer is still active
                var ibsChargeAccount = _ibsServiceProvider.ChargeAccount().GetIbsAccount(request.AccountNumber, request.CustomerNumber);
                if (!ibsChargeAccount.IsValid())
                {
                    throw new HttpError(HttpStatusCode.Forbidden, ErrorCode.AccountCharge_InvalidAccountNumber.ToString());
                }
            }

            var command = new UpdateBookingSettings();
            Mapper.Map(request, command);

            command.AccountId = new Guid(this.GetSession().UserAuthId);

            _commandBus.Send(command);

            return new HttpResult(HttpStatusCode.OK);
        }
    }
}