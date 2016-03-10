using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using CustomerPortal.Web.Areas.Admin.Models;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Entities.Network;
using MongoRepository;

namespace CustomerPortal.Web.Areas.Admin.Controllers
{
    public class TaxiHailNetworkSettingsController : TaxiHailNetworkSettingsControllerBase
    {
        public TaxiHailNetworkSettingsController()
            : base(new MongoRepository<TaxiHailNetworkSettings>())
        {
        }

        public ActionResult Index(string id)
        {
            var allMarkets = new MongoRepository<Market>().OrderBy(x => x.Name).ToList();

            var company = new MongoRepository<Company>().First(x => x.Id == id);
            if (company != null)
            {
                var network = Repository.FirstOrDefault(x => x.Id == company.CompanyKey);
                if (network == null)
                {
                    network = new TaxiHailNetworkSettings
                    {
                        Id = company.CompanyKey
                    };
                }
                return PartialView(new TaxiHailNetworkSettingsModel { AvailableMarkets = allMarkets, TaxiHailNetworkSettings = network });
            }
            return PartialView(new TaxiHailNetworkSettingsModel { AvailableMarkets = allMarkets, TaxiHailNetworkSettings = new TaxiHailNetworkSettings { Id = id } });
        }

        [HttpPost]
        public JsonResult Index(TaxiHailNetworkSettingsModel model, string networkId = null)
        {
            if (ModelState.IsValid)
            {
                if (networkId != null)
                {
                    model.TaxiHailNetworkSettings.Id = networkId;

                    // fetch existing entry to put data not included in form (Preferences property for now)
                    var existing = Repository.GetById(networkId);
                    if (existing != null)
                    {
                        model.TaxiHailNetworkSettings.Preferences = existing.Preferences;
                    }
                }

                if (!string.IsNullOrEmpty(model.TaxiHailNetworkSettings.BlackListedFleetIds))
                {
                    var blackList = Regex.Replace(model.TaxiHailNetworkSettings.BlackListedFleetIds, @"\s+", string.Empty).Split(',');

                    if (blackList.Contains(model.TaxiHailNetworkSettings.FleetId.ToString()))
                    {
                        return Json(new { Success = false, Message = "You can not put your own fleet in the black list" });
                    }
                }

                if (!model.TaxiHailNetworkSettings.IsInNetwork)
                {
                    foreach (var taxiHailNetworkSettings in Repository)
                    {
                        var preference = taxiHailNetworkSettings.Preferences.FirstOrDefault(x => x.CompanyKey == model.TaxiHailNetworkSettings.Id);
                        if (preference != null)
                        {
                            taxiHailNetworkSettings.Preferences.Remove(preference);
                            Repository.Update(taxiHailNetworkSettings);
                        }
                    }
                }
                
                Repository.Update(model.TaxiHailNetworkSettings);
                return Json(new { Success = true, Message = "Changes Saved" });
            }

            return Json(new { Success = false, Message = "Please fill all the required fields" });
        }
    }
}
