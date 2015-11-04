#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using apcurium.MK.Common.Extensions;
using CustomerPortal.Web.Areas.Admin.Models;
using CustomerPortal.Web.Entities.Network;
using MongoRepository;

#endregion


namespace CustomerPortal.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = RoleName.Admin)]
    public class NetworkVehiclesController : Controller
    {
        private IRepository<Market> Repository { get; set; }

        public NetworkVehiclesController()
            : this(new MongoRepository<Market>())
        {
        }

        public NetworkVehiclesController(IRepository<Market> repository)
        {
            Repository = repository;
        }

        public ActionResult Index()
        {
            // Get all unique market
            var uniqueMarkets = Repository.ToArray();

            return View(uniqueMarkets.Select(market => new MarketModel { Market = market.Name }));
        }

        public ActionResult VehicleIndex(MarketModel marketModel)
        {
            // Find all vehicle type for this market
            var market = Repository.First(x => x.Name == marketModel.Market);

            return View(new MarketModel { Market = marketModel.Market, Vehicles = market.Vehicles });
        }

        public ActionResult CreateMarket()
        {
            return View(new MarketModel());
        }

        [HttpPost]
        public ActionResult CreateMarket(MarketModel marketModel)
        {
            var existingMarket = Repository.FirstOrDefault(v => v.Name == marketModel.Market);
            if (existingMarket != null)
            {
                ViewBag.Error = "A market with that name already exists.";

                return View(new MarketModel());
            }

            Repository.Add(new Market
            {
                Id = Guid.NewGuid().ToString(),
                Name = marketModel.Market
            });

            return RedirectToAction("VehicleIndex", marketModel);
        }

        public ActionResult DeleteMarket(MarketModel marketModel)
        {
            try
            {
                Repository.Delete(v => v.Name == marketModel.Market);
            }
            catch (Exception)
            {
                ViewBag.Error = "An error occured. Unable to delete the market";
            }
            
            return RedirectToAction("Index");
        }

        public ActionResult CreateVehicle(MarketModel marketModel)
        {
            return View(new VehicleModel { Market = marketModel.Market });
        }

        [HttpPost]
        public ActionResult CreateVehicle(VehicleModel networkVehicle)
        {
            networkVehicle.Id = Guid.NewGuid().ToString();

            // Generate the next sequential vehicle network id
            var allNetworkVehicles = new List<Vehicle>();
            foreach (var market in Repository)
            {
                allNetworkVehicles.AddRange(market.Vehicles);
            }

            var nextNetworkVehicleId = 0;
            if (allNetworkVehicles.Any())
            {
                nextNetworkVehicleId = allNetworkVehicles
                    .OrderBy(x => x.NetworkVehicleId)
                    .Last()
                    .NetworkVehicleId + 1;
            }
            networkVehicle.NetworkVehicleId = nextNetworkVehicleId;

            try
            {
                var marketRepresentation = Repository.First(x => x.Name == networkVehicle.Market);
                marketRepresentation.Vehicles.Add(networkVehicle.SelectOrDefault(x => new Vehicle
                {
                    Id = x.Id,
                    Name = x.Name,
                    LogoName = x.LogoName,
                    MaxNumberPassengers = x.MaxNumberPassengers,
                    NetworkVehicleId = x.NetworkVehicleId
                }));
                Repository.Update(marketRepresentation);
            }
            catch (Exception)
            {
                ViewBag.Error = "An error occured. Unable to create the vehicle.";

                return View(networkVehicle);
            }

            return RedirectToAction("VehicleIndex", new MarketModel { Market = networkVehicle.Market });
        }

        public ActionResult EditVehicle(string market, string id)
        {
            var marketContainingVehicleToEdit = Repository.First(x => x.Name == market);
            var networkVehicle = marketContainingVehicleToEdit.Vehicles.First(x => x.Id == id).SelectOrDefault(x => new VehicleModel
            {
                Market = marketContainingVehicleToEdit.Name,
                Id = x.Id,
                Name = x.Name,
                LogoName = x.LogoName,
                MaxNumberPassengers = x.MaxNumberPassengers,
                NetworkVehicleId = x.NetworkVehicleId
            });

            return View(networkVehicle);
        }

        [HttpPost]
        public ActionResult EditVehicle(VehicleModel networkVehicle)
        {
            try
            {
                var marketContainingVehicle = Repository.First(x => x.Name == networkVehicle.Market);
                var existingVehicle = marketContainingVehicle.Vehicles.FirstOrDefault(x => x.Id == networkVehicle.Id);
                marketContainingVehicle.Vehicles.Remove(existingVehicle);
                marketContainingVehicle.Vehicles.Add(networkVehicle.SelectOrDefault(x => new Vehicle
                {
                    Id = x.Id,
                    Name = x.Name,
                    LogoName = x.LogoName,
                    MaxNumberPassengers = x.MaxNumberPassengers,
                    NetworkVehicleId = x.NetworkVehicleId
                }));
                Repository.Update(marketContainingVehicle);
            }
            catch (Exception)
            {
                ViewBag.Error = "An error occured. Unable to update the vehicle type.";

                return View(networkVehicle);
            }

            return RedirectToAction("VehicleIndex", new MarketModel { Market = networkVehicle.Market });
        }

        public ActionResult DeleteVehicle(string market, string id)
        {
            var marketContainingVehicleToDelete = Repository.First(x => x.Name == market);
            if (marketContainingVehicleToDelete == null)
            {
                throw new Exception(string.Format("Vehicle with Id {0} doesn't exist", id));
            }

            var vehicleToDelete = marketContainingVehicleToDelete.Vehicles.First(x => x.Id == id);
            if (vehicleToDelete == null)
            {
                throw new Exception(string.Format("Vehicle with Id {0} doesn't exist", id));
            }

            try
            {
                marketContainingVehicleToDelete.Vehicles.Remove(vehicleToDelete);
                Repository.Update(marketContainingVehicleToDelete);
            }
            catch (Exception)
            {
                ViewBag.Error = "An error occured. Unable to delete the vehicle.";
            }

            return RedirectToAction("VehicleIndex", new MarketModel { Market = market });
        }
    }
}