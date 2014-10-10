using CustomerPortal.Web.Android;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Entities.Network;
using MongoRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

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
            var network = Repository.FirstOrDefault(x => x.CompanyId == id);
            
            if (network == null)
            {
                var companyRepo = new MongoRepository<Company>().FirstOrDefault(x => x.CompanyKey == id);

                if (companyRepo != null)
                    network = new TaxiHailNetworkSettings
                    {
                        CompanyId = companyRepo.CompanyKey,
                        Id = companyRepo.CompanyKey
                    };
            }
         
            return PartialView(network);
        }

        [HttpPost]
        public ActionResult Index(TaxiHailNetworkSettings model)
        {
            Repository.Update(model);

            return RedirectToAction("Index", "Home", new { area = "Customer", companyId = model.CompanyId });
        }

    }
}
