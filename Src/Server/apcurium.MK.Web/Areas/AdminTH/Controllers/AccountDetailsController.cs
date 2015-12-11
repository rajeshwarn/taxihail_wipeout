using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.Areas.AdminTH.Models;
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
	public class AccountDetailsController : ServiceStackController
	{
		private readonly IAccountDao _accountDao;
		private readonly ICreditCardDao _creditCardDao;
		private Guid _id;

		public AccountDetailsController(ICacheClient cache, IServerSettings serverSettings, IAccountDao accountDao, ICreditCardDao creditCardDao)
			: base(cache, serverSettings)
		{
			_accountDao = accountDao;
			_creditCardDao = creditCardDao;
		}

		// GET: AdminTH/AccountDetails
		public ActionResult Index(Guid id)
		{
			_id = id;
			var accountDetail = _accountDao.FindById(_id);
			var accountDetails = new AccountDetails();

			accountDetails.Id = id;
			accountDetails.Name = accountDetail.Name;
			accountDetails.Email = accountDetail.Email;
			accountDetails.CustomerNumber = accountDetail.Settings.CustomerNumber;
			accountDetails.CreationDate = accountDetail.CreationDate;
			accountDetails.FacebookAccount = accountDetail.FacebookId;
			accountDetails.IBSAccountId = accountDetail.IBSAccountId;
			accountDetails.IsConfirmed = accountDetail.IsConfirmed;
			accountDetails.IsEnabled = !accountDetail.DisabledByAdmin;
			accountDetails.PhoneNumber = accountDetail.Settings.Phone;
			accountDetails.ChargeType = accountDetail.Settings.ChargeType;
			accountDetails.DefaultTipPercent = accountDetail.DefaultTipPercent;
			accountDetails.IsPayPalAccountLinked = accountDetail.IsPayPalAccountLinked;
			accountDetails.CreditCardLast4Digits = _creditCardDao.FindById(accountDetail.DefaultCreditCard.GetValueOrDefault()).Last4Digits;

			return View(accountDetails);
		}

		[HttpPost]
		[ValidateInput(false)]
		public async Task<ActionResult> Save(FormCollection form)
		{
			var formDictionary = form.ToDictionary();

			var accountDetail = _accountDao.FindById(_id);

			return View("Index", new AccountDetails());
		}
	}
}