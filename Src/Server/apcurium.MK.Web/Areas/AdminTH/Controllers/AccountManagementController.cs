using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Api.Services.Admin;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Web.Areas.AdminTH.Models;
using apcurium.MK.Web.Attributes;
using Infrastructure.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using apcurium.MK.Booking.Resources;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Enumeration;
using PagedList;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common;
using apcurium.MK.Common.Caching;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Web.Areas.AdminTH.Controllers
{
    [AuthorizationRequired(RoleName.Support)]
    public class AccountManagementController : ApcuriumServiceController
    {
        private readonly IAccountDao _accountDao;
        private readonly IAccountNoteService _accountNoteService;
        private readonly ICreditCardDao _creditCardDao;
        private readonly ICommandBus _commandBus;
        private readonly IServerSettings _serverSettings;
        private readonly IOrderDao _orderDao;
        private readonly IPromotionDao _promoDao;
        private readonly INotificationService _notificationService;
        private readonly BookingSettingsService _bookingSettingsService;
        private readonly ConfirmAccountService _confirmAccountService;
        private readonly ExportDataService _exportDataService;
        private readonly IPaymentService _paymentService;
        private readonly Resources _resources;

        public AccountManagementController(ICacheClient cache,
           IServerSettings serverSettings,
           IAccountDao accountDao,
           IAccountNoteService accountNoteService,
           ICreditCardDao creditCardDao,
           ICommandBus commandBus,
           IOrderDao orderDao,
           IPromotionDao promoDao,
           IPaymentService paymentService,
           INotificationService notificationService,
        BookingSettingsService bookingSettingsService,
           ConfirmAccountService confirmAccountService,
           ExportDataService exportDataService)
           : base(cache, serverSettings)
        {
            _accountDao = accountDao;
            _accountNoteService = accountNoteService;
            _creditCardDao = creditCardDao;
            _bookingSettingsService = bookingSettingsService;
            _commandBus = commandBus;
            _serverSettings = serverSettings;
            _orderDao = orderDao;
            _promoDao = promoDao;
            _confirmAccountService = confirmAccountService;
            _exportDataService = exportDataService;
            _paymentService = paymentService;
            _notificationService = notificationService;

            _resources = new Resources(serverSettings);
        }

        public ActionResult Index(Guid id, int page = 1)
        {
            AccountManagementModel accountManagementModel = InitializeModel(id);

            // needed to feed orders list
            accountManagementModel.OrdersPaged = GetOrders(id, page, accountManagementModel.OrdersPageSize);
            accountManagementModel.OrdersPageIndex = page;

            return View(accountManagementModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Save(AccountManagementModel accountManagementModel)
        {
            if (ModelState.IsValid)
            {
                var accountDetail = _accountDao.FindById(accountManagementModel.Id);

                var accountUpdateRequest = new AccountUpdateRequest
                {
                    AccountId = accountManagementModel.Id,
                    BookingSettingsRequest = new BookingSettingsRequest
                    {
                        AccountNumber = accountDetail.Settings.AccountNumber,
                        ChargeTypeId = accountDetail.Settings.ChargeTypeId,
                        Country = accountManagementModel.CountryCode,
                        CustomerNumber = accountDetail.Settings.CustomerNumber,
                        DefaultTipPercent = accountManagementModel.DefaultTipPercent,
                        Email = accountManagementModel.Email,
                        FirstName = accountDetail.Name,
                        LastName = accountDetail.Name,
                        Name = accountManagementModel.Name,
                        NumberOfTaxi = accountDetail.Settings.NumberOfTaxi,
                        Passengers = accountDetail.Settings.Passengers,
                        PayBack = accountDetail.Settings.PayBack,
                        Phone = accountManagementModel.PhoneNumber,
                        ProviderId = accountDetail.Settings.ProviderId,
                        VehicleTypeId = accountDetail.Settings.VehicleTypeId
                    }
                };

                try
                {
                    _bookingSettingsService.Put(accountUpdateRequest);
                    TempData["UserMessage"] = "Operation done successfully";
                }
                catch (Exception e)
                {
                    TempData["UserMessage"] = e.Message;
                }
            }
            else
            {
                TempData["UserMessage"] = "Model state is not valid";
            }

            // needed to feed orders list
            accountManagementModel.OrdersPaged = GetOrders(accountManagementModel.Id, accountManagementModel.OrdersPageIndex, accountManagementModel.OrdersPageSize);

            return View("Index", accountManagementModel);
        }

        [HttpPost]
        public ActionResult ResetPassword(AccountManagementModel accountManagementModel)
        {
            var accountDetail = _accountDao.FindById(accountManagementModel.Id);

            var newPassword = new PasswordService().GeneratePassword();
            var resetCommand = new ResetAccountPassword
            {
                AccountId = accountManagementModel.Id,
                Password = newPassword
            };

            var emailCommand = new SendPasswordResetEmail
            {
                ClientLanguageCode = accountDetail.Language,
                EmailAddress = accountDetail.Email,
                Password = newPassword,
            };

            _commandBus.Send(resetCommand);
            _commandBus.Send(emailCommand);

            TempData["UserMessage"] = "Operation done successfully, new password: " + newPassword;

            // needed to feed orders list
            accountManagementModel.OrdersPaged = GetOrders(accountManagementModel.Id, accountManagementModel.OrdersPageIndex, accountManagementModel.OrdersPageSize);

            return View("Index", accountManagementModel);
        }

        [HttpPost]
        public async Task<ActionResult> SendConfirmationCodeSMS(AccountManagementModel accountManagementModel)
        {
            if (accountManagementModel.Email.HasValueTrimmed()
               && accountManagementModel.CountryCode != null
               && accountManagementModel.CountryCode.Code.HasValueTrimmed()
               && accountManagementModel.PhoneNumber.HasValueTrimmed())
            {
                try
                {
                    _confirmAccountService.Get(new ConfirmationCodeRequest
                    {
                        CountryCode = accountManagementModel.CountryCode.Code,
                        Email = accountManagementModel.Email,
                        PhoneNumber = accountManagementModel.PhoneNumber
                    });
                    TempData["UserMessage"] = "Operation done successfully";
                }
                catch (Exception e)
                {
                    TempData["UserMessage"] = e.Message;
                }
            }
            else
            {
                TempData["UserMessage"] = "Please provide correct country code, email and phone number";
            }

            // needed to feed orders list
            accountManagementModel.OrdersPaged = GetOrders(accountManagementModel.Id, accountManagementModel.OrdersPageIndex, accountManagementModel.OrdersPageSize);

            return View("Index", accountManagementModel);
        }

        [HttpPost]
        public ActionResult EnableDisableAccount(AccountManagementModel accountManagementModel)
        {
            if (ModelState.IsValid)
            {
                if (accountManagementModel.IsEnabled)
                {
                    AddNote(accountManagementModel, NoteType.DeactivateAccount, accountManagementModel.DisableAccountNotePopupContent);
                    _commandBus.Send(new DisableAccountByAdmin { AccountId = accountManagementModel.Id });
                    accountManagementModel.IsEnabled = false;
                }
                else
                {
                    _commandBus.Send(new EnableAccountByAdmin { AccountId = accountManagementModel.Id });
                    accountManagementModel.IsEnabled = true;
                }

                ModelState.Clear();
                TempData["UserMessage"] = "Operation done successfully";
            }
            else
            {
                TempData["UserMessage"] = "Model state is not valid";
            }

            // needed to feed orders list
            accountManagementModel.OrdersPaged = GetOrders(accountManagementModel.Id, accountManagementModel.OrdersPageIndex, accountManagementModel.OrdersPageSize);

            return View("Index", accountManagementModel);
        }

        [HttpPost]
        public ActionResult UnlinkIBSAccount(AccountManagementModel accountManagementModel)
        {
            _commandBus.Send(new UnlinkAccountFromIbs { AccountId = accountManagementModel.Id });
            TempData["UserMessage"] = "Operation done successfully";

            // needed to feed orders list
            accountManagementModel.OrdersPaged = GetOrders(accountManagementModel.Id, accountManagementModel.OrdersPageIndex, accountManagementModel.OrdersPageSize);

            return View("Index", accountManagementModel);
        }

        [HttpPost]
        public ActionResult DeleteCreditCardsInfo(AccountManagementModel accountManagementModel)
        {
            var paymentSettings = _serverSettings.GetPaymentSettings();

            var forceUserDisconnect = paymentSettings.CreditCardIsMandatory
               && (paymentSettings.IsPaymentOutOfAppDisabled != OutOfAppPaymentDisabled.None);

            _commandBus.Send(new DeleteCreditCardsFromAccounts
            {
                AccountIds = new[] { accountManagementModel.Id },
                ForceUserDisconnect = forceUserDisconnect
            });

            TempData["UserMessage"] = "Operation done successfully";

            // needed to feed orders list
            accountManagementModel.OrdersPaged = GetOrders(accountManagementModel.Id, accountManagementModel.OrdersPageIndex, accountManagementModel.OrdersPageSize);

            return View("Index", accountManagementModel);
        }

        [HttpPost]
        public async Task<ActionResult> ExportOrders(AccountManagementModel accountManagementModel)
        {
            var csv = (List<Dictionary<string, string>>)_exportDataService.Post(new ExportDataRequest { AccountId = accountManagementModel.Id, Target = DataType.Orders });
            if (csv.IsEmpty())
            {
                return View("Index", accountManagementModel);
            }

            var csvFlattened = new StringBuilder();
            foreach (var item in csv.ElementAt(0))
            {
                csvFlattened.Append(item.Key).Append(",");
            }

            csvFlattened.Append("\n");

            foreach (var line in csv)
            {
                foreach (var item in line)
                {
                    csvFlattened.Append(item.Value).Append(",");
                }
                csvFlattened.Append("\n");
            }

            // needed to feed orders list
            accountManagementModel.OrdersPaged = GetOrders(accountManagementModel.Id, accountManagementModel.OrdersPageIndex, accountManagementModel.OrdersPageSize);

            return File(new ASCIIEncoding().GetBytes(csvFlattened.ToString()), "text/csv", "Export.csv");
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddStandardNote(AccountManagementModel accountManagementModel)
        {
            if (ModelState.IsValid)
            {
                AddNote(accountManagementModel, NoteType.Standard, accountManagementModel.NotePopupContent);
                TempData["UserMessage"] = "Note added";
                ModelState.Clear();
            }
            else
            {
                TempData["UserMessage"] = "Model state is not valid";
            }

            // needed to feed orders list
            accountManagementModel.OrdersPaged = GetOrders(accountManagementModel.Id, accountManagementModel.OrdersPageIndex, accountManagementModel.OrdersPageSize);

            return View("Index", accountManagementModel);
        }

        [HttpPost]
        public ActionResult RefundOrder(AccountManagementModel accountManagementModel)
        {
            if (ModelState.IsValid)
            {
                var refundPaymentResponse = _paymentService.RefundPayment(null, accountManagementModel.RefundOrderId);

                if (refundPaymentResponse.IsSuccessful)
                {
                    var order = _orderDao.FindByAccountId(accountManagementModel.Id).FirstOrDefault(o => o.Id == accountManagementModel.RefundOrderId);
                    var orderModel = new OrderModel(order);

                    _notificationService.SendOrderRefundEmail(
                        DateTime.Now, 
                        refundPaymentResponse.Last4Digits,
                        orderModel.TotalAmount(), 
                        accountManagementModel.Email, 
                        AuthSession.UserAuthName,
                        order.ClientLanguageCode);

                    AddNote(accountManagementModel, NoteType.Refunded, accountManagementModel.RefundOrderNotePopupContent);
                    TempData["UserMessage"] = "order refunded, note added, email sent";
                }
                else
                {
                    TempData["UserMessage"] = "an error occured: " + refundPaymentResponse.Message;
                }

                ModelState.Clear();
            }
            else
            {
                TempData["UserMessage"] = "Model state is not valid";
            }

            // needed to feed orders list
            accountManagementModel.OrdersPaged = GetOrders(accountManagementModel.Id, accountManagementModel.OrdersPageIndex, accountManagementModel.OrdersPageSize);

            return View("Index", accountManagementModel);
        }

        private void AddNote(AccountManagementModel accountManagementModel, NoteType noteType, string noteContent)
        {
            var accountNoteEntry = new AccountNoteEntry
            {
                AccountId = accountManagementModel.Id,
                WriterAccountId = new Guid(AuthSession.UserAuthId),
                WriterAccountEmail = AuthSession.UserAuthName,
                Type = noteType,
                CreationDate = DateTime.Now,
                Note = noteContent
            };

            _accountNoteService.Add(accountNoteEntry);
            if (accountManagementModel.Notes == null)
            {
                accountManagementModel.Notes = new List<NoteModel>();
            }
            accountManagementModel.Notes.Insert(0, new NoteModel(accountNoteEntry));
        }

        private AccountManagementModel InitializeModel(Guid accountId)
        {
            var accountDetail = _accountDao.FindById(accountId);

            var orders = GetOrderDetails(accountId) ?? new List<OrderDetail>();
            var ordersCount = orders.Count();
            var accountAgeInDays = (DateTime.UtcNow - accountDetail.CreationDate).TotalDays;
            var averageTripsPerDay = decimal.Round((decimal) (ordersCount/accountAgeInDays), 1);
            var totalCanceled = GetOrderStatusDetails(accountId).Count(x => x.IBSStatusId == VehicleStatuses.Common.Cancelled || x.IBSStatusId == VehicleStatuses.Common.CancelledDone);
            var totalCompleted = orders.Count(order => order.Status == (int)OrderStatus.Completed);
            var totalNoShow = GetOrderStatusDetails(accountId).Count(x => x.IBSStatusId == VehicleStatuses.Common.NoShow);

            var model = new AccountManagementModel
            {
                Id = accountId,
                Name = accountDetail.Name,
                Email = accountDetail.Email,
                CustomerNumber = accountDetail.Settings.CustomerNumber,
                CreationDate = accountDetail.CreationDate,
                FacebookAccount = accountDetail.FacebookId,
                IBSAccountId = accountDetail.IBSAccountId,
                IsConfirmed = accountDetail.IsConfirmed,
                IsEnabled = !accountDetail.DisabledByAdmin,
                CountryCode = accountDetail.Settings.Country,
                PhoneNumber = accountDetail.Settings.Phone,
                ChargeType = accountDetail.Settings.ChargeType,
                DefaultTipPercent = accountDetail.DefaultTipPercent,
                IsPayPalAccountLinked = accountDetail.IsPayPalAccountLinked,
                TotalCanceled = totalCanceled,
                TotalCompleted = totalCompleted,
                AverageTripsPerDay = averageTripsPerDay,
                TotalNoShows = totalNoShow
            };

            if (accountDetail.DefaultCreditCard != null)
            {
                var defaultCreditCard = _creditCardDao.FindById(accountDetail.DefaultCreditCard.Value);
                model.CreditCardLast4Digits = defaultCreditCard.Last4Digits;
            }

            model.Notes = _accountNoteService.FindByAccountId(accountId)
                .OrderByDescending(c => c.CreationDate)
                .Select(x => new NoteModel(x))
                .ToList();

            return model;
        }

        private IEnumerable<OrderDetail> _orderDetails;
        private IEnumerable<OrderDetail> GetOrderDetails(Guid accountId)
        {
            return _orderDetails ?? (_orderDetails = _orderDao.FindByAccountId(accountId)
                .OrderByDescending(c => c.CreatedDate));
        }

        private IEnumerable<OrderStatusDetail> _orderStatusDetails;
        private IEnumerable<OrderStatusDetail> GetOrderStatusDetails(Guid accountId)
        {
            return _orderStatusDetails ?? (_orderStatusDetails = _orderDao.FindOrderStatusByAccountId(accountId));
        }

        private IEnumerable<PromotionUsageDetail> _promotionUsageDetails;
        private IEnumerable<PromotionUsageDetail> GetPromotionUsageDetails(Guid accountId)
        {
            return _promotionUsageDetails ?? (_promotionUsageDetails = _promoDao.FindPromotionUsageByAccountId(accountId));
        }

        private PagedList<OrderModel> GetOrders(Guid accountId, int page, int ordersPageSize)
        {
            var paymentSettings = _serverSettings.GetPaymentSettings();

            var orders = GetOrderDetails(accountId)
               .Select(x =>
               {
                   var promo =  GetPromotionUsageDetails(accountId).FirstOrDefault(promotionUsageDetail => promotionUsageDetail.OrderId == x.Id);
                   var status = GetOrderStatusDetails(accountId).FirstOrDefault(orderStatusDetail => orderStatusDetail.OrderId == x.Id);
                   var orderPairing = _orderDao.FindOrderPairingById(x.Id);

                   return new OrderModel(x)
                   {
                       IsOrderPairing = orderPairing != null,
                       PromoCode = promo != null ? promo.Code : string.Empty,
                       FareString = _resources.FormatPrice(x.Fare),
                       TaxString = _resources.FormatPrice(x.Tax),
                       TollString = _resources.FormatPrice(x.Toll),
                       TipString = _resources.FormatPrice(x.Tip),
                       SurchargeString = _resources.FormatPrice(x.Surcharge),
                       TotalAmountString = _resources.FormatPrice(x.TotalAmount()),
                       IsRideLinqCMTPaymentMode = (paymentSettings.PaymentMode == PaymentMethod.RideLinqCmt) && (x.Settings.ChargeTypeId == ChargeTypes.CardOnFile.Id),
                       StatusString = status != null ? (status.IBSStatusId == VehicleStatuses.Common.NoShow ? "NoShow" : ((OrderStatus)x.Status).ToString()) : string.Empty
                   };
               })
               .ToList();

            return new PagedList<OrderModel>(orders, page, ordersPageSize);
        }
    }
}