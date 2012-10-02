using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
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
            var httpRequest = RequestContext.Get<IHttpRequest>();
            var root = new Uri(RequestContext.AbsoluteUri).GetLeftPart(UriPartial.Authority); ;
            var aspNetRequest = httpRequest.OriginalRequest as HttpRequest;
            if (aspNetRequest != null)
            {
                // We are in IIS
                //The ApplicationVirtualPath property always returns "/" as the first character of the returned value.
                //If the application is located in the root of the Web site, the return value is just "/".
                if (HostingEnvironment.ApplicationVirtualPath.Length > 1)
                {
                    root += HostingEnvironment.ApplicationVirtualPath;
                }
            }
            else
            {
                // We are probably in a test environment, using HttpListener
                // We Assume there is no virtual path
            }

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
