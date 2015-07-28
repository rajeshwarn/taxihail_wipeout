#region

using System;
using System.IO;
using System.Net;
using System.Reflection;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Email;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class ConfirmAccountService : Service
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

        public object Get(ConfirmAccountRequest request)
        {
            var account = _accountDao.FindByEmail(request.EmailAddress);
            if (account == null)
            {
                throw new HttpError(HttpStatusCode.NotFound, "No account matching this email address");
            }

            if (request.IsSMSConfirmation.HasValue && request.IsSMSConfirmation.Value)
            {
                if (account.ConfirmationToken != request.ConfirmationToken)
                {
                    throw new HttpError(ErrorCode.CreateAccount_InvalidConfirmationToken.ToString());
                }

                _commandBus.Send(new ConfirmAccount
                {
                    AccountId = account.Id,
                    ConfimationToken = request.ConfirmationToken
                });
                return new HttpResult(HttpStatusCode.OK);
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
                return new HttpResult(body, ContentType.Html);
            }
        }

        public void Get(ConfirmationCodeRequest request)
        {
            var account = _accountDao.FindByEmail(request.Email);

            if (account == null)
            {
                throw new HttpError(HttpStatusCode.NotFound, "No account matching this email address");
            }
                
            if (!_serverSettings.ServerData.AccountActivationDisabled)
            {
                if (_serverSettings.ServerData.SMSConfirmationEnabled)
                {
                    _commandBus.Send(new SendAccountConfirmationSMS
                    {
                        ClientLanguageCode = account.Language,
                        Code = account.ConfirmationToken,
                        CountryCode = account.Settings.Country,
                        PhoneNumber = account.Settings.Phone
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