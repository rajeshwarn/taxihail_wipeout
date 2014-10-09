using CustomerPortal.Web.Entities.Network;
using MongoRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CustomerPortal.Web.Areas.Admin.Controllers
{
    public class TaxiHailNetworkSettingsControllerBase : Controller
    {
        public TaxiHailNetworkSettingsControllerBase(IRepository<TaxiHailNetworkSettings> repository)
        {
            Repository = repository;
        }

        protected IRepository<TaxiHailNetworkSettings> Repository { get; private set; }
    }
}