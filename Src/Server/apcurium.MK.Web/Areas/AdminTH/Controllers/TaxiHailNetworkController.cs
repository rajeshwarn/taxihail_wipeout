using System.Linq;
using System.Web.Mvc;
using apcurium.MK.Common.Configuration;
using CustomerPortal.Client.Impl;
using CustomerPortal.Contract.Resources;
using ServiceStack.CacheAccess;

namespace apcurium.MK.Web.Areas.AdminTH.Controllers
{
    public class TaxiHailNetworkController : ServiceStackController
    {
        private readonly TaxiHailNetworkServiceClient _taxiHailNetworkService;
        private readonly string _applicationKey;

        // GET: AdminTH/TaxiHailNetwork
        public TaxiHailNetworkController(ICacheClient cache,IServerSettings serverSettings) : base(cache)
        {
            _taxiHailNetworkService=new TaxiHailNetworkServiceClient();

            _applicationKey = serverSettings.ServerData.TaxiHail.ApplicationKey;
        }

        public ActionResult Index()
        {
            if (AuthSession.IsAuthenticated)
            {
                var response = _taxiHailNetworkService.GetOverlapingCompaniesPreferences(_applicationKey);

                return View(response);
            }

            return new HttpUnauthorizedResult();
        }

        [HttpPost]
        public JsonResult Index(FormCollection form)
        {
            if (ModelState.IsValid)
            {
                var response = _taxiHailNetworkService.GetOverlapingCompaniesPreferences(_applicationKey);
                var preferences = (from companyPreference in response
                    let canAccept = form["acceptKey_" + companyPreference.CompanyKey].Contains("true")
                    let canDispatch = form["dispatchKey_" + companyPreference.CompanyKey].Contains("true")
                    select new CompanyPreference
                    {
                        CompanyKey = form["idKey_" + companyPreference.CompanyKey], CanAccept = canAccept, CanDispatch = canDispatch
                    }).ToList();

                _taxiHailNetworkService.SetOverlapingCompaniesPreferences(_applicationKey, preferences.ToArray());

                 return Json(new { Success = true, Message = "Changes Saved" });
            }

            return Json(new { Success = false, Message = "Error" });
        }
    }
}