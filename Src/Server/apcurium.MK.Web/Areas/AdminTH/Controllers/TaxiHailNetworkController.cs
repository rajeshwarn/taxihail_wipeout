using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
                return View();
            }

            return new HttpUnauthorizedResult();
        }
    }
}