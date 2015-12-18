using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.Areas.AdminTH.Models;
using Infrastructure.Messaging;
using ServiceStack.CacheAccess;
using ServiceStack.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace apcurium.MK.Web.Areas.AdminTH.Controllers
{
   public class AccountManagementController : ServiceStackController
   {
      private readonly IAccountDao _accountDao;
      private readonly ICreditCardDao _creditCardDao;
      private readonly ICommandBus _commandBus;
      private readonly IServerSettings _serverSettings;
      private readonly IOrderDao _orderDao;
      private readonly BookingSettingsService _bookingSettingsService;
      private readonly ConfirmAccountService _confirmAccountService;

      public AccountManagementController(ICacheClient cache,
         IServerSettings serverSettings,
         IAccountDao accountDao,
         ICreditCardDao creditCardDao,
         ICommandBus commandBus,
         IOrderDao orderDao,
         BookingSettingsService bookingSettingsService,
         ConfirmAccountService confirmAccountService)
         : base(cache, serverSettings)
      {
         _accountDao = accountDao;
         _creditCardDao = creditCardDao;
         _bookingSettingsService = bookingSettingsService;
         _commandBus = commandBus;
         _serverSettings = serverSettings;
         _orderDao = orderDao;
         _confirmAccountService = confirmAccountService;
      }

      public ActionResult Index(Guid id)
      {
         AccountManagementModel accountManagementModel = InitialiseModel(id);

         return View(accountManagementModel);
      }

      private AccountManagementModel InitialiseModel(Guid id)
      {
         var accountDetail = _accountDao.FindById(id);
         var accountManagementModel = new AccountManagementModel();

         accountManagementModel.Id = id;
         accountManagementModel.Name = accountDetail.Name;
         accountManagementModel.Email = accountDetail.Email;
         accountManagementModel.CustomerNumber = accountDetail.Settings.CustomerNumber;
         accountManagementModel.CreationDate = accountDetail.CreationDate;
         accountManagementModel.FacebookAccount = accountDetail.FacebookId;
         accountManagementModel.IBSAccountId = accountDetail.IBSAccountId;
         accountManagementModel.IsConfirmed = accountDetail.IsConfirmed;
         accountManagementModel.IsEnabled = !accountDetail.DisabledByAdmin;
         accountManagementModel.CountryCode = accountDetail.Settings.Country;
         accountManagementModel.PhoneNumber = accountDetail.Settings.Phone;
         accountManagementModel.ChargeType = accountDetail.Settings.ChargeType;
         accountManagementModel.DefaultTipPercent = accountDetail.DefaultTipPercent;
         accountManagementModel.IsPayPalAccountLinked = accountDetail.IsPayPalAccountLinked;
         if (accountDetail.DefaultCreditCard != null
            && accountDetail.DefaultCreditCard.GetValueOrDefault() != null
            && _creditCardDao.FindById(accountDetail.DefaultCreditCard.GetValueOrDefault()) != null)
         {
            accountManagementModel.CreditCardLast4Digits = _creditCardDao.FindById(accountDetail.DefaultCreditCard.GetValueOrDefault()).Last4Digits;
         }

         // save IOrderDao object in session
         HttpContext.Session.Add("orderDao", _orderDao);

         return accountManagementModel;
      }

      [HttpPost]
      [ValidateInput(false)]
      public async Task<ActionResult> Save(AccountManagementModel accountManagementModel)
      {
         if (ModelState.IsValid)
         {
            var accountDetail = _accountDao.FindById(accountManagementModel.Id);

            var bookingSettingsRequest = new BookingSettingsRequest();
            bookingSettingsRequest.AccountNumber = accountDetail.Settings.AccountNumber;
            bookingSettingsRequest.ChargeTypeId = accountDetail.Settings.ChargeTypeId;
            bookingSettingsRequest.Country = accountManagementModel.CountryCode;
            bookingSettingsRequest.CustomerNumber = accountDetail.Settings.CustomerNumber;
            bookingSettingsRequest.DefaultTipPercent = accountManagementModel.DefaultTipPercent;
            bookingSettingsRequest.Email = accountManagementModel.Email;
            bookingSettingsRequest.FirstName = accountDetail.Name;
            bookingSettingsRequest.LastName = accountDetail.Name;
            bookingSettingsRequest.Name = accountManagementModel.Name;
            bookingSettingsRequest.NumberOfTaxi = accountDetail.Settings.NumberOfTaxi;
            bookingSettingsRequest.Passengers = accountDetail.Settings.Passengers;
            bookingSettingsRequest.PayBack = accountDetail.Settings.PayBack;
            bookingSettingsRequest.Phone = accountManagementModel.PhoneNumber;
            bookingSettingsRequest.ProviderId = accountDetail.Settings.ProviderId;
            bookingSettingsRequest.VehicleTypeId = accountDetail.Settings.VehicleTypeId;

            var accountUpdateRequest = new AccountUpdateRequest();
            accountUpdateRequest.AccountId = accountManagementModel.Id;
            accountUpdateRequest.BookingSettingsRequest = bookingSettingsRequest;

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

         TempData["UserMessage"] = "Operation done successfully";
         return View("Index", accountManagementModel);
      }

      [HttpPost]
      public async Task<ActionResult> SendConfirmationCodeSMS(AccountManagementModel accountManagementModel)
      {
         if (accountManagementModel.Email != null
            && accountManagementModel.Email.Length > 0
            && accountManagementModel.CountryCode != null
            && accountManagementModel.CountryCode.Code != null
            && accountManagementModel.CountryCode.Code.Length > 0
            && accountManagementModel.PhoneNumber != null
            && accountManagementModel.PhoneNumber.Length > 0)
         {

            try
            {
               _confirmAccountService.Get(new ConfirmationCodeRequest() { CountryCode = accountManagementModel.CountryCode.Code, Email = accountManagementModel.Email, PhoneNumber = accountManagementModel.PhoneNumber });
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

         return View("Index", accountManagementModel);
      }

      [HttpPost]
      public async Task<ActionResult> EnableDisableAccount(AccountManagementModel accountManagementModel)
      {
         if (accountManagementModel.IsEnabled)
         {
            _commandBus.Send(new DisableAccountByAdmin { AccountId = accountManagementModel.Id });
            accountManagementModel.IsEnabled = false;
         }
         else
         {
            _commandBus.Send(new EnableAccountByAdmin { AccountId = accountManagementModel.Id });
            accountManagementModel.IsEnabled = true;
         }

         TempData["UserMessage"] = "Operation done successfully";
         return View("Index", accountManagementModel);
      }

      [HttpPost]
      public async Task<ActionResult> UnlinkIBSAccount(AccountManagementModel accountManagementModel)
      {
         _commandBus.Send(new UnlinkAccountFromIbs { AccountId = accountManagementModel.Id });
         TempData["UserMessage"] = "Operation done successfully";
         return View("Index", accountManagementModel);
      }

      [HttpPost]
      public async Task<ActionResult> DeleteCreditCardsInfo(AccountManagementModel accountManagementModel)
      {
         var paymentSettings = _serverSettings.GetPaymentSettings();

         var forceUserDisconnect = paymentSettings.CreditCardIsMandatory
            && paymentSettings.IsOutOfAppPaymentDisabled;

         _commandBus.Send(new DeleteCreditCardsFromAccounts
         {
            AccountIds = new[] { accountManagementModel.Id },
            ForceUserDisconnect = forceUserDisconnect
         });

         TempData["UserMessage"] = "Operation done successfully";
         return View("Index", accountManagementModel);
      }
   }
}