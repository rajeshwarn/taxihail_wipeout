using apcurium.MK.Booking.Comparer;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.Areas.AdminTH.Models;
using apcurium.MK.Web.Attributes;
using ServiceStack.CacheAccess;
using ServiceStack.ServiceModel;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace apcurium.MK.Web.Areas.AdminTH.Controllers
{
	[AuthorizationRequired(RoleName.Admin)]
	public class AccountsManagementController : ServiceStackController
	{
		private readonly IAccountDao _accountDao;

		public AccountsManagementController(ICacheClient cache, IServerSettings serverSettings, IAccountDao accountDao)
			: base(cache, serverSettings)
		{
			_accountDao = accountDao;
		}

		public ActionResult Index()
		{
			// first time we open Index without values
			return View(new AccountsManagement());
		}

		// POST: AdminTH/AccountsManagement/Search
		[HttpPost]
		[ValidateInput(false)]
		public async Task<ActionResult> Search(FormCollection form)
		{
			var formDictionary = form.ToDictionary();
			formDictionary.Remove("__RequestVerificationToken");

			string searchCriteriaValue;
			formDictionary.TryGetValue("SearchCriteria", out searchCriteriaValue);
			if (string.IsNullOrEmpty(searchCriteriaValue))
			{
				TempData["SearchCriteriaEmpty"] = "Search criteria field should not be empty.";
				return RedirectToAction("Index");
			}
			else
			{
				var accountsManagement = new AccountsManagement();
				accountsManagement.SearchCriteria = searchCriteriaValue;

				accountsManagement.AccountsDetail = _accountDao.FindByNamePattern(accountsManagement.SearchCriteria)
					.Concat<AccountDetail>(_accountDao.FindByPhoneNumberPattern(accountsManagement.SearchCriteria))
					.Concat<AccountDetail>(_accountDao.FindByEmailPattern(accountsManagement.SearchCriteria))
					.Distinct<AccountDetail>(new AccountDetailComparer())
					.ToArray();

				return View("Index", accountsManagement);
			}
		}
	}
}