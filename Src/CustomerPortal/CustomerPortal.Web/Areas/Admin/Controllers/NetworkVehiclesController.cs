#region

using System;
using System.Linq;
using System.Web.Mvc;
using CustomerPortal.Web.Areas.Admin.Models;
using CustomerPortal.Web.Entities.Network;
using MongoRepository;

#endregion


namespace CustomerPortal.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = RoleName.Admin)]
    public class NetworkVehiclesController : NetworkVehiclesBaseController
    {
        public NetworkVehiclesController()
            : this(new MongoRepository<NetworkVehicle>())
        {
        }

        public NetworkVehiclesController(IRepository<NetworkVehicle> repository)
            : base(repository)
        {
        }

        public ActionResult Index()
        {
            // Get all unique market
            var uniqueMarkets = Repository.Select(x => x.Market)
                .Distinct()
                .ToArray();

            return View(uniqueMarkets.Select(market => new MarketModel { Market = market }));
        }

        public ActionResult VehicleIndex(MarketModel marketModel)
        {
            // Find all vehicle type for this market
            var networkVehicles = Repository.Where(v => v.Market == marketModel.Market);

            return View(new NetworkVehiclesModel{ Market = marketModel.Market, Vehicles = networkVehicles });
        }

        public ActionResult CreateMarket()
        {
            return View(new MarketModel());
        }

        [HttpPost]
        public ActionResult CreateMarket(MarketModel marketModel)
        {
            var networkVehicles = Repository.Where(v => v.Market == marketModel.Market);

            // If there already are vehicles in the market, that means that it already exists
            if (networkVehicles.Any())
            {
                ViewBag.Error = "A market with that name already exists.";

                return View(new MarketModel());
            }

            return RedirectToAction("CreateVehicle", marketModel);
        }

        public ActionResult DeleteMarket(MarketModel marketModel)
        {
            try
            {
                Repository.Delete(v => v.Market == marketModel.Market);
            }
            catch (Exception)
            {
                ViewBag.Error = "An error occured. Unable to delete the market";
            }
            
            return RedirectToAction("Index");
        }

        public ActionResult CreateVehicle(MarketModel marketModel)
        {
            return View(new NetworkVehicle { Market = marketModel.Market });
        }

        [HttpPost]
        public ActionResult CreateVehicle(NetworkVehicle networkVehicle)
        {
            networkVehicle.Id = Guid.NewGuid().ToString();

            // Generate the next sequential vehicle network id
            var lastNetworkVehicleId = Repository
                .OrderBy(x => x.NetworkVehicleId)
                .Last()
                .NetworkVehicleId;

            networkVehicle.NetworkVehicleId = lastNetworkVehicleId + 1;

            try
            {
                Repository.Add(networkVehicle);
            }
            catch (Exception)
            {
                ViewBag.Error = "An error occured. Unable to create the vehicle.";

                return View(networkVehicle);
            }

            return RedirectToAction("VehicleIndex", new MarketModel { Market = networkVehicle.Market });
        }

        public ActionResult EditVehicle(string id)
        {
            var networkVehicle = Repository.GetById(id);

            return View(networkVehicle);
        }

        [HttpPost]
        public ActionResult EditVehicle(NetworkVehicle networkVehicle)
        {
            try
            {
                Repository.Update(networkVehicle);
            }
            catch (Exception)
            {
                ViewBag.Error = "An error occured. Unable to update the vehicle type.";

                return View(networkVehicle);
            }

            return RedirectToAction("VehicleIndex", new MarketModel { Market = networkVehicle.Market });
        }

        public ActionResult DeleteVehicle(string id)
        {
            var vehicleToDelete = Repository.GetById(id);
            if (vehicleToDelete == null)
            {
                throw new Exception(string.Format("Vehicle with Id {0} doesn't exist", id));
            }

            try
            {
                Repository.Delete(vehicleToDelete);
            }
            catch (Exception)
            {
                ViewBag.Error = "An error occured. Unable to delete the vehicle.";
            }

            return RedirectToAction("VehicleIndex", new MarketModel { Market = vehicleToDelete.Market });
        }
    }
}