using System.Web.Mvc;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Web.Areas.Company.Controllers
{
    public class TermsAndConditionsController : BaseController
    {
        private readonly ICompanyDao _companyDao;

        public TermsAndConditionsController(ICompanyDao companyDao, IServerSettings serverSettings) 
            : base(serverSettings)
        {
            _companyDao = companyDao;
        }

        // GET: Company/Privacy
        public ActionResult Index()
        {
            return View(_companyDao.Get());
        }
    }
}