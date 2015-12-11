using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.Attributes;
using ServiceStack.CacheAccess;

namespace apcurium.MK.Web.Areas.AdminTH.Controllers
{
    [AuthorizationRequired(RoleName.Admin)]
    public class BlackListController : ServiceStackController
    {
        private readonly IBlackListEntryService _blackListService;

        public BlackListController(ICacheClient cache,
            IServerSettings serverSettings, 
            IBlackListEntryService blackListService)
            : base(cache, serverSettings)
        {
            _blackListService = blackListService;
        }
        // GET: AdminTH/BlackList
        public ActionResult Index()
        {
            return View(_blackListService.GetAll());
        }

        // POST: AdminTH/BlackList/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            // TODO: Add insert logic here
            if (collection.AllKeys.Contains("PhoneNumber"))
            {
                 _blackListService.Add(new BlackListEntry {PhoneNumber = collection["PhoneNumber"] });
            }

            return RedirectToAction("Index", _blackListService.GetAll());
        }

        public ActionResult Delete(Guid id)
        {
            _blackListService.Delete(id);
            return RedirectToAction("Index", _blackListService.GetAll());
        }
    }
}
