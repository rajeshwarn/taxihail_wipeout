#region

using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Routing;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Email;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;
using apcurium.MK.Common;
using apcurium.MK.Common.Helpers;


#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class ConfirmAccountService : BaseApiService
    {
        private readonly IAccountDao _accountDao;
        private readonly ICommandBus _commandBus;
        private readonly IServerSettings _serverSettings;
        private readonly ITemplateService _templateService;

        public ConfirmAccountService(ICommandBus commandBus, IAccountDao accountDao, ITemplateService templateService,
            IServerSettings serverSettings)
        {
            _accountDao = accountDao;
            _templateService = templateService;
            _serverSettings = serverSettings;
            _commandBus = commandBus;
        }

        public static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public string Get(ConfirmAccountRequest request)
        {
            var account = _accountDao.FindByEmail(request.EmailAddress);
            if (account == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "No account matching this email address");
            }

            if (request.IsSMSConfirmation.HasValue && request.IsSMSConfirmation.Value)
            {
                if (account.ConfirmationToken != request.ConfirmationToken)
                {
                    throw new HttpException(ErrorCode.CreateAccount_InvalidConfirmationToken.ToString());
                }

                _commandBus.Send(new ConfirmAccount
                {
                    AccountId = account.Id,
                    ConfimationToken = request.ConfirmationToken
                });

                return string.Empty;
            }
            else
            {
                _commandBus.Send(new ConfirmAccount
                {
                    AccountId = account.Id,
                    ConfimationToken = request.ConfirmationToken
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
                return body;
            }
        }

        public void Get(ConfirmationCodeRequest request)
        {
            var account = _accountDao.FindByEmail(request.Email);

			if (account == null)
			{
				throw new HttpException((int)HttpStatusCode.NotFound, "No account matching this email address");
			}

            if (!_serverSettings.ServerData.AccountActivationDisabled)
            {
                if (_serverSettings.ServerData.SMSConfirmationEnabled)
                {
					var countryCodeForSms = account.Settings.Country;
					var phoneNumberForSms = account.Settings.Phone;

					var countryCodeFromRequest = CountryCode.GetCountryCodeByIndex(CountryCode.GetCountryCodeIndexByCountryISOCode(request.CountryCode));

					if (countryCodeFromRequest.IsValid()
						&& request.PhoneNumber.HasValue()
                        && PhoneHelper.IsPossibleNumber(countryCodeFromRequest, request.PhoneNumber)
						&& (account.Settings.Country.Code != countryCodeFromRequest.CountryISOCode.Code || account.Settings.Phone != request.PhoneNumber))
					{
						countryCodeForSms = countryCodeFromRequest.CountryISOCode;
						phoneNumberForSms = request.PhoneNumber;

						var updateBookingSettings = new UpdateBookingSettings()
						{
							AccountId = account.Id,
							Email = account.Email,
							Name = account.Name,
							Country = countryCodeFromRequest.CountryISOCode,
							Phone = request.PhoneNumber,
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
        }
    }
}