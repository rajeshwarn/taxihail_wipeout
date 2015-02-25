using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.Attributes;
using CustomerPortal.Client;
using CustomerPortal.Contract.Resources;
using CustomerPortal.Contract.Response;
using ServiceStack.CacheAccess;

namespace apcurium.MK.Web.Areas.AdminTH.Controllers
{
    [AuthorizationRequired(RoleName.Admin)]
    public class TaxiHailNetworkController : ServiceStackController
    {
        private readonly IServerSettings _serverSettings;
        private readonly ITaxiHailNetworkServiceClient _taxiHailNetworkService;
        private readonly ConfigurationsService _configurationsService;

        // GET: AdminTH/TaxiHailNetwork
        public TaxiHailNetworkController(ICacheClient cache, IServerSettings serverSettings, ITaxiHailNetworkServiceClient taxiHailNetworkService, ConfigurationsService configurationsService) 
            : base(cache, serverSettings)
        {
            _serverSettings = serverSettings;
            _taxiHailNetworkService = taxiHailNetworkService;
            _configurationsService = configurationsService;
        }

        public async Task<ActionResult> Index()
        {
            var localCompaniesPreferences = await _taxiHailNetworkService.GetNetworkCompanyPreferences(_serverSettings.ServerData.TaxiHail.ApplicationKey);
            var roamingCompaniesPreferences = await _taxiHailNetworkService.GetRoamingCompanyPreferences(_serverSettings.ServerData.TaxiHail.ApplicationKey);

            if (localCompaniesPreferences == null || roamingCompaniesPreferences == null)
            {
                return View();
            }

            var companies = new Dictionary<string, List<CompanyPreferenceResponse>>
            {
                {"Local", localCompaniesPreferences}
            };

            foreach (var market in roamingCompaniesPreferences.Keys)
            {
                companies.Add(market, roamingCompaniesPreferences[market]);
            }

            return View(companies);
        }

        [HttpPost]
        public async Task<JsonResult> Index(FormCollection form)
        {
            if (ModelState.IsValid)
            {
                var localCompaniesPreferences = await _taxiHailNetworkService.GetNetworkCompanyPreferences(_serverSettings.ServerData.TaxiHail.ApplicationKey);
                var roamingCompaniesPreferences = await _taxiHailNetworkService.GetRoamingCompanyPreferences(_serverSettings.ServerData.TaxiHail.ApplicationKey);

                var companiesPreferences = new Dictionary<string, List<CompanyPreferenceResponse>>
                {
                    {"Local", localCompaniesPreferences}
                };
                foreach (var roamingCompaniesPreference in roamingCompaniesPreferences)
                {
                    companiesPreferences.Add(roamingCompaniesPreference.Key, roamingCompaniesPreference.Value);
                }

                var preferences = new List<CompanyPreference>();

                foreach (var market in companiesPreferences.Keys)
                {
                    var marketCompaniesPreferences = companiesPreferences[market];

                    for (var i = 0; i < marketCompaniesPreferences.Count; i++)
                    {
                        var orderKey = string.Format("orderKey_{0}",
                            marketCompaniesPreferences[i].CompanyPreference.CompanyKey);

                        var order = 0;
                        if (form.AllKeys.Contains(orderKey))
                        {
                            order = form[orderKey] == string.Empty ? i : int.Parse(form[orderKey]);
                        }

                        preferences.Add(new CompanyPreference
                        {
                            CompanyKey = form["idKey_" + marketCompaniesPreferences[i].CompanyPreference.CompanyKey],
                            CanAccept = form["acceptKey_" + marketCompaniesPreferences[i].CompanyPreference.CompanyKey].Contains("true"),
                            CanDispatch = form["dispatchKey_" + marketCompaniesPreferences[i].CompanyPreference.CompanyKey].Contains("true"),
                            Order = order
                        });
                    }
                }

                await _taxiHailNetworkService.SetNetworkCompanyPreferences(
                        _serverSettings.ServerData.TaxiHail.ApplicationKey, 
                        preferences.OrderBy(thn => thn.Order.HasValue)
                                   .ThenBy(thn => thn.Order.GetValueOrDefault())
                        .ToArray());

                SaveNetworkSettingsIfNecessary();

                return Json(new { Success = true, Message = "Changes Saved" });
            }

            return Json(new { Success = false, Message = "All fields are required" });
        }

        private  void SaveNetworkSettingsIfNecessary()
        {
            if(!_serverSettings.ServerData.Network.Enabled)
            {
                _configurationsService.Post(new ConfigurationsRequest
                {
                    AppSettings = new Dictionary<string, string> { { "Network.Enabled", "true" } }
                });
            }
        }
    }
}