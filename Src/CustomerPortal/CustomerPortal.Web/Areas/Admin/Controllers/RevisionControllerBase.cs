#region

using System.Web.Mvc;
using CustomerPortal.Web.Entities;
using MongoRepository;

#endregion

namespace CustomerPortal.Web.Areas.Admin.Controllers
{
    public class RevisionControllerBase : Controller
    {
        public RevisionControllerBase(IRepository<Revision> repository)
        {
            Repository = repository;
        }

        protected IRepository<Revision> Repository { get; set; }
    }
}