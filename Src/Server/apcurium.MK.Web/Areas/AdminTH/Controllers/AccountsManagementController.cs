using apcurium.MK.Booking.Comparer;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common;
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
        public async Task<ActionResult> Search(string searchCriteria)
        {
            if (string.IsNullOrEmpty(searchCriteria))
            {
                TempData["SearchCriteriaEmpty"] = "Search criteria field should not be empty.";
                return RedirectToAction("Index");
            }
            else
            {
                var accountsManagement = new AccountsManagementModel();
                accountsManagement.SearchCriteria = searchCriteria;

                accountsManagement.AccountsDetail = _accountDao.FindByNamePattern(accountsManagement.SearchCriteria)
                   .Concat<AccountDetail>(_accountDao.FindByPhoneNumberPattern(accountsManagement.SearchCriteria))
                   .Concat<AccountDetail>(_accountDao.FindByEmailPattern(accountsManagement.SearchCriteria))
                   .Distinct<AccountDetail>(new AccountDetailComparer())
                   .ToArray();

                accountsManagement.CountryDialCode = new string[accountsManagement.AccountsDetail.Length];
                int idx = 0;
                foreach (AccountDetail accoutDetail in accountsManagement.AccountsDetail)
                {
                    CountryCode countryCode = new CountryCode() { CountryISOCode = accoutDetail.Settings.Country };
                    accountsManagement.CountryDialCode[idx++] = countryCode.СountryDialCodeInternationalFormat;
                }

                return View("Index", accountsManagement);
            }
        }
    }
}