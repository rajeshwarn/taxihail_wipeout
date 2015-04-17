#region

using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using CustomerPortal.Web.Areas.Admin.Models;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Entities.Network;
using CustomerPortal.Web.Security;
using ExtendedMongoMembership;
using MongoDB.Bson;
using MongoRepository;
using WebMatrix.WebData;

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
            var uniqueMarkets = Repository.Select(x => x.Market).Distinct().ToArray();

            return View(uniqueMarkets);
        }

        public ActionResult VehicleIndex(string market)
        {
            // Find all vehicle type for this market

            return View();
        }

        public ActionResult CreateMarket()
        {
            return View(new MarketModel());
        }

        [HttpPost]
        public ActionResult CreateMarket(MarketModel marketModel)
        {
            return RedirectToAction("CreateVehicle", marketModel);
        }

        public ActionResult DeleteMarket(string market)
        {
            // TODO: Delete Confirmation

            try
            {
                Repository.Delete(v => v.Market == market);
            }
            catch (Exception)
            {
                ViewBag.Error = "An error occured. Unable to delete the market";
            }
            
            return RedirectToAction("Index");
        }

        public ActionResult CreateVehicle(MarketModel market)
        {
            return View(new NetworkVehicle{ Market = market.Market });
        }

        [HttpPost]
        public ActionResult CreateVehicle(NetworkVehicle vehicle)
        {
            vehicle.Id = Guid.NewGuid().ToString();
            Repository.Add(vehicle);

            return RedirectToAction("CreateVehicle", vehicle.Market);
        }

        public ActionResult EditVehicle(string vehicleId)
        {
            var networkVehicle = Repository.GetById(vehicleId);

            return View(networkVehicle);
        }

        [HttpPost]
        public ActionResult EditVehicle(NetworkVehicle updatedVehicle)
        {
            // TODO: check if can just use updatedVehicle

            try
            {
                var vehicleToEdit = Repository.GetById(updatedVehicle.Id);
                vehicleToEdit.Name = updatedVehicle.Name;
                vehicleToEdit.Market = updatedVehicle.Market;
                vehicleToEdit.LogoName = updatedVehicle.LogoName;
                vehicleToEdit.MaxNumberPassengers = updatedVehicle.MaxNumberPassengers;

                Repository.Update(vehicleToEdit);
            }
            catch (Exception)
            {
                ViewBag.Error = "An error occured. Unable to update the vehicle type.";
                return RedirectToAction(updatedVehicle.Id);
            }

            return RedirectToAction("VehicleIndex");
        }

        public ActionResult DeleteVehicle(Guid vehicleId)
        {
            try
            {
                Repository.Delete(v => v.Id == vehicleId.ToString());
            }
            catch (Exception)
            {
                ViewBag.Error = "An error occured. Unable to delete the vehicle.";
            }
            
            return RedirectToAction("VehicleIndex");
        }
    }
}