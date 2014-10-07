#region

using System.Web.Mvc;
using CustomerPortal.Web.Entities;
using MongoRepository;

#endregion

namespace CustomerPortal.Web.Areas.Admin.Controllers
{
    public class DeployementControllerBase : Controller
    {
        public DeployementControllerBase(IRepository<DeploymentJob> repository)
        {
            Repository = repository;
        }

        protected IRepository<DeploymentJob> Repository { get; set; }
    }
}