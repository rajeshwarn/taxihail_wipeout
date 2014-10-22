using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using apcurium.MK.Common.Configuration;
using CustomerPortal.Client;
using CustomerPortal.Client.Impl;
using CustomerPortal.Contract.Resources;
using CustomerPortal.Contract.Response;
using ServiceStack.CacheAccess;

namespace apcurium.MK.Web.Areas.AdminTH.Controllers
{
    public class TaxiHailNetworkController : ServiceStackController
    {
        private readonly ITaxiHailNetworkServiceClient _taxiHailNetworkService;
        private readonly string _applicationKey;

        // GET: AdminTH/TaxiHailNetwork
        public TaxiHailNetworkController(ICacheClient cache,IServerSettings serverSettings,ITaxiHailNetworkServiceClient taxiHailNetworkService ) : base(cache,serverSettings)
        {
            _taxiHailNetworkService = taxiHailNetworkService;

            _applicationKey = serverSettings.ServerData.TaxiHail.ApplicationKey;
        }

        public async Task<ActionResult> Index()
        {
            if (AuthSession.IsAuthenticated)
            {
                var response = await _taxiHailNetworkService.GetNetworkCompanyPreferences(_applicationKey);

                return View(response);
            }

               return Redirect(BaseUrl);
        }

        [HttpPost]
        public async Task<JsonResult> Index(FormCollection form)
        {
            if (ModelState.IsValid)
            {
                var companyPreferences = await _taxiHailNetworkService.GetNetworkCompanyPreferences(_applicationKey);

                var preferences = new List<CompanyPreference>();
                for (var i = 0; i < companyPreferences.Count; i++)
                {
                    int? order=null;
                    order = form["orderKey_" + companyPreferences[i].CompanyPreference.CompanyKey] == string.Empty ? i : int.Parse(form["orderKey_" + companyPreferences[i].CompanyPreference.CompanyKey]);
                    var canAccept = form["acceptKey_" + companyPreferences[i].CompanyPreference.CompanyKey].Contains("true");
                    var canDispatch = form["dispatchKey_" + companyPreferences[i].CompanyPreference.CompanyKey].Contains("true");
                    preferences.Add(new CompanyPreference
                    {
                        CompanyKey = form["idKey_" + companyPreferences[i].CompanyPreference.CompanyKey], 
                        CanAccept = canAccept, 
                        CanDispatch = canDispatch,
                        Order=order
                    });
                    

                }

                await _taxiHailNetworkService.SetNetworkCompanyPreferences(_applicationKey, preferences.OrderBy(x=>x.Order.HasValue).ThenBy(x=>x.Order.GetValueOrDefault()).ToArray());

                 return Json(new { Success = true, Message = "Changes Saved" });
            }

            return Json(new { Success = false, Message = "All fields are required" });
        }
    }
}