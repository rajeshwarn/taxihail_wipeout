using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.Areas.AdminTH.Models;
using apcurium.MK.Web.Attributes;
using ServiceStack.CacheAccess;
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

		//private AccountsManagement GetUsersForSearchCriteria()
		//{
		//	var settings = _accountDao.FindByNamePattern();
		//	return new AccountsManagement();
		//}

		// POST: AdminTH/AccountsManagement/Search
		[HttpPost]
		[ValidateInput(false)]
		public async Task<ActionResult> Search(FormCollection form)
		{
			if(!form.HasKeys())
			{
				TempData["SearchCriteriaEmpty"] = "Search criteria field should not be empty.";
				return RedirectToAction("Index");
			}

			var accountsManagement = new AccountsManagement();
			accountsManagement.SearchCriteria = form.GetValue("searchCriteria").AttemptedValue;

			return RedirectToAction("Index");
		}
	}
}