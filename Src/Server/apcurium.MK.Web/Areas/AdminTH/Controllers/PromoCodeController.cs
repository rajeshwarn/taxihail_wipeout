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
            return View(new PromoCode());
        }

        // POST: AdminTH/PromoCode/Create
        [HttpPost]
        public ActionResult Create(PromoCode promoCode)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new Exception("Something's not right");
                }

                if (_promotionDao.FindByPromoCode(promoCode.Code) != null)
                {
                    throw new Exception("A promotion with this code already exists");
                }

                _commandBus.Send(new CreatePromotion
                {
                    PromoId = Guid.NewGuid(),
                    Name = promoCode.Name,
                    StartDate = promoCode.StartDate,
                    EndDate = promoCode.EndDate,
                    DaysOfWeek = promoCode.DaysOfWeek,
                    StartTime = promoCode.StartTime,
                    EndTime = promoCode.EndTime,
                    AppliesToCurrentBooking = promoCode.AppliesToCurrentBooking,
                    AppliesToFutureBooking = promoCode.AppliesToFutureBooking,
                    DiscountValue = promoCode.DiscountValue,
                    DiscountType = promoCode.DiscountType,
                    MaxUsagePerUser = promoCode.MaxUsagePerUser,
                    MaxUsage = promoCode.MaxUsage,
                    Code = promoCode.Code
                });

                TempData["Info"] = string.Format("Promotion \"{0}\" created", promoCode.Name);
                TempData["RefreshButton"] = "<a class='text-warning' href='#' onclick='window.location.reload(true);'>Click here to refresh if you don't see it</a>";
                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(promoCode);
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
        public ActionResult Edit(PromoCode promoCode)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new Exception("Something's not right");
                }

                _commandBus.Send(new UpdatePromotion
                {
                    PromoId = promoCode.Id,
                    Name = promoCode.Name,
                    StartDate = promoCode.StartDate,
                    EndDate = promoCode.EndDate,
                    DaysOfWeek = promoCode.DaysOfWeek,
                    StartTime = promoCode.StartTime,
                    EndTime = promoCode.EndTime,
                    AppliesToCurrentBooking = promoCode.AppliesToCurrentBooking,
                    AppliesToFutureBooking = promoCode.AppliesToFutureBooking,
                    DiscountValue = promoCode.DiscountValue,
                    DiscountType = promoCode.DiscountType,
                    MaxUsagePerUser = promoCode.MaxUsagePerUser,
                    MaxUsage = promoCode.MaxUsage,
                    Code = promoCode.Code
                });

                TempData["Info"] = string.Format("Promotion \"{0}\" updated", promoCode.Name);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(promoCode);
            }
        }

        // GET: AdminTH/PromoCode/Activate/5
        public ActionResult Activate(Guid id)
        {
            _commandBus.Send(new ActivatePromotion
            {
                PromoId = id
            });

            TempData["Info"] = "Promotion activated";
            return RedirectToAction("Index");
        }

        // GET: AdminTH/PromoCode/Deactivate/5
        public ActionResult Deactivate(Guid id)
        {
            _commandBus.Send(new DeactivatePromotion
            {
                PromoId = id
            });

            TempData["Info"] = "Promotion deactivated";
            return RedirectToAction("Index");
        }
    }
}
