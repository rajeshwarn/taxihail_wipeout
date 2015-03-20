﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Web.Areas.AdminTH.Models;
using apcurium.MK.Web.Attributes;
using Infrastructure.Messaging;
using ServiceStack.CacheAccess;
using ServiceStack.ServiceModel.Extensions;

namespace apcurium.MK.Web.Areas.AdminTH.Controllers
{
    [AuthorizationRequired(RoleName.Admin)]
    public class PromoCodeController : ServiceStackController
    {
        private readonly IPromotionDao _promotionDao;
        private readonly IAccountDao _accountDao;
        private readonly ICommandBus _commandBus;
        private readonly IServerSettings _serverSettings;

        public PromoCodeController(ICacheClient cache,
            IServerSettings serverSettings,
            IPromotionDao promotionDao,
            IAccountDao accountDao,
            ICommandBus commandBus) : base(cache, serverSettings)
        {
            _promotionDao = promotionDao;
            _accountDao = accountDao;
            _commandBus = commandBus;
            _serverSettings = serverSettings;
        }

        // GET: AdminTH/PromoCode
        public ActionResult Index()
        {
            var paymentSettings = _serverSettings.GetPaymentSettings();
            if (paymentSettings.PaymentMode == PaymentMethod.None)
            {
                TempData["Warning"] = "No payment Method has been configured. For users to be able to use promo codes, " +
                                      "go to the Payment Settings section and configure a Payment method.";
            }

            var updatedModel = TempData["Model"];
            if (updatedModel != null)
            {
                return View(updatedModel);
            }
            else
            {
                var promotions = _promotionDao.GetAll().Select(x => new PromoCode(x));
                return View(promotions);
            }
        }

        // GET: AdminTH/PromoCode/Create
        [SkipAuthentication]
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
                    var validationErrors = string.Join(", ",
                        ModelState.Values.Where(x => x.Errors.Count > 0)
                        .SelectMany(x => x.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToArray());
                    throw new Exception(string.Format("Something's not right.{0}{1}", Environment.NewLine, validationErrors));
                }

                if (_promotionDao.FindByPromoCode(promoCode.Code) != null)
                {
                    throw new Exception("A promotion with this code already exists");
                }

                var promotionId = Guid.NewGuid();
                promoCode.Id = promotionId;

                _commandBus.Send(new CreatePromotion
                {
                    PromoId = promotionId,
                    Name = promoCode.Name,
                    Description = promoCode.Description,
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
                    Code = promoCode.Code,
                    PublishedStartDate = promoCode.PublishedStartDate,
                    PublishedEndDate = promoCode.PublishedEndDate,
                    TriggerSettings = promoCode.TriggerSettings
                });

                TempData["Info"] = string.Format("Promotion \"{0}\" created", promoCode.Name);

                var promotions = _promotionDao.GetAll().Select(x => new PromoCode(x)).ToList();
                promotions.Add(promoCode);
                var orderedPromotions = promotions.OrderBy(p => p.Name);

                TempData["Model"] = orderedPromotions;

                return RedirectToAction("Index", orderedPromotions);
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

            var model = new PromoCode(promotion);
            if (promotion.TriggerSettings.Type != PromotionTriggerTypes.NoTrigger)
            {
                model.CanModifyTriggerGoal = !_promotionDao.GetProgressByPromo(id).Any();
            }

            return View(model);
        }

        // POST: AdminTH/PromoCode/Edit/5
        [HttpPost]
        public ActionResult Edit(PromoCode promoCode)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var validationErrors = string.Join(", ",
                        ModelState.Values.Where(x => x.Errors.Count > 0)
                        .SelectMany(x => x.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToArray());
                    throw new Exception(string.Format("Something's not right.{0}{1}", Environment.NewLine, validationErrors));
                }
                
                if (promoCode.TriggerSettings.Type == PromotionTriggerTypes.AmountSpent)
                {
                    promoCode.TriggerSettings.RideCount = 0;
                }
                else if (promoCode.TriggerSettings.Type == PromotionTriggerTypes.RideCount)
                {
                    promoCode.TriggerSettings.AmountSpent = 0;
                }
                else
                {
                    promoCode.TriggerSettings.RideCount = 0;
                    promoCode.TriggerSettings.AmountSpent = 0;
                }

                if (!promoCode.CanModifyTriggerGoal)
                {
                    var promotion = _promotionDao.FindById(promoCode.Id);

                    promoCode.TriggerSettings.Type = promotion.TriggerSettings.Type;
                    promoCode.TriggerSettings.AmountSpent = promotion.TriggerSettings.AmountSpent;
                    promoCode.TriggerSettings.RideCount = promotion.TriggerSettings.RideCount;
                }

                _commandBus.Send(new UpdatePromotion
                {
                    PromoId = promoCode.Id,
                    Name = promoCode.Name,
                    Description = promoCode.Description,
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
                    Code = promoCode.Code,
                    PublishedStartDate = promoCode.PublishedStartDate,
                    PublishedEndDate = promoCode.PublishedEndDate,
                    TriggerSettings = promoCode.TriggerSettings
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

            var promotion = _promotionDao.FindById(id);
            TempData["Info"] = string.Format("Promotion \"{0}\" Enabled", promotion.Name);

            return RedirectToAction("Index");
        }

        // GET: AdminTH/PromoCode/Deactivate/5
        public ActionResult Deactivate(Guid id)
        {
            _commandBus.Send(new DeactivatePromotion
            {
                PromoId = id
            });

            var promotion = _promotionDao.FindById(id);
            TempData["Info"] = string.Format("Promotion \"{0}\" Disabled", promotion.Name);

            return RedirectToAction("Index");
        }

        // GET: AdminTH/PromoCode/Statistics/5
        public ActionResult Statistics(Guid id)
        {
            var promotionUsages = _promotionDao.GetRedeemedPromotionUsages(id).ToArray();
            return View(promotionUsages.Any() ? new PromoStats(promotionUsages) : null);
        }

        public ActionResult UserStatistics(Guid id)
        {
            var promotionUsages = _promotionDao.GetRedeemedPromotionUsages(id).ToArray();
            return View(promotionUsages.Any() ? new PromoStats(promotionUsages) : null);
        }

        public ActionResult Unlock()
        {
            var promotions = _promotionDao.GetAll().Select(x => new PromoCode(x));
            return View(promotions);
        }

        [HttpPost]
        public ActionResult Unlock(FormCollection form)
        {
            var appSettings = form.ToDictionary();
            if (appSettings != null)
            {
                // Remove all whitespaces and split
                var userEmails = Regex.Replace(appSettings["userEmails"], @"\s+", string.Empty).Split(',');

                Guid promotionId;
                Guid.TryParse(appSettings["promotionIdToUnlock"], out promotionId);

                var userAccoundIds = new List<Guid>();

                foreach (var userEmail in userEmails)
                {
                    var accountDetail = _accountDao.FindByEmail(userEmail);
                    if (accountDetail == null)
                    {
                        // Account not found
                        continue;
                    }

                    userAccoundIds.Add(accountDetail.Id);
                }

                if (userAccoundIds.Any() && promotionId.HasValue())
                {
                    _commandBus.Send(new AddUserToPromotionWhiteList
                    {
                        PromoId = promotionId,
                        AccountIds = userAccoundIds
                    });

                    var promotion = _promotionDao.FindById(promotionId);

                    TempData["Info"] = string.Format("Promotion \"{0} ({1})\"\n was sent to users {2}",
                        promotion.Name,
                        promotion.Code,
                        userEmails.Flatten(","));
                }
            }

            return RedirectToAction("Unlock");
        }
    }
}
