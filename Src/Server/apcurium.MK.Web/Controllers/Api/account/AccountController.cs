using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Web;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Email;
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
        private readonly ITemplateService _templateService;

        public AccountController(IAccountDao accountDao, IServerSettings serverSettings,
            IAccountChargeDao accountChargeDao, ICommandBus commandBus, IIBSServiceProvider ibsServiceProvider,
            ITemplateService templateService)
        {
            _accountDao = accountDao;
            _serverSettings = serverSettings;
            _accountChargeDao = accountChargeDao;
            _commandBus = commandBus;
            _ibsServiceProvider = ibsServiceProvider;
            _templateService = templateService;

            _resources = new Resources(serverSettings);
        }

        [HttpPut, Auth(Roles = new[] {Roles.Support}), Route("update/{accountId}")]
        public HttpResponseMessage AccountUpdate(Guid accountId, BookingSettingsRequest bookingSettings)
        {
            var existingEmailAccountDetail = _accountDao.FindByEmail(bookingSettings.Email);
            var currentAccountDetail = _accountDao.FindById(accountId);

            if (currentAccountDetail.Email != bookingSettings.Email && currentAccountDetail.FacebookId.HasValue())
            {
                throw new HttpException((int) HttpStatusCode.BadRequest,
                    _resources.Get("EmailChangeWithFacebookAccountErrorMessage"));
            }

            if (existingEmailAccountDetail != null && existingEmailAccountDetail.Email == bookingSettings.Email &&
                existingEmailAccountDetail.Id != accountId)
            {
                throw new HttpException((int) HttpStatusCode.BadRequest, ErrorCode.EmailAlreadyUsed.ToString(),
                    new Exception(_resources.Get("EmailUsedMessage")));
            }

            var countryCode =
                CountryCode.GetCountryCodeByIndex(
                    CountryCode.GetCountryCodeIndexByCountryISOCode(bookingSettings.Country));

            if (PhoneHelper.IsPossibleNumber(countryCode, bookingSettings.Phone))
            {
                bookingSettings.Phone = PhoneHelper.GetDigitsFromPhoneNumber(bookingSettings.Phone);
            }
            else
            {
                throw new HttpException(string.Format(_resources.Get("PhoneNumberFormat"),
                    new Exception(countryCode.GetPhoneExample())));
            }

            var isChargeAccountEnabled = _serverSettings.GetPaymentSettings().IsChargeAccountPaymentEnabled;

            // Validate account number if charge account is enabled and account number is set.
            if (isChargeAccountEnabled && !string.IsNullOrWhiteSpace(bookingSettings.AccountNumber))
            {
                if (!bookingSettings.CustomerNumber.HasValue())
                {
                    throw new HttpException((int) HttpStatusCode.Forbidden,
                        ErrorCode.AccountCharge_InvalidAccountNumber.ToString());
                }

                // Validate locally that the account exists
                var account = _accountChargeDao.FindByAccountNumber(bookingSettings.AccountNumber);
                if (account == null)
                {
                    throw new HttpException((int) HttpStatusCode.Forbidden,
                        ErrorCode.AccountCharge_InvalidAccountNumber.ToString());
                }

                // Validate with IBS to make sure the account/customer is still active
                var ibsChargeAccount = _ibsServiceProvider.ChargeAccount()
                    .GetIbsAccount(bookingSettings.AccountNumber, bookingSettings.CustomerNumber);
                if (!ibsChargeAccount.IsValid())
                {
                    throw new HttpException((int) HttpStatusCode.Forbidden,
                        ErrorCode.AccountCharge_InvalidAccountNumber.ToString());
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

        [HttpGet, Route("getconfirmationcode/{email}/{countryCode}/{phoneNumber}")]
        public HttpResponseMessage GetConfirmationCode(string email, string countryCode, string phoneNumber)
        {
            var account = _accountDao.FindByEmail(email);

            if (account == null)
            {
                throw new HttpException((int) HttpStatusCode.NotFound, "No account matching this email address");
            }

            if (!_serverSettings.ServerData.AccountActivationDisabled)
            {
                if (_serverSettings.ServerData.SMSConfirmationEnabled)
                {
                    var countryCodeForSms = account.Settings.Country;
                    var phoneNumberForSms = account.Settings.Phone;

                    var countryCodeFromRequest =
                        CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(countryCode));

                    if (countryCodeFromRequest.IsValid()
                        && phoneNumber.HasValue()
                        && PhoneHelper.IsPossibleNumber(countryCodeFromRequest, phoneNumber)
                        &&
                        (account.Settings.Country.Code != countryCodeFromRequest.CountryISOCode.Code ||
                         account.Settings.Phone != phoneNumber))
                    {
                        countryCodeForSms = countryCodeFromRequest.CountryISOCode;
                        phoneNumberForSms = phoneNumber;

                        var updateBookingSettings = new UpdateBookingSettings()
                        {
                            AccountId = account.Id,
                            Email = account.Email,
                            Name = account.Name,
                            Country = countryCodeFromRequest.CountryISOCode,
                            Phone = phoneNumber,
                            Passengers = account.Settings.Passengers,
                            VehicleTypeId = account.Settings.VehicleTypeId,
                            ChargeTypeId = account.Settings.ChargeTypeId,
                            ProviderId = account.Settings.ProviderId,
                            NumberOfTaxi = account.Settings.NumberOfTaxi,
                            AccountNumber = account.Settings.AccountNumber,
                            CustomerNumber = account.Settings.CustomerNumber,
                            DefaultTipPercent = account.DefaultTipPercent,
                            PayBack = account.Settings.PayBack
                        };

                        _commandBus.Send(updateBookingSettings);
                    }

                    _commandBus.Send(new SendAccountConfirmationSMS
                    {
                        ClientLanguageCode = account.Language,
                        Code = account.ConfirmationToken,
                        CountryCode = countryCodeForSms,
                        PhoneNumber = phoneNumberForSms
                    });
                }
                else
                {
                    _commandBus.Send(new SendAccountConfirmationEmail
                    {
                        ClientLanguageCode = account.Language,
                        EmailAddress = account.Email,
                        ConfirmationUrl =
                            new Uri(string.Format("/api/account/confirm/{0}/{1}", account.Email,
                                account.ConfirmationToken), UriKind.Relative)
                    });
                }
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        [HttpGet, Route("confirm/{emailAddress}/{confirmationToken}/{isSmsConfirmation:bool?}")]
        public HttpResponseMessage ConfirmAccount(string emailAddress, string confirmationToken, bool? isSmsConfirmation)
        {
            var account = _accountDao.FindByEmail(emailAddress);
            if (account == null)
            {
                throw new HttpException((int) HttpStatusCode.NotFound, "No account matching this email address");
            }

            if (isSmsConfirmation ?? false)
            {
                if (account.ConfirmationToken != confirmationToken)
                {
                    throw new HttpException(ErrorCode.CreateAccount_InvalidConfirmationToken.ToString());
                }

                _commandBus.Send(new ConfirmAccount
                {
                    AccountId = account.Id,
                    ConfimationToken = confirmationToken
                });
                return new HttpResponseMessage(HttpStatusCode.OK);
            }

            _commandBus.Send(new ConfirmAccount
            {
                AccountId = account.Id,
                ConfimationToken = confirmationToken
            });

            // Determine the root path to the app 
            var root = ApplicationPathResolver.GetApplicationPath(RequestContext);

            var template = _templateService.Find("AccountConfirmationSuccess", account.Language);
            var templateData = new
            {
                RootUrl = root,
                PackageName = _serverSettings.ServerData.GCM.PackageName,
                ApplicationName = _serverSettings.ServerData.TaxiHail.ApplicationName,
                AccentColor = _serverSettings.ServerData.TaxiHail.AccentColor
            };

            var body = _templateService.Render(template, templateData);

            var stringContent = new StringContent(body);

            stringContent.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Text.Html);

            return new HttpResponseMessage
            {
                Content = stringContent
            };
        }
    }
}
