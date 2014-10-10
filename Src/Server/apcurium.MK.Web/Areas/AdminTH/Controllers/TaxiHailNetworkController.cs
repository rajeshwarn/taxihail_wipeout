using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using apcurium.MK.Web.Areas.AdminTH.Models;
using ServiceStack.CacheAccess;

namespace apcurium.MK.Web.Areas.AdminTH.Controllers
{
    public class TaxiHailNetworkController : ServiceStackController
    {
        // GET: AdminTH/TaxiHailNetwork
        public TaxiHailNetworkController(ICacheClient cache) : base(cache)
        {
        }

        public ActionResult Index()
        {
            if (AuthSession.IsAuthenticated)
            {
                var compamyPreferences = new List<CompanyPreference>
                {
                    new CompanyPreference{CanAccept = true,CanDispatch = true, CompanyId = "test1"}
                };
                return View(compamyPreferences);
            }

            return new HttpUnauthorizedResult();
        }
    }
}