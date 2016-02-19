using System.Web.Mvc;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Security;
using apcurium.MK.Common;
using apcurium.MK.Common.Caching;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Web.Attributes;
using Infrastructure.Messaging;

namespace apcurium.MK.Web.Areas.AdminTH.Controllers
{
    [AuthorizationRequired(RoleName.Admin)]
    public class PrivacyController : ServiceStackController
    {
        private readonly ICompanyDao _companyDao;
        private readonly ICommandBus _commandBus;

        public PrivacyController(ICacheClient cache, IServerSettings serverSettings, ICompanyDao companyDao, ICommandBus commandBus)
            : base(cache, serverSettings)
        {
            _companyDao = companyDao;
            _commandBus = commandBus;
        }

        // GET: AdminTH/Privacy
        public ActionResult Index()
        {
            return View(_companyDao.Get());
        }

        // POST: AdminTH/Privacy/Update
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(string privacyPolicy)
        {
            _commandBus.Send(new UpdatePrivacyPolicy
            {
                CompanyId = AppConstants.CompanyId,
                Policy = privacyPolicy
            });

            TempData["Info"] = "Privacy policy updated";
            TempData["PrivacyPolicy"] = privacyPolicy;

            return RedirectToAction("Index");
        }
    }
}