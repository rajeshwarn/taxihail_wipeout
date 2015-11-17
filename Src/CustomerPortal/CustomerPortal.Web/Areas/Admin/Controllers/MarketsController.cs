﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using apcurium.MK.Common.Extensions;
using CustomerPortal.Web.Areas.Admin.Models;
using CustomerPortal.Web.Entities.Network;
using CustomerPortal.Web.Extensions;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using MongoRepository;

namespace CustomerPortal.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = RoleName.Admin)]
    public class MarketsController : Controller
    {
        private IRepository<Market> Repository { get; set; }

        public MarketsController()
            : this(new MongoRepository<Market>())
        {
        }

        public MarketsController(IRepository<Market> repository)
        {
            Repository = repository;
        }

        public ActionResult Index()
        {
            var allMarkets = Repository.OrderBy(x => x.Name).ToArray();
            
            return View(allMarkets.Select(market => new MarketModel { Market = market.Name }));
        }

        public ActionResult MarketIndex(MarketModel marketModel)
        {
            // Find all vehicle type for this market
            var market = Repository.GetMarket(marketModel.Market);
            if (market == null)
            {
                return View(new MarketModel());
            }

            return View(new MarketModel
            {
                Market = marketModel.Market,
                DispatcherSettings = market.DispatcherSettings,
                Vehicles = market.Vehicles,
                EnableDriverBonus = market.EnableDriverBonus
            });
        }

        public ActionResult CreateMarket()
        {
            return View(new MarketModel());
        }

        [HttpPost]
        public ActionResult CreateMarket(MarketModel marketModel)
        {
            var existing = Repository.GetMarket(marketModel.Market);

            if (existing != null)
            {
                ViewBag.Error = "A market with that name already exists.";

                return View(new MarketModel());
            }

            Repository.Add(new Market
            {
                Id = Guid.NewGuid().ToString(),
                Name = marketModel.Market
            });

            return RedirectToAction("MarketIndex", marketModel);
        }

        public ActionResult DeleteMarket(MarketModel marketModel)
        {
            var companiesUsingThisMarketInTheirNetworkSettings = new MongoRepository<TaxiHailNetworkSettings>()
                .Where(x => x.Market == marketModel.Market)
                .OrderBy(x => x.Id)
                .Select(x => x.Id)
                .ToList();

            if (companiesUsingThisMarketInTheirNetworkSettings.Any())
            {
                TempData["warning"] = "Couldn't delete the market because some companies are configured to use it:"
                                      + companiesUsingThisMarketInTheirNetworkSettings.Aggregate("",
                                          (current, company) => current + string.Format(
                                                  company == companiesUsingThisMarketInTheirNetworkSettings.Last()
                                                      ? "{0}"
                                                      : "{0}, ", company));
            }
            else
            {
                try
                {
                    Repository.Delete(v => v.Name == marketModel.Market);
                }
                catch (Exception)
                {
                    ViewBag.Error = "An error occured. Unable to delete the market";
                }
            }
            
            return RedirectToAction("Index");
        }

        public ActionResult EditDispatcherSettings(string market)
        {
            var marketRepresentation = Repository.GetMarket(market);
            return View(new MarketModel { Market = marketRepresentation.Name, DispatcherSettings = marketRepresentation.DispatcherSettings });
        }

        [HttpPost]
        public ActionResult EditDispatcherSettings(MarketModel marketModel)
        {
            try
            {
                var marketRepresentation = Repository.GetMarket(marketModel.Market);
                marketRepresentation.DispatcherSettings = marketModel.DispatcherSettings;
                Repository.Update(marketRepresentation);
            }
            catch (Exception)
            {
                ViewBag.Error = "An error occured. Unable to edit dispatcher settings.";

                return View(marketModel);
            }

            return RedirectToAction("MarketIndex", new MarketModel { Market = marketModel.Market });
        }

        public ActionResult CreateVehicle(string market)
        {
            return View(new VehicleModel { Market = market });
        }
        
        [HttpPost]
        public ActionResult CreateVehicle(VehicleModel networkVehicle)
        {
            try
            {
                var marketRepresentation = Repository.GetMarket(networkVehicle.Market);
                marketRepresentation.Vehicles.Add(new Vehicle
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = networkVehicle.Name,
                    LogoName = networkVehicle.LogoName,
                    MaxNumberPassengers = networkVehicle.MaxNumberPassengers,
                    NetworkVehicleId = GenerateNextSequentialNetworkVehicleId()
                });
                Repository.Update(marketRepresentation);
            }
            catch (Exception)
            {
                ViewBag.Error = "An error occured. Unable to create the vehicle.";

                return View(networkVehicle);
            }

            return RedirectToAction("MarketIndex", new MarketModel { Market = networkVehicle.Market });
        }

        public ActionResult EditVehicle(string market, string id)
        {
            var marketContainingVehicle = Repository.GetMarket(market);
            var networkVehicle = marketContainingVehicle.Vehicles.First(x => x.Id == id).SelectOrDefault(x => new VehicleModel
            {
                Market = marketContainingVehicle.Name,
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
                var marketContainingVehicle = Repository.GetMarket(networkVehicle.Market);
                var existingVehicle = marketContainingVehicle.Vehicles.First(x => x.Id == networkVehicle.Id);

                existingVehicle.Name = networkVehicle.Name;
                existingVehicle.LogoName = networkVehicle.LogoName;
                existingVehicle.MaxNumberPassengers = networkVehicle.MaxNumberPassengers;
                existingVehicle.NetworkVehicleId = networkVehicle.NetworkVehicleId;

                Repository.Update(marketContainingVehicle);
            }
            catch (Exception)
            {
                ViewBag.Error = "An error occured. Unable to update the vehicle type.";

                return View(networkVehicle);
            }

            return RedirectToAction("MarketIndex", new MarketModel { Market = networkVehicle.Market });
        }

        public ActionResult DeleteVehicle(string market, string id)
        {
            try
            {
                var marketContainingVehicle = Repository.GetMarket(market);
                var vehicleToDelete = marketContainingVehicle.Vehicles.First(x => x.Id == id);
                marketContainingVehicle.Vehicles.Remove(vehicleToDelete);
                Repository.Update(marketContainingVehicle);
            }
            catch (Exception)
            {
                ViewBag.Error = "An error occured. Unable to delete the vehicle.";
            }

            return RedirectToAction("MarketIndex", new MarketModel { Market = market });
        }

        public ActionResult SaveSettings(string market, bool enableDriverBonus)
        {
            try
            {
                var marketToEdit = Repository.GetMarket(market);
                if (marketToEdit == null)
                {
                    ViewBag.Error = "An error occured. Market is null.";

                    return RedirectToAction("Index");
                }

                marketToEdit.EnableDriverBonus = enableDriverBonus;
                Repository.Update(marketToEdit);
            }
            catch (Exception)
            {
                ViewBag.Error = "An error occured. Unable to save the settings.";
            }

            return RedirectToAction("Index");
        }

        private int GenerateNextSequentialNetworkVehicleId()
        {
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

            return nextNetworkVehicleId;
        }
    }
}