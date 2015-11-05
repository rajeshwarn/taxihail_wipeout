﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using apcurium.MK.Common.Extensions;
using CustomerPortal.Web.Areas.Admin.Models;
using CustomerPortal.Web.Entities.Network;
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

        public ActionResult VehicleIndex(MarketModel marketModel)
        {
            // Find all vehicle type for this market
            var market = GetMarket(marketModel.Market);
            
            return View(new MarketModel { Market = marketModel.Market, Vehicles = market.Vehicles });
        }

        public ActionResult CreateMarket()
        {
            return View(new MarketModel());
        }

        [HttpPost]
        public ActionResult CreateMarket(MarketModel marketModel)
        {
            var existing = GetMarket(marketModel.Market);

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

            return RedirectToAction("VehicleIndex", marketModel);
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

        public ActionResult CreateVehicle(MarketModel marketModel)
        {
            return View(new VehicleModel { Market = marketModel.Market });
        }
        
        [HttpPost]
        public ActionResult CreateVehicle(VehicleModel networkVehicle)
        {
            try
            {
                var marketRepresentation = GetMarket(networkVehicle.Market);
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

            return RedirectToAction("VehicleIndex", new MarketModel { Market = networkVehicle.Market });
        }

        public ActionResult EditVehicle(string market, string id)
        {
            var marketContainingVehicle = GetMarket(market);
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
                var marketContainingVehicle = GetMarket(networkVehicle.Market);
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

            return RedirectToAction("VehicleIndex", new MarketModel { Market = networkVehicle.Market });
        }

        public ActionResult DeleteVehicle(string market, string id)
        {
            try
            {
                var marketContainingVehicle = GetMarket(market);
                var vehicleToDelete = marketContainingVehicle.Vehicles.First(x => x.Id == id);
                marketContainingVehicle.Vehicles.Remove(vehicleToDelete);
                Repository.Update(marketContainingVehicle);
            }
            catch (Exception)
            {
                ViewBag.Error = "An error occured. Unable to delete the vehicle.";
            }

            return RedirectToAction("VehicleIndex", new MarketModel { Market = market });
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

        private Market GetMarket(string market)
        {
            var nameQuery = Query<Market>.Matches(x => x.Name, new BsonRegularExpression(market, "i"));
            return Repository.Collection.FindOne(nameQuery);
        }
    }
}