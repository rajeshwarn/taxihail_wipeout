using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using apcurium.MK.Common.Configuration;
using CustomerPortal.Client.impl;
using CustomerPortal.Contract.Resources;
using ServiceStack.CacheAccess;

namespace apcurium.MK.Web.Areas.AdminTH.Controllers
{
    public class TaxiHailNetworkController : ServiceStackController
    {
        private readonly IServerSettings _serverSettings;
        private readonly TaxiHailNetworkServiceClient _taxiHailNetworkService;

        // GET: AdminTH/TaxiHailNetwork
        public TaxiHailNetworkController(ICacheClient cache,IServerSettings serverSettings) : base(cache)
        {
            _serverSettings = serverSettings;
            _taxiHailNetworkService=new TaxiHailNetworkServiceClient();
        }

        public ActionResult Index()
        {
            if (AuthSession.IsAuthenticated)
            {
                var response =_taxiHailNetworkService.GetOverlapingCompaniesPreferences(_serverSettings.ServerData.TaxiHail.ApplicationKey);

                return View(response);
            }

            return new HttpUnauthorizedResult();
        }

        [HttpPost]
        public ActionResult Index(FormCollection form)
        {
            if (AuthSession.IsAuthenticated)
            {
                var response = _taxiHailNetworkService.GetOverlapingCompaniesPreferences("tes1");
                var preferences= (from companyPreference in response
                    let canAccept = form["acceptKey_" + companyPreference.CompanyId].Contains("true")
                    let canDispatch = form["dispatchKey_" + companyPreference.CompanyId].Contains("true")
                    select new CompanyPreference
                    {
                        CompanyId = form["idKey_" + companyPreference.CompanyId], CanAccept = canAccept, CanDispatch = canDispatch
                    }).ToList();

                _taxiHailNetworkService.SetOverlapingCompaniesPreferences("tes1",preferences.ToArray());

                 return View(preferences);
            }

            return new HttpUnauthorizedResult();
        }


    }
}