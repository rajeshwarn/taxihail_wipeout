using System.Web.Mvc;
using CustomerPortal.Web.Entities.Network;
using MongoRepository;

namespace CustomerPortal.Web.Areas.Admin.Controllers
{
    public class NetworkVehiclesBaseController : Controller
    {
        public NetworkVehiclesBaseController(IRepository<NetworkVehicle> repository)
        {
            Repository = repository;
        }

        protected IRepository<NetworkVehicle> Repository { get; set; }
    }
}