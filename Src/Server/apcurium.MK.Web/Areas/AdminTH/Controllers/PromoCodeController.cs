using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Caching;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Enumeration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Web.Areas.AdminTH.Models;
using apcurium.MK.Web.Attributes;
using apcurium.MK.Web.Extensions;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Areas.AdminTH.Controllers
{
    [AuthorizationRequired(RoleName.Support)]
    public class PromoCodeController : ApcuriumServiceController
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
                var promotions = _promotionDao.GetAll().Select(x => new PromoCodeModel(x));
                return View(promotions);
            }
        }

        // GET: AdminTH/PromoCode/Create
        [SkipAuthentication]
        public ActionResult Create()
        {
            return View(new PromoCodeModel());
        }

        // POST: AdminTH/PromoCode/Create
        [HttpPost]
        public ActionResult Create(PromoCodeModel promoCode)
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

                var createPromotionCommand = new CreatePromotion
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
                    Code = promoCode.Code,
                    TriggerSettings = promoCode.TriggerSettings
                };

                if (promoCode.TriggerSettings.Type == PromotionTriggerTypes.NoTrigger)
                {
                    createPromotionCommand.PublishedStartDate = promoCode.PublishedStartDate;
                    createPromotionCommand.PublishedEndDate = promoCode.PublishedEndDate;
                }
                else
                {
                    // Trigger promotions are always published (but user will only see them when whitelisted)
                    createPromotionCommand.PublishedStartDate = SqlDateTime.MinValue.Value;
                    createPromotionCommand.PublishedEndDate = SqlDateTime.MaxValue.Value;
                }

                if (promoCode.TriggerSettings.Type != PromotionTriggerTypes.CustomerSupport)
                {
                    // User and system usage is unlimited for support promotion. The whitelist will determine if a user can use it.
                    createPromotionCommand.MaxUsage = promoCode.MaxUsage;
                    createPromotionCommand.MaxUsagePerUser = promoCode.MaxUsagePerUser;
                }

                _commandBus.Send(createPromotionCommand);

                TempData["Info"] = string.Format("Promotion \"{0}\" created", promoCode.Name);

                var promotions = _promotionDao.GetAll().Select(x => new PromoCodeModel(x)).ToList();
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

            var model = new PromoCodeModel(promotion);
            if (promotion.TriggerSettings.Type != PromotionTriggerTypes.NoTrigger)
            {
                model.CanModifyTriggerGoal = !_promotionDao.GetProgressByPromo(id).Any();
            }

            return View(model);
        }

        // POST: AdminTH/PromoCode/Edit/5
        [HttpPost]
        public ActionResult Edit(PromoCodeModel promoCode)
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

        public ActionResult Delete(Guid id)
        {
            var promotions = _promotionDao.GetAll().ToList();
            var promToDelete = promotions.Find(p => p.Id == id);

            if (promToDelete != null)
            {
                promotions.Remove(promToDelete);

                _commandBus.Send(new ICommand[]
                    {
                        new DeactivatePromotion { PromoId = id },
                        new DeletePromotion { PromoId = id }
                    });

                TempData["Model"] = promotions.Select(x => new PromoCodeModel(x)).OrderBy(p => p.Name);

                TempData["Info"] = string.Format("Promotion \"{0}\" Deleted", promToDelete.Name);
            }

            return RedirectToAction("Index");
        }

        // GET: AdminTH/PromoCode/Statistics/5
        public ActionResult Statistics(Guid id)
        {
            var promotionUsages = _promotionDao.GetRedeemedPromotionUsages(id).ToArray();
            return View(promotionUsages.Any() ? new PromoStatsModel(promotionUsages) : null);
        }

        public ActionResult UserStatistics(Guid id)
        {
            var promotionUsages = _promotionDao.GetRedeemedPromotionUsages(id).ToArray();
            return View(promotionUsages.Any() ? new PromoStatsModel(promotionUsages) : null);
        }

        public ActionResult Unlock()
        {
            var promotions = GetCustomerSupportPromoCodes();

            return View(promotions);
        }

        private IEnumerable<PromoCodeModel> GetCustomerSupportPromoCodes()
        {
            return _promotionDao.GetAll()
                .Select(p => new PromoCodeModel(p))
                .Where(p => p.Active
                    && p.TriggerSettings.Type == PromotionTriggerTypes.CustomerSupport);
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

                var userAccoundIds = 
                    (from userEmail in userEmails 
                     select _accountDao.FindByEmail(userEmail) into accountDetail
                     where accountDetail != null 
                     select accountDetail.Id).ToArray();

                if (userAccoundIds.Any() && promotionId.HasValue())
                {
                    ViewBag.Error = null;

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
                else
                {
                    var message = promotionId.HasValue()
                        ? string.Empty
                        : "You must select a promotion to send to the customer." + Environment.NewLine;

                    message = userAccoundIds.Any()
                        ? message.Replace(Environment.NewLine, string.Empty)
                        : message + "You must enter one or more valid customer email.";

                    ViewBag.Error = message;

                    var promotions = GetCustomerSupportPromoCodes();

                    return View(promotions);
                }
            }

            return RedirectToAction("Unlock");
        }
    }
}
