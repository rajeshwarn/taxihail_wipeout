using System;
using System.Linq;
using System.Web.Mvc;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.Areas.AdminTH.Models;
using Infrastructure.Messaging;
using ServiceStack.CacheAccess;

namespace apcurium.MK.Web.Areas.AdminTH.Controllers
{
    public class PromoCodeController : ServiceStackController
    {
        private readonly IPromotionDao _promotionDao;
        private readonly ICommandBus _commandBus;

        public PromoCodeController(ICacheClient cache, IServerSettings serverSettings, IPromotionDao promotionDao, ICommandBus commandBus) : base(cache, serverSettings)
        {
            _promotionDao = promotionDao;
            _commandBus = commandBus;
        }

        // GET: AdminTH/PromoCode
        public ActionResult Index()
        {
            var promotions = _promotionDao.GetAll().Select(x => new PromoCode(x));
            return View(promotions);
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
                _commandBus.Send(new CreatePromotion
                {
                    PromoId = Guid.NewGuid(),
                    Name = form["Name"],
                    Code = "code"
                });

                TempData["Info"] = string.Format("Promotion \"{0}\" created", form["Name"]);
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
            var promotion = _promotionDao.FindById(id);
            return View(new PromoCode(promotion));
        }

        // POST: AdminTH/PromoCode/Edit/5
        [HttpPost]
        public ActionResult Edit(Guid id, FormCollection form)
        {
            try
            {
                _commandBus.Send(new UpdatePromotion
                {
                    PromoId = id,
                    Name = form["Name"],
                    Code = "code"
                });

                TempData["Info"] = string.Format("Promotion \"{0}\" updated", form["Name"]);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Info"] = ex.Message;
                return View();
            }
        }

        // GET: AdminTH/PromoCode/Activate/5
        public ActionResult Activate(Guid id)
        {
            try
            {
                _commandBus.Send(new ActivatePromotion
                {
                    PromoId = id
                });

                TempData["Info"] = "Promotion activated";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View("Edit");
            }
        }

        // GET: AdminTH/PromoCode/Deactivate/5
        public ActionResult Deactivate(Guid id)
        {
            try
            {
                _commandBus.Send(new DeactivatePromotion
                {
                    PromoId = id
                });

                TempData["Info"] = "Promotion deactivated";
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
