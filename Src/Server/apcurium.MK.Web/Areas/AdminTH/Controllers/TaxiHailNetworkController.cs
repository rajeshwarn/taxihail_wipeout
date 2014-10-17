using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using CustomerPortal.Client;
using CustomerPortal.Contract.Resources;
using ServiceStack.CacheAccess;

namespace apcurium.MK.Web.Areas.AdminTH.Controllers
{
    public class TaxiHailNetworkController : ServiceStackController
    {
        private readonly ITaxiHailNetworkServiceClient _taxiHailNetworkService;
        private readonly ILogger _logger;
        private readonly string _applicationKey;

        // GET: AdminTH/TaxiHailNetwork
        public TaxiHailNetworkController(
            ICacheClient cache,
            IServerSettings serverSettings,
            ITaxiHailNetworkServiceClient taxiHailNetworkService,
            ILogger logger) : base(cache,serverSettings)
        {
            _taxiHailNetworkService = taxiHailNetworkService;
            _logger = logger;

            _applicationKey = serverSettings.ServerData.TaxiHail.ApplicationKey;
        }

        public async Task<ActionResult> Index()
        {
            if (AuthSession.IsAuthenticated)
            {
                var response = await _taxiHailNetworkService.GetNetworkCompanyPreferences(_applicationKey);

                return View(response);
            }

            _logger.LogMessage("User not authenticated, redirecting to home page.");

            return Redirect(BaseUrl);
        }

        [HttpPost]
        public async Task<JsonResult> Index(FormCollection form)
        {
            if (ModelState.IsValid)
            {
                var response = await  _taxiHailNetworkService.GetNetworkCompanyPreferences(_applicationKey);

                var preferences = new List<CompanyPreference>();
                foreach (var companyPreference in response)
                {
                    var canAccept = form["acceptKey_" + companyPreference.CompanyKey].Contains("true");
                    var canDispatch = form["dispatchKey_" + companyPreference.CompanyKey].Contains("true");
                    preferences.Add(new CompanyPreference
                    {
                        CompanyKey = form["idKey_" + companyPreference.CompanyKey], 
                        CanAccept = canAccept, 
                        CanDispatch = canDispatch
                    });

                }

                await _taxiHailNetworkService.SetNetworkCompanyPreferences(_applicationKey, preferences.ToArray());

                 return Json(new { Success = true, Message = "Changes Saved" });
            }

            return Json(new { Success = false, Message = "Error" });
        }
    }
}