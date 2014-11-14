using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Configuration;
using Newtonsoft.Json;
using ServiceStack.CacheAccess;
using ServiceStack.ServiceInterface;
using ServiceStack.Text.Json;

namespace apcurium.MK.Web.Areas.AdminTH.Controllers
{
    public class ImportChargeAccountController : ServiceStackController
    {
        private readonly IServerSettings _serverSettings;
        private readonly string _applicationKey;
        private AdministrationServiceClient _client;
        // GET: AdminTH/ImportChargeAccount
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
                
                if (HasData())
                { 
                    return CheckForImportedAccounts();
                }

                var response = _client.GetNewChargeAccounts().ToList();
                return View(response);
            }

            return Redirect(BaseUrl);
        }

        public ActionResult CheckForImportedAccounts()
        {

            var importedAccountsJson = ImportedAccounts;
            var importedAccountsList = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(importedAccountsJson);
                
            var report = _client.ImportAccounts(new IbsChargeAccountImportRequest()
            {
                Accounts = importedAccountsList
            });

            var response =
                _client.GetNewChargeAccounts()
                    .Where(x => importedAccountsList.All(y => y.Key != x.AccountNumber))
                    .ToList();

            ViewBag.Report = report.ReportLines;

            return View(response);
        }

        private bool HasData()
        {
            return (Context != null && ImportedAccounts != null);
        }

        private string ImportedAccounts
        {
            get
            {
                return Context.HttpContext.Request["ImportedAccounts"];    
            }
        }
    }
}