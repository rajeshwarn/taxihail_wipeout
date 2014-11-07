using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.Web.Mvc;
using apcurium.MK.Common.Configuration;
using ServiceStack.CacheAccess;

namespace apcurium.MK.Web.Areas.AdminTH.Controllers
{
    public class PromoCode
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class PromoCodeController : ServiceStackController
    {
        private List<PromoCode> promoCodes = new List<PromoCode> { new PromoCode { Id = Guid.NewGuid(), Name = "test" } };

        public PromoCodeController(ICacheClient cache, IServerSettings serverSettings) : base(cache, serverSettings)
        {
        }

        // GET: AdminTH/PromoCode
        public ActionResult Index()
        {
            if (AuthSession.IsAuthenticated)
            {
                return View(promoCodes);
            }

            return Redirect(BaseUrl);
        }

        // GET: AdminTH/PromoCode/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AdminTH/PromoCode/Create
        [HttpPost]
        public ActionResult Create(FormCollection form)
        {
            try
            {
                // TODO: Add create logic here

                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        // GET: AdminTH/PromoCode/Edit/5
        public ActionResult Edit(Guid id)
        {
            var promoCode = promoCodes.First();
            return View(promoCode);
        }

        // POST: AdminTH/PromoCode/Edit/5
        [HttpPost]
        public ActionResult Edit(Guid id, FormCollection form)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        // GET: AdminTH/PromoCode/Delete/5
        public ActionResult Delete(Guid id)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View("Edit");
            }
        }
    }
}
