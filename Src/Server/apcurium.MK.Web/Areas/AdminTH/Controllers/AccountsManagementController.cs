using apcurium.MK.Booking.Comparer;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.Areas.AdminTH.Models;
using apcurium.MK.Web.Attributes;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using apcurium.MK.Common.Caching;

namespace apcurium.MK.Web.Areas.AdminTH.Controllers
{
    [AuthorizationRequired(RoleName.Support)]
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
            return View(new AccountsManagementModel());
        }

        // POST: AdminTH/AccountsManagement/Search
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Search(string searchCriteria)
        {
            if (string.IsNullOrEmpty(searchCriteria))
            {
                TempData["SearchCriteriaEmpty"] = "Search criteria field should not be empty.";
                return RedirectToAction("Index");
            }

            var accountsManagement = new AccountsManagementModel { SearchCriteria = searchCriteria };

            accountsManagement.Accounts = _accountDao.FindByNamePattern(accountsManagement.SearchCriteria)
                .Concat(_accountDao.FindByPhoneNumberPattern(accountsManagement.SearchCriteria))
                .Concat(_accountDao.FindByEmailPattern(accountsManagement.SearchCriteria))
                .Distinct(new AccountDetailComparer())
                .OrderBy(x => x.Name)
                .ToArray();

            accountsManagement.CountryDialCode = new string[accountsManagement.Accounts.Length];
            var idx = 0;
            foreach (var accoutDetail in accountsManagement.Accounts)
            {
                var countryCode = new CountryCode { CountryISOCode = accoutDetail.Settings.Country };
                accountsManagement.CountryDialCode[idx++] = countryCode.СountryDialCodeInternationalFormat;
            }

            return View("Index", accountsManagement);
        }
    }
}