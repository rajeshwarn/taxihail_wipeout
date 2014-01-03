#region

using System;
using System.IO;
using System.Net;
using System.Reflection;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Email;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services
{
    public class ConfirmAccountService : Service
    {
        private readonly IAccountDao _accountDao;
        private readonly ICommandBus _commandBus;
        private readonly IConfigurationManager _configurationManager;
        private readonly ITemplateService _templateService;

        public ConfirmAccountService(ICommandBus commandBus, IAccountDao accountDao, ITemplateService templateService,
            IConfigurationManager configurationManager)
        {
            _accountDao = accountDao;
            _templateService = templateService;
            _configurationManager = configurationManager;
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
            if (account == null) throw new HttpError(HttpStatusCode.NotFound, "Not Found");

            _commandBus.Send(new ConfirmAccount
            {
                AccountId = account.Id,
                ConfimationToken = request.ConfirmationToken
            });

            // Determine the root path to the app 
            var root = ApplicationPathResolver.GetApplicationPath(RequestContext);

            var template = _templateService.Find("AccountConfirmationSuccess");
            var templateData = new
            {
                ApplicationName = _configurationManager.GetSetting("TaxiHail.ApplicationName"),
                RootUrl = root,
                AccentColor = _configurationManager.GetSetting("TaxiHail.AccentColor")
            };
            var body = _templateService.Render(template, templateData);
            return new HttpResult(body, ContentType.Html);
        }
    }
}