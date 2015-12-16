using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
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
		private readonly BookingSettingsService _bookingSettingsService;
		private readonly ResetPasswordService _resetPasswordService;
		private readonly ICommandBus _commandBus;
		private readonly IServerSettings _serverSettings;
		private readonly IOrderDao _orderDao;

		public AccountManagementController(ICacheClient cache,
			IServerSettings serverSettings,
			IAccountDao accountDao,
			ICreditCardDao creditCardDao,
			BookingSettingsService bookingSettingsService,
			ResetPasswordService resetPasswordService,
			ICommandBus commandBus,
			IOrderDao orderDao)
			: base(cache, serverSettings)
		{
			_accountDao = accountDao;
			_creditCardDao = creditCardDao;
			_bookingSettingsService = bookingSettingsService;
			_resetPasswordService = resetPasswordService;
			_commandBus = commandBus;
			_serverSettings = serverSettings;
			_orderDao = orderDao;
		}

		// GET: AdminTH/AccountDetails
		public ActionResult Index(Guid id)
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

			// get all coutryCode to feed combobox
			accountManagementModel.CountryCodesList = new List<SelectListItem>();
			foreach (CountryCode countryCode in CountryCode.CountryCodes)
			{
				accountManagementModel.CountryCodesList.Add(new SelectListItem() { Value = countryCode.CountryISOCode.Code, Text = HttpUtility.HtmlDecode(countryCode.GetTextCountryDialCodeAndCountryName()) });
			}

			// get all order for an AccountId
			var orderMapper = new OrderMapper();
			accountManagementModel.OrderDetailList = _orderDao.FindByAccountId(accountManagementModel.Id)
				.Where(x => !x.IsRemovedFromHistory)
				.OrderByDescending(c => c.CreatedDate)
				.Select(read => orderMapper.ToResource(read))
				.ToList();

			return View(accountManagementModel);
		}

		[HttpPost]
		[ValidateInput(false)]
		public async Task<ActionResult> Save(AccountManagementModel accountManagementModel)
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

			// TODO manage return ?!
			_bookingSettingsService.Put(accountUpdateRequest);

			accountManagementModel.CountryCodesList = new List<SelectListItem>();
			foreach (CountryCode countryCode in CountryCode.CountryCodes)
			{
				accountManagementModel.CountryCodesList.Add(new SelectListItem() { Value = countryCode.CountryISOCode.Code, Text = HttpUtility.HtmlDecode(countryCode.GetTextCountryDialCodeAndCountryName()) });
			}

			return View("Index", accountManagementModel);
		}

		[HttpPost]
		public async Task<ActionResult> ResetPassword(AccountManagementModel accountManagementModel)
		{
			_resetPasswordService.Post(new ResetPassword() { EmailAddress = accountManagementModel.Email });

			return View("Index", accountManagementModel);
		}

		[HttpPost]
		public async Task<ActionResult> SendConfirmationCodeSMS(AccountManagementModel accountManagementModel)
		{
			_resetPasswordService.Post(new ResetPassword() { EmailAddress = accountManagementModel.Email });

			return View("Index", accountManagementModel);
		}

		public async Task<ActionResult> EnableDisableAccount(AccountManagementModel accountManagementModel)
		{
			if (accountManagementModel.IsEnabled)
			{
				_commandBus.Send(new DisableAccountByAdmin { AccountId = accountManagementModel.Id });
			}
			else
			{
				_commandBus.Send(new EnableAccountByAdmin { AccountId = accountManagementModel.Id });
			}

			return View("Index", accountManagementModel);
		}

		public async Task<ActionResult> UnlinkIBSAccount(AccountManagementModel accountManagementModel)
		{
			_commandBus.Send(new UnlinkAccountFromIbs { AccountId = accountManagementModel.Id });
			return View("Index", accountManagementModel);
		}

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

			return View("Index", accountManagementModel);
		}

	}
}