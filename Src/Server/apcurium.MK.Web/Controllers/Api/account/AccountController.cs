using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Resources;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Helpers;
using apcurium.MK.Web.Security;
using AutoMapper;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Controllers.Api.Account
{
    [RoutePrefix("account")]
    public class AccountController : BaseApiController
    {
        private readonly IAccountDao _accountDao;
        private readonly Resources _resources;
        private readonly IServerSettings _serverSettings;
        private readonly IAccountChargeDao _accountChargeDao;
        private readonly ICommandBus _commandBus;
        private readonly IIBSServiceProvider _ibsServiceProvider;

        public AccountController(IAccountDao accountDao, IServerSettings serverSettings, IAccountChargeDao accountChargeDao, ICommandBus commandBus, IIBSServiceProvider ibsServiceProvider)
        {
            _accountDao = accountDao;
            _serverSettings = serverSettings;
            _accountChargeDao = accountChargeDao;
            _commandBus = commandBus;
            _ibsServiceProvider = ibsServiceProvider;

            _resources = new Resources(serverSettings);
        }
        
        [HttpPut,Auth(Roles = new [] { Roles.Support }), Route("update/{accountId}")]
        public HttpResponseMessage AccountUpdate(Guid accountId, BookingSettingsRequest bookingSettings)
        {
            var existingEmailAccountDetail = _accountDao.FindByEmail(bookingSettings.Email);
            var currentAccountDetail = _accountDao.FindById(accountId);

            if (currentAccountDetail.Email != bookingSettings.Email && currentAccountDetail.FacebookId.HasValue())
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, _resources.Get("EmailChangeWithFacebookAccountErrorMessage"));
            }

            if (existingEmailAccountDetail != null && existingEmailAccountDetail.Email == bookingSettings.Email && existingEmailAccountDetail.Id != accountId)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, ErrorCode.EmailAlreadyUsed.ToString(), new Exception(_resources.Get("EmailUsedMessage")));
            }

            var countryCode = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(bookingSettings.Country));

            if (PhoneHelper.IsPossibleNumber(countryCode, bookingSettings.Phone))
            {
                bookingSettings.Phone = PhoneHelper.GetDigitsFromPhoneNumber(bookingSettings.Phone);
            }
            else
            {
                throw new HttpException(string.Format(_resources.Get("PhoneNumberFormat"), new Exception(countryCode.GetPhoneExample())));
            }

            var isChargeAccountEnabled = _serverSettings.GetPaymentSettings().IsChargeAccountPaymentEnabled;

            // Validate account number if charge account is enabled and account number is set.
            if (isChargeAccountEnabled && !string.IsNullOrWhiteSpace(bookingSettings.AccountNumber))
            {
                if (!bookingSettings.CustomerNumber.HasValue())
                {
                    throw new HttpException((int)HttpStatusCode.Forbidden, ErrorCode.AccountCharge_InvalidAccountNumber.ToString());
                }

                // Validate locally that the account exists
                var account = _accountChargeDao.FindByAccountNumber(bookingSettings.AccountNumber);
                if (account == null)
                {
                    throw new HttpException((int)HttpStatusCode.Forbidden, ErrorCode.AccountCharge_InvalidAccountNumber.ToString());
                }

                // Validate with IBS to make sure the account/customer is still active
                var ibsChargeAccount = _ibsServiceProvider.ChargeAccount().GetIbsAccount(bookingSettings.AccountNumber, bookingSettings.CustomerNumber);
                if (!ibsChargeAccount.IsValid())
                {
                    throw new HttpException((int)HttpStatusCode.Forbidden, ErrorCode.AccountCharge_InvalidAccountNumber.ToString());
                }
            }

            var command = new UpdateBookingSettings();
            Mapper.Map(bookingSettings, command);

            command.AccountId = accountId;

            _commandBus.Send(command);

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpPut, Route("bookingsettings")]
        public HttpResponseMessage AccountUpdate(BookingSettingsRequest request)
        {
            return AccountUpdate(GetSession().UserId, request);
        }
    }
}
