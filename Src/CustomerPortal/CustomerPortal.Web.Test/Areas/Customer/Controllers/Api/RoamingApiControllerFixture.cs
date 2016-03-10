using System.Collections.Generic;
using System.Linq;
using CustomerPortal.Contract.Resources;
using CustomerPortal.Contract.Response;
using CustomerPortal.Web.Areas.Customer.Controllers.Api;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Entities.Network;
using CustomerPortal.Web.Test.Helpers;
using CustomerPortal.Web.Test.Helpers.Repository;
using MongoDB.Driver;
using MongoRepository;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CustomerPortal.Web.Test.Areas.Customer.Controllers.Api
{
    public class RoamingApiControllerFixture
    {
        private TaxiHailNetworkSettings _chrisTaxi;
        private TaxiHailNetworkSettings _chrisTaxiBis;
        private TaxiHailNetworkSettings _tonyTaxi;
        private TaxiHailNetworkSettings _tomTaxi;
        private TaxiHailNetworkSettings _pilouTaxi;
        private TaxiHailNetworkSettings _lastTaxi;
        private TaxiHailNetworkSettings _bobTaxi;
        private TaxiHailNetworkSettings _blacklistedTaxi;
        private TaxiHailNetworkSettings _taxiWhenBlacklistedProhibited;

        private Company _chrisTaxiCompany;
        private Company _chrisTaxiBisCompany;
        private Company _tonyTaxiCompany;
        private Company _tomTaxiCompany;
        private Company _pilouTaxiCompany;
        private Company _lastTaxiCompany;
        private Company _bobTaxiCompany;
        private Company _blacklistedTaxiCompany;
        private Company _taxiWhenBlacklistedProhibitedCompany;
        
        [SetUp]
        public void Setup()
        {
            NetworkRepository = new InMemoryRepository<TaxiHailNetworkSettings>();
            CompanyRepository = new InMemoryRepository<Company>();
            
            Sut = new RoamingApiController(NetworkRepository, CompanyRepository, GetMockedMarketRepo());

            _chrisTaxi = new TaxiHailNetworkSettings
            {
                Id = "ChrisTaxi",
                IsInNetwork = true,
                Market = "MTL",
                FleetId = 424242,
                WhiteListedFleetIds = "987564,321674,23134,88784,99999", // ChrisBis, Tom, Tony, Pilou, Last. Bob exclued
                Region = new MapRegion
                {
                    CoordinateStart = new MapCoordinate { Latitude = 45.514466, Longitude = -73.846313 }, // MTL Top left 
                    CoordinateEnd = new MapCoordinate { Latitude = 45.411296, Longitude = -73.513314 } // MTL BTM Right
                },
                Preferences = new List<CompanyPreference>
                {
                    new CompanyPreference{CompanyKey = "ChrisTaxiBis", CanAccept = true, CanDispatch = true, Order = 2},
                    new CompanyPreference{CompanyKey = "TomTaxi", CanAccept = true, CanDispatch = false, Order = 0},
                    new CompanyPreference{CompanyKey = "PilouTaxi", CanAccept = true, CanDispatch = true, Order = 1}
                }
            }; 

            _chrisTaxiBis = new TaxiHailNetworkSettings
            {
                Id = "ChrisTaxiBis",
                IsInNetwork = true,
                Market = "MTL",
                FleetId = 987564,
                Region = new MapRegion
                {
                    CoordinateStart = new MapCoordinate { Latitude = 45.514466, Longitude = -73.846313 }, // MTL Top left 
                    CoordinateEnd = new MapCoordinate { Latitude = 45.41129, Longitude = -73.51331 } // MTL BTM Right

                },
                Preferences = new List<CompanyPreference>
                {
                    new CompanyPreference{CompanyKey = "ChrisTaxi",CanAccept = true,CanDispatch = true},
                    new CompanyPreference{CompanyKey = "TomTaxi",CanAccept = true,CanDispatch = true},
                    new CompanyPreference{CompanyKey = "PilouTaxi",CanAccept = true,CanDispatch = true},
                }
            };

            _tonyTaxi = new TaxiHailNetworkSettings
            {
                Id = "TonyTaxi",
                IsInNetwork = true,
                Market = "CHI",
                FleetId = 321674,
                Region = new MapRegion
                {
                    CoordinateStart = new MapCoordinate { Latitude = 49994, Longitude = -73.656990 }, // Apcuruium 
                    CoordinateEnd = new MapCoordinate { Latitude = 45.474307, Longitude = -73.58412 } // Home
                }
            };

            _tomTaxi = new TaxiHailNetworkSettings
            {
                //same Longitude as TonyTaxi
                Id = "TomTaxi",
                Market = "CHI",
                FleetId = 23134,
                IsInNetwork = true,
                Region = new MapRegion()
                {
                    CoordinateStart = new MapCoordinate { Latitude = 5000000, Longitude = -73.656990 },
                    CoordinateEnd = new MapCoordinate { Latitude = 45.43874, Longitude = -73.58412 }
                },
                Preferences = new List<CompanyPreference>
                {
                    new CompanyPreference{CompanyKey = "ChrisTaxiBis",CanAccept = true,CanDispatch = true},
                    new CompanyPreference{CompanyKey = "ChrisTaxi",CanAccept = false,CanDispatch = true},
                    new CompanyPreference{CompanyKey = "PilouTaxi",CanAccept = true,CanDispatch = true},
                }
            };

            _pilouTaxi = new TaxiHailNetworkSettings
            {
                //Same Latitude as ChrisTaxi and Chris TaxiBis
                Id = "PilouTaxi",
                IsInNetwork = true,
                Market = "NYC",
                FleetId = 88784,
                WhiteListedFleetIds = "444444", // Random one to test BobTaxi exclusion
                Region = new MapRegion
                {
                    CoordinateStart = new MapCoordinate { Latitude = 45.514466, Longitude = -73.889451 },
                    CoordinateEnd = new MapCoordinate { Latitude = 45.411296, Longitude = -73.496042 }
                },
                Preferences = new List<CompanyPreference>
                {
                    new CompanyPreference{CompanyKey = "ChrisTaxiBis",CanAccept = true,CanDispatch = true},
                    new CompanyPreference{CompanyKey = "ChrisTaxi",CanAccept = false,CanDispatch = true},
                    new CompanyPreference{CompanyKey = "TomTaxi",CanAccept = true,CanDispatch = true},
                    new CompanyPreference{CompanyKey = "BobTaxi",CanAccept = true,CanDispatch = true},
                }
            };

            _lastTaxi = new TaxiHailNetworkSettings
            {
                //Overlap ChrisTaxi and ChrisTaxiBis
                Id = "LastTaxi",
                IsInNetwork = true,
                Market = "SYD",
                FleetId = 99999,
                BlackListedFleetIds = "10101019, 666, 010101",
                Region = new MapRegion
                {
                    CoordinateStart = new MapCoordinate { Latitude = 45.420595, Longitude = -75.708386 }, // Ottawa
                    CoordinateEnd = new MapCoordinate { Latitude = 45.411045, Longitude = -75.684568 }
                }
            };

            _bobTaxi = new TaxiHailNetworkSettings
            {
                Id = "BobTaxi",
                IsInNetwork = true,
                Market = "NYC",
                FleetId = 587564,
                Region = new MapRegion
                {
                    CoordinateStart = new MapCoordinate { Latitude = 45.514466, Longitude = -73.846313 }, // MTL Top left 
                    CoordinateEnd = new MapCoordinate { Latitude = 45.41129, Longitude = -73.51331 } // MTL BTM Right

                },
                Preferences = new List<CompanyPreference>
                {
                    new CompanyPreference{CompanyKey = "ChrisTaxi",CanAccept = true,CanDispatch = true},
                    new CompanyPreference{CompanyKey = "TomTaxi",CanAccept = true,CanDispatch = true},
                    new CompanyPreference{CompanyKey = "PilouTaxi",CanAccept = true,CanDispatch = true},
                }
            };



            _taxiWhenBlacklistedProhibited = new TaxiHailNetworkSettings
            {
                Id = "TaxiWhenBlacklistedProhibited",
                IsInNetwork = true,
                Market = "SYD",
                FleetId = 88784,
                BlackListedFleetIds = "666",
                Region = new MapRegion
                {
                    CoordinateStart = new MapCoordinate { Latitude = 45.563135, Longitude = -73.71953 }, //College Montmorency Laval
                    CoordinateEnd = new MapCoordinate { Latitude = 45.498094, Longitude = -73.62233 } //Station cote des neiges
                },
                Preferences = new List<CompanyPreference>
                {
                    new CompanyPreference{CompanyKey = "BlacklistedTaxi",CanAccept = true,CanDispatch = true},
                }
            };

            _blacklistedTaxi = new TaxiHailNetworkSettings()
            {
                Id = "BlacklistedTaxi",
                IsInNetwork = true,
                Market = "SYD",
                FleetId = 666,
                Region = new MapRegion()
                {
                    CoordinateStart = new MapCoordinate { Latitude = 45.563135, Longitude = -73.71953 }, //College Montmorency Laval
                    CoordinateEnd = new MapCoordinate { Latitude = 45.498094, Longitude = -73.62233 } //Station cote des neiges
                },
                Preferences = new List<CompanyPreference>
                {
                    new CompanyPreference{CompanyKey = "TaxiWhenBlacklistedProhibited",CanAccept = true,CanDispatch = true},
                }
            };


            _chrisTaxiCompany = new Company
            {
                Id = "ChrisTaxi",
                CompanyKey = "ChrisTaxi",
                CompanyName = "ChrisTaxi",
                IBS = new IBSSettings
                {
                    Password = "test",
                    Username = "Taxi",
                    ServiceUrl = "http://google.com"
                }
            };
            _chrisTaxiBisCompany = new Company
            {
                Id = "ChrisTaxiBis",
                CompanyKey = "ChrisTaxiBis",
                CompanyName = "ChrisTaxiBis",
                IBS = new IBSSettings
                {
                    Password = "Bob",
                    Username = "Alice",
                    ServiceUrl = "http://altavista.com"
                }
            };
            _lastTaxiCompany = new Company
            {
                Id = "LastTaxi",
                CompanyKey = "LastTaxi",
                CompanyName = "LastTaxi",
                IBS = new IBSSettings
                {
                    Password = "test",
                    Username = "Taxi",
                    ServiceUrl = "http://google.com"
                }
            };
            _pilouTaxiCompany = new Company
            {
                Id = "PilouTaxi",
                CompanyKey = "PilouTaxi",
                CompanyName = "PilouTaxi",
                IBS = new IBSSettings
                {
                    Password = "test",
                    Username = "Taxi",
                    ServiceUrl = "http://google.com"
                }
            };
            _tomTaxiCompany = new Company
            {
                Id = "TomTaxi",
                CompanyKey = "TomTaxi",
                CompanyName = "TomTaxi",
                IBS = new IBSSettings
                {
                    Password = "test",
                    Username = "Taxi",
                    ServiceUrl = "http://google.com"
                }
            };
            _tonyTaxiCompany = new Company
            {
                Id = "TonyTaxi",
                CompanyKey = "TonyTaxi",
                CompanyName = "TonyTaxi",
                IBS = new IBSSettings
                {
                    Password = "test",
                    Username = "Taxi",
                    ServiceUrl = "http://google.com"
                }
            };
            _bobTaxiCompany = new Company
            {
                Id = "BobTaxi",
                CompanyKey = "BobTaxi",
                CompanyName = "BobTaxi",
                IBS = new IBSSettings
                {
                    Password = "Bob",
                    Username = "Alice",
                    ServiceUrl = "http://altavista.com"
                }
            };

            _blacklistedTaxiCompany = new Company()
            {
                Id = "BlacklistedTaxi",
                CompanyKey = "BlacklistedTaxi",
                CompanyName = "BlacklistedTaxi",
                IBS = new IBSSettings
                {
                    Password = "test",
                    Username = "Taxi",
                    ServiceUrl = "http://google.com"
                }
            };

            _taxiWhenBlacklistedProhibitedCompany = new Company()
            {
                Id = "TaxiWhenBlacklistedProhibited",
                CompanyKey = "TaxiWhenBlacklistedProhibited",
                CompanyName = "TaxiWhenBlacklistedProhibited",
                IBS = new IBSSettings
                {
                    Password = "test",
                    Username = "Taxi",
                    ServiceUrl = "http://google.com"
                }
            };

            NetworkRepository.DeleteAll();
            NetworkRepository.Add(_chrisTaxi);
            NetworkRepository.Add(_chrisTaxiBis);
            NetworkRepository.Add(_tonyTaxi);
            NetworkRepository.Add(_tomTaxi);
            NetworkRepository.Add(_pilouTaxi);
            NetworkRepository.Add(_lastTaxi);
            NetworkRepository.Add(_bobTaxi);
            NetworkRepository.Add(_blacklistedTaxi);
            NetworkRepository.Add(_taxiWhenBlacklistedProhibited);

            CompanyRepository.DeleteAll();
            CompanyRepository.Add(_chrisTaxiCompany);
            CompanyRepository.Add(_chrisTaxiBisCompany);
            CompanyRepository.Add(_tonyTaxiCompany);
            CompanyRepository.Add(_tomTaxiCompany);
            CompanyRepository.Add(_pilouTaxiCompany);
            CompanyRepository.Add(_lastTaxiCompany);
            CompanyRepository.Add(_bobTaxiCompany);
            CompanyRepository.Add(_blacklistedTaxiCompany);
            CompanyRepository.Add(_taxiWhenBlacklistedProhibitedCompany);
        }

        public InMemoryRepository<TaxiHailNetworkSettings> NetworkRepository { get; set; }
        public InMemoryRepository<Company> CompanyRepository { get; set; }
        public RoamingApiController Sut { get; set; }

        [Test]
        public void When_Getting_Every_Fleets_From_Every_Market()
        {
            var response = Sut.GetRoamingFleetsPreferences("ChrisTaxi");
            var json = response.Content.ReadAsStringAsync().Result;
            var fleetsPreferences =
                JsonConvert.DeserializeObject<Dictionary<string, List<CompanyPreferenceResponse>>>(json);

            Assert.NotNull(fleetsPreferences);

            Assert.AreEqual(3, fleetsPreferences.Count);
            Assert.Contains("CHI", fleetsPreferences.Keys);
            Assert.Contains("NYC", fleetsPreferences.Keys);
            Assert.Contains("SYD", fleetsPreferences.Keys);

            var chicagoFleets = fleetsPreferences["CHI"];
            Assert.AreEqual(2, chicagoFleets.Count);
            Assert.AreEqual("TomTaxi", chicagoFleets[0].CompanyPreference.CompanyKey);
            Assert.AreEqual("TonyTaxi", chicagoFleets[1].CompanyPreference.CompanyKey);

            var newYorkFleets = fleetsPreferences["NYC"];
            Assert.AreEqual(1, newYorkFleets.Count);
            Assert.AreEqual("PilouTaxi", newYorkFleets[0].CompanyPreference.CompanyKey);

            var sydneyFleets = fleetsPreferences["SYD"];
            Assert.AreEqual(2, sydneyFleets.Count);
            Assert.AreEqual("LastTaxi", sydneyFleets[0].CompanyPreference.CompanyKey);
            Assert.AreEqual("TaxiWhenBlacklistedProhibited", sydneyFleets[1].CompanyPreference.CompanyKey);
        }

        [Test]
        public void When_Getting_Company_Market_In_Home_Market()
        {
            var response = Sut.GetCompanyMarket("ChrisTaxi", 45.423513, -73.653214);
            var json = response.Content.ReadAsStringAsync().Result;
            var market = JsonConvert.DeserializeObject<string>(json);

            Assert.IsNull(market);
        }

        [Test]
        public void When_Getting_Company_Market_And_Changed_Market()
        {
            var response = Sut.GetCompanyMarket("ChrisTaxi", 45.412042, -75.695321);
            var json = response.Content.ReadAsStringAsync().Result;
            var market = JsonConvert.DeserializeObject<string>(json);

            Assert.AreEqual("SYD", market);
        }

        [Test]
        public void When_Getting_Company_Market_Settings_In_Unknown_Market()
        {
            var response = Sut.GetCompanyMarketSettings("abc", 10, -70);
            var json = response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<CompanyMarketSettingsResponse>(json);

            Assert.IsNull(result.Market);
            Assert.NotNull(result.DispatcherSettings);
            Assert.AreEqual(0, result.DispatcherSettings.NumberOfOffersPerCycle);
            Assert.AreEqual(1, result.DispatcherSettings.NumberOfCycles);
            Assert.AreEqual(15, result.DispatcherSettings.DurationOfOfferInSeconds);
            Assert.AreEqual(false, result.EnableDriverBonus);
            Assert.AreEqual(null, result.ReceiptFooter);
            Assert.AreEqual(false, result.ShowCallDriver);
        }

        [Test]
        public void When_Getting_Company_Market_Settings_In_Home_Market()
        {
            var response = Sut.GetCompanyMarketSettings("ChrisTaxi", 45.423513, -73.653214);
            var json = response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<CompanyMarketSettingsResponse>(json);

            Assert.IsNull(result.Market);
            Assert.NotNull(result.DispatcherSettings);
            Assert.AreEqual(4, result.DispatcherSettings.NumberOfOffersPerCycle);
            Assert.AreEqual(5, result.DispatcherSettings.NumberOfCycles);
            Assert.AreEqual(55, result.DispatcherSettings.DurationOfOfferInSeconds);
            Assert.AreEqual(true, result.EnableDriverBonus);
            Assert.AreEqual("my custom footer 1", result.ReceiptFooter);
            Assert.AreEqual(false, result.ShowCallDriver);
        }

        [Test]
        public void When_Getting_Company_Market_Settings_And_Changed_Market()
        {
            var response = Sut.GetCompanyMarketSettings("ChrisTaxi", 45.412042, -75.695321);
            var json = response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<CompanyMarketSettingsResponse>(json);

            Assert.AreEqual("SYD", result.Market);
            Assert.NotNull(result.DispatcherSettings);
            Assert.AreEqual(2, result.DispatcherSettings.NumberOfOffersPerCycle);
            Assert.AreEqual(3, result.DispatcherSettings.NumberOfCycles);
            Assert.AreEqual(15, result.DispatcherSettings.DurationOfOfferInSeconds);
            Assert.AreEqual(true, result.EnableDriverBonus);
            Assert.AreEqual(null, result.ReceiptFooter);
            Assert.AreEqual(true, result.ShowCallDriver);
        }

        [Test]
        public void When_Getting_Fleets_From_a_Market()
        {
            var response = Sut.GetMarketFleets("ChrisTaxi", "MTL");
            var json = response.Content.ReadAsStringAsync().Result;
            var fleets = JsonConvert.DeserializeObject<List<NetworkFleetResponse>>(json);

            Assert.AreEqual(1, fleets.Count);

            var secondFleet = fleets.FirstOrDefault(f => f.CompanyKey == "ChrisTaxiBis");
            Assert.NotNull(secondFleet);
            Assert.AreEqual("ChrisTaxiBis", secondFleet.CompanyKey);
            Assert.AreEqual("ChrisTaxiBis", secondFleet.CompanyName);
            Assert.AreEqual("http://altavista.com", secondFleet.IbsUrl);
            Assert.AreEqual("Alice", secondFleet.IbsUserName);
            Assert.AreEqual("Bob", secondFleet.IbsPassword);
        }

        [Test]
        public void When_Getting_Fleets_From_a_Market_IgnoreBlacklist()
        {
            var response = Sut.GetMarketFleets("TaxiWhenBlacklistedProhibited", "SYD");
            var json = response.Content.ReadAsStringAsync().Result;
            var fleets = JsonConvert.DeserializeObject<List<NetworkFleetResponse>>(json);

            Assert.AreEqual(0, fleets.Count);
            Assert.AreNotEqual(true, fleets.Any(t => t.CompanyKey == "BlacklistedTaxi"));
        }

        [Test]
        public void When_Getting_Fleets_From_a_Market_With_Excluded_Feet()
        {
            var response = Sut.GetMarketFleets("PilouTaxi", "NYC");
            var json = response.Content.ReadAsStringAsync().Result;
            var fleets = JsonConvert.DeserializeObject<List<NetworkFleetResponse>>(json);

            Assert.AreEqual(0, fleets.Count);
        }

        [Test]
        public void When_Getting_Single_Fleet_From_a_Market()
        {
            var response = Sut.GetMarketFleet("MTL", 424242);
            var json = response.Content.ReadAsStringAsync().Result;
            var fleet = JsonConvert.DeserializeObject<NetworkFleetResponse>(json);

            Assert.NotNull(fleet);
            Assert.AreEqual("ChrisTaxi", fleet.CompanyKey);
            Assert.AreEqual("ChrisTaxi", fleet.CompanyName);
            Assert.AreEqual("http://google.com", fleet.IbsUrl);
            Assert.AreEqual("Taxi", fleet.IbsUserName);
            Assert.AreEqual("test", fleet.IbsPassword);
        }

        private IRepository<Market> GetMockedMarketRepo()
        {
            var marketRepositoryMock = new Mock<IRepository<Market>>();

            var serverMock = MongoMock.CreateMongoServer();
            var databaseMock = MongoMock.CreateMongoDatabase(serverMock.Object);
            var collectionMock = MongoMock.CreateMongoCollection<Market>(databaseMock.Object, "FooCollection");
            var cursorMock = MongoMock.CreateMongoCursor(collectionMock.Object, new List<Market>
            {
                new Market { Name = "MTL", EnableDriverBonus = true, ReceiptFooter = "my custom footer 1", DispatcherSettings = new DispatcherSettings { NumberOfOffersPerCycle = 4, NumberOfCycles = 5, DurationOfOfferInSeconds = 55 } },
                new Market { Name = "NYC", EnableDriverBonus = false, ReceiptFooter = "my custom footer 2", DispatcherSettings = new DispatcherSettings { NumberOfOffersPerCycle = 1, NumberOfCycles = 2, DurationOfOfferInSeconds = 50 } },
                new Market { Name = "NYCSS", EnableDriverBonus = true, ReceiptFooter = "my custom footer 3", DispatcherSettings = new DispatcherSettings { NumberOfOffersPerCycle = 3, NumberOfCycles = 4, DurationOfOfferInSeconds = 60 } },
                new Market { Name = "SYD", EnableDriverBonus = true, ReceiptFooter = null, DispatcherSettings = new DispatcherSettings { NumberOfOffersPerCycle = 2, NumberOfCycles = 3, DurationOfOfferInSeconds = 15 }, ShowCallDriver = true},
                new Market { Name = "CHI", EnableDriverBonus = false, ReceiptFooter = null, DispatcherSettings = new DispatcherSettings { NumberOfOffersPerCycle = 1, NumberOfCycles = 4, DurationOfOfferInSeconds = 25 } }
            });
            collectionMock.Setup(x => x.Find(It.IsAny<IMongoQuery>())).Returns(cursorMock.Object);
            marketRepositoryMock.Setup(x => x.Collection).Returns(collectionMock.Object);

            return marketRepositoryMock.Object;
        }
    }
}
