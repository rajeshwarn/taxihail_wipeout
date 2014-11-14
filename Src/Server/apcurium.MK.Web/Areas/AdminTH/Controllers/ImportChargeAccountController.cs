using System.Web.Mvc;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Common.Configuration;
using ServiceStack.CacheAccess;


namespace apcurium.MK.Web.Areas.AdminTH.Controllers
{
    public class ImportChargeAccountController : ServiceStackController
    {
        private readonly IServerSettings _serverSettings;
        private readonly string _applicationKey;
        private AdministrationServiceClient _client;
        
        public ImportChargeAccountController(ICacheClient cache, IServerSettings serverSettings) 
            : base(cache, serverSettings)
        {
            _serverSettings = serverSettings;
            _applicationKey = serverSettings.ServerData.TaxiHail.ApplicationKey;
        }

        public ActionResult Index()
        {
            if (AuthSession.IsAuthenticated)
            {
                _client = new AdministrationServiceClient(BaseUrlAPI, SessionID, null);
                return View(_client.ImportAccounts());
            }

            return Redirect(BaseUrl);
        }
    }
}