using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Helpers;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Email;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Api.Services
{
    public class ConfirmAccountService : RestServiceBase<ConfirmAccountRequest>
    {
        private readonly IAccountDao _accountDao;
        private readonly ITemplateService _templateService;
        private readonly IConfigurationManager _configurationManager;
        private readonly ICommandBus _commandBus;

        public ConfirmAccountService(ICommandBus commandBus, IAccountDao accountDao, ITemplateService templateService, IConfigurationManager configurationManager)
        {
            _accountDao = accountDao;
            _templateService = templateService;
            _configurationManager = configurationManager;
            _commandBus = commandBus;
        }

        public override object OnGet(ConfirmAccountRequest request)
        {
            var account = _accountDao.FindByEmail(request.EmailAddress);
            if(account == null) throw new HttpError(HttpStatusCode.NotFound, "Not Found");

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

        static public string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}
