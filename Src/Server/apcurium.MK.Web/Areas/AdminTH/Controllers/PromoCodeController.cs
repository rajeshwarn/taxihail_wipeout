using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace apcurium.MK.Web.Areas.AdminTH.Controllers
{
    public class PromoCode
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class PromoCodeController : Controller
    {
        // GET: AdminTH/PromoCode
        public ActionResult Index()
        {
            var promoCodes = new List<PromoCode> {new PromoCode {Id = Guid.NewGuid(), Name = "test"}};
            return View(promoCodes);
        }

        // GET: AdminTH/PromoCode/Details/5
        public ActionResult Details(Guid id)
        {
            return View();
        }

        // GET: AdminTH/PromoCode/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AdminTH/PromoCode/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: AdminTH/PromoCode/Edit/5
        public ActionResult Edit(Guid id)
        {
            return View();
        }

        // POST: AdminTH/PromoCode/Edit/5
        [HttpPost]
        public ActionResult Edit(Guid id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: AdminTH/PromoCode/Delete/5
        public ActionResult Delete(Guid id)
        {
            return View();
        }

        // POST: AdminTH/PromoCode/Delete/5
        [HttpPost]
        public ActionResult Delete(Guid id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
