#region

using System.Web.Mvc;
using CustomerPortal.Web.Entities;
using MongoRepository;

#endregion

namespace CustomerPortal.Web.Areas.Admin.Controllers
{
    public class CompanyControllerBase : Controller
    {
        public CompanyControllerBase(IRepository<Company> repository)
        {
            Repository = repository;
        }

        protected IRepository<Company> Repository { get; private set; }
    }
}