using System;
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
    public class NetworkApiControllerFixture
    {
        private TaxiHailNetworkSettings _chrisTaxi;
        private TaxiHailNetworkSettings _chrisTaxiBis;
        private TaxiHailNetworkSettings _tonyTaxi;
        private TaxiHailNetworkSettings _tomTaxi;
        private TaxiHailNetworkSettings _pilouTaxi;
        private TaxiHailNetworkSettings _lastTaxi;
        private TaxiHailNetworkSettings _blacklistedTaxi;

        private Company _chrisTaxiCompany;
        private Company _chrisTaxiBisCompany;
        private Company _tonyTaxiCompany;
        private Company _tomTaxiCompany;
        private Company _pilouTaxiCompany;
        private Company _lastTaxiCompany;
        private Company _blacklistedTaxiCompany;

        const string BaseUrl = "http://localhost/CustomerPortal.Web/";
        
        [SetUp]
        public void Setup()
        {
            NetworkRepository = new InMemoryRepository<TaxiHailNetworkSettings>();
            CompanyRepository = new InMemoryRepository<Company>();

            Sut = new NetworkApiController(NetworkRepository, CompanyRepository, GetMockedMarketRepo());

            _chrisTaxi = new TaxiHailNetworkSettings()
            {
                Id = "ChrisTaxi",
                IsInNetwork = true,
                Market = "MTL",
                WhiteListedFleetIds = "123456,564321", // And excluding PilouTaxi and TomTaxi
                Preferences = new List<CompanyPreference>
                {
                    new CompanyPreference{CompanyKey = "ChrisTaxiBis",CanAccept = true,CanDispatch = true,Order = 2},
                    new CompanyPreference{CompanyKey = "TomTaxi",CanAccept = true,CanDispatch = false,Order = 0},
                    new CompanyPreference{CompanyKey = "PilouTaxi",CanAccept = true,CanDispatch = true,Order = 1},
                }
            };

            _chrisTaxiBis = new TaxiHailNetworkSettings()
            {
                Id = "ChrisTaxiBis",
                IsInNetwork = true,
                Market = "MTL",
                FleetId = 123456,
                Preferences = new List<CompanyPreference>
                {
                    new CompanyPreference{CompanyKey = "ChrisTaxi",CanAccept = false,CanDispatch = true},
                    new CompanyPreference{CompanyKey = "TomTaxi",CanAccept = true,CanDispatch = true},
                    new CompanyPreference{CompanyKey = "PilouTaxi",CanAccept = true,CanDispatch = true},
                }
            };

            _tonyTaxi = new TaxiHailNetworkSettings()
            {
                Id = "TonyTaxi",
                IsInNetwork = true,
                Market = "CHI",
                FleetId = 564321
            };

            _tomTaxi = new TaxiHailNetworkSettings()
            {
                //same Longitude as TonyTaxi
                Id = "TomTaxi",
                Market = "CHI",
                IsInNetwork = true,
                FleetId = 99999,
                Preferences = new List<CompanyPreference>
                {
                    new CompanyPreference{CompanyKey = "ChrisTaxiBis",CanAccept = true,CanDispatch = true},
                    new CompanyPreference{CompanyKey = "ChrisTaxi",CanAccept = false,CanDispatch = true},
                    new CompanyPreference{CompanyKey = "PilouTaxi",CanAccept = true,CanDispatch = true},
                }
            };

            _pilouTaxi = new TaxiHailNetworkSettings()
            {
                //Same Latitude as ChrisTaxi and Chris TaxiBis
                Id = "PilouTaxi",
                IsInNetwork = true,
                Market = "NYC",
                FleetId = 44444,
                Preferences = new List<CompanyPreference>
                {
                    new CompanyPreference{CompanyKey = "ChrisTaxiBis",CanAccept = true,CanDispatch = true},
                    new CompanyPreference{CompanyKey = "ChrisTaxi",CanAccept = false,CanDispatch = true},
                    new CompanyPreference{CompanyKey = "TomTaxi",CanAccept = true,CanDispatch = true},
                }
            };

            _lastTaxi = new TaxiHailNetworkSettings()
            {
                //Overlap ChrisTaxi and ChrisTaxiBis
                Id = "LastTaxi",
                IsInNetwork = true,
                Market = "SYD",
                BlackListedFleetIds = "10101019, 666, 010101"
            };

            _blacklistedTaxi = new TaxiHailNetworkSettings()
            {
                Id = "TaxiBlacklisted",
                IsInNetwork = true,
                Market = "SYD",
                FleetId = 666
            };
           
            _chrisTaxiCompany = new Company()
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
            _chrisTaxiBisCompany= new Company()
            {
                Id = "ChrisTaxiBis",
                CompanyKey = "ChrisTaxiBis",
                CompanyName = "ChrisTaxiBis",
                IBS = new IBSSettings
                {
                    Password = "test",
                    Username = "Taxi",
                    ServiceUrl = "http://google.com"
                }
            };
            _lastTaxiCompany = new Company()
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
            _pilouTaxiCompany = new Company()
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
            _tomTaxiCompany = new Company()
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
            _tonyTaxiCompany = new Company()
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


            NetworkRepository.DeleteAll();
            NetworkRepository.Add(_chrisTaxi);
            NetworkRepository.Add(_chrisTaxiBis);
            NetworkRepository.Add(_tonyTaxi);
            NetworkRepository.Add(_tomTaxi);
            NetworkRepository.Add(_pilouTaxi);
            NetworkRepository.Add(_lastTaxi);
            NetworkRepository.Add(_blacklistedTaxi);

            CompanyRepository.DeleteAll();
            CompanyRepository.Add(_chrisTaxiCompany);
            CompanyRepository.Add(_chrisTaxiBisCompany);
            CompanyRepository.Add(_tonyTaxiCompany);
            CompanyRepository.Add(_tomTaxiCompany);
            CompanyRepository.Add(_pilouTaxiCompany);
            CompanyRepository.Add(_lastTaxiCompany);
            CompanyRepository.Add(_blacklistedTaxiCompany);
        }

        public InMemoryRepository<TaxiHailNetworkSettings> NetworkRepository { get; set; }
        public InMemoryRepository<Company> CompanyRepository { get; set; }
        public NetworkApiController Sut { get; set; }
        
        [Test]
        public void When_Regions_Inside()
        {

            var response = Sut.Get(_chrisTaxi.Id);
             Assert.True(response.IsSuccessStatusCode);
            var json = response.Content.ReadAsStringAsync().Result;
            Assert.IsNotEmpty(JsonConvert.DeserializeObject<List<CompanyPreferenceResponse>>(json));

            var response2 = Sut.Get(_tonyTaxi.Id);
            Assert.True(response2.IsSuccessStatusCode);
            var json2 = response2.Content.ReadAsStringAsync().Result;
            Assert.IsNotEmpty(JsonConvert.DeserializeObject<List<CompanyPreferenceResponse>>(json2));
        }


        [Test]
        public void When_Regions_Identical()
        {
            var response = Sut.Get(_chrisTaxi.Id);
            Assert.True(response.IsSuccessStatusCode);
            var json = response.Content.ReadAsStringAsync().Result;

            var results = JsonConvert.DeserializeObject<List<CompanyPreferenceResponse>>(json);

            Assert.IsNotEmpty(results);

            // Should Not return ChrisTaxi
            Assert.AreNotEqual(true, results.Any(t => t.CompanyPreference.CompanyKey == "ChrisTaxi"));
        }

        [Test]
        public void When_Company_Not_In_Whitelist()
        {
            var response = Sut.Get(_chrisTaxi.Id);
            Assert.True(response.IsSuccessStatusCode);
            var json = response.Content.ReadAsStringAsync().Result;

            var results = JsonConvert.DeserializeObject<List<CompanyPreferenceResponse>>(json);

            Assert.AreEqual(1, results.Count);

            // Should Not return ChrisTaxi nor TonyTaxi
            Assert.AreNotEqual(true, results.Any(t => t.CompanyPreference.CompanyKey == "ChrisTaxi"));
            Assert.AreNotEqual(true, results.Any(t => t.CompanyPreference.CompanyKey == "PilouTaxi"));
        }


        [Test]
        public void When_Company_In_Blacklist()
        {
            var response = Sut.Get(_lastTaxi.Id);
            Assert.True(response.IsSuccessStatusCode);
            var json = response.Content.ReadAsStringAsync().Result;

            var results = JsonConvert.DeserializeObject<List<CompanyPreferenceResponse>>(json);

            Assert.AreEqual(0, results.Count);

            // Should Not return BlacklistedTaxi
            Assert.AreNotEqual(true, results.Any(t => t.CompanyPreference.CompanyKey == "TaxiBlacklisted"));
        }

        [Test]
        public void When_Getting_Network_From_Coordinate()
        {
            var response = Sut.Get(_chrisTaxi.Id,  45.463944, -73.643234);
            Assert.True(response.IsSuccessStatusCode);
            var json = response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<List<NetworkFleetResponse>>(json);
            Assert.IsNotEmpty(result);

            var response3 = Sut.Get(_chrisTaxi.Id);
            Assert.True(response3.IsSuccessStatusCode);
            var json3 = response.Content.ReadAsStringAsync().Result;
            var result3 = JsonConvert.DeserializeObject<List<NetworkFleetResponse>>(json3);
            Assert.AreEqual(1, result3.Count);
            Assert.AreEqual("ChrisTaxiBis", result3[0].CompanyKey);

            var response2 = Sut.Get(_tonyTaxi.Id, 46.359854, -72.575015);
            Assert.True(response2.IsSuccessStatusCode);
            var json2 = response2.Content.ReadAsStringAsync().Result;
            var result2 = JsonConvert.DeserializeObject<List<NetworkFleetResponse>>(json2);
            Assert.IsEmpty(result2);
        }

        [Test]
        public void Post_Preferences()
        {
            var response = Sut.Get(_chrisTaxi.Id);
            var json = response.Content.ReadAsStringAsync().Result;
            var chrisPreferences = JsonConvert.DeserializeObject<List<CompanyPreferenceResponse>>(json);

            var chrisBis = chrisPreferences.FirstOrDefault(p => p.CompanyPreference.CompanyKey == _chrisTaxiBis.Id);
            Assert.NotNull(chrisBis,"Precondition Failed");
            Assert.True(chrisBis.CompanyPreference.CanAccept, "Precondition Failed");
            Assert.True(chrisBis.CompanyPreference.CanDispatch, "Precondition Failed");

            chrisBis.CompanyPreference.CanAccept = chrisBis.CompanyPreference.CanDispatch = false;

            response = Sut.Post(_chrisTaxi.Id, chrisPreferences.Select(x=>x.CompanyPreference).ToArray());
            Assert.True(response.IsSuccessStatusCode);


            response = Sut.Get(_chrisTaxi.Id);
            json = response.Content.ReadAsStringAsync().Result;
            chrisPreferences = JsonConvert.DeserializeObject<List<CompanyPreferenceResponse>>(json);

            chrisBis = chrisPreferences.FirstOrDefault(p => p.CompanyPreference.CompanyKey == _chrisTaxiBis.Id);
            Assert.NotNull(chrisBis);
            Assert.False(chrisBis.CompanyPreference.CanAccept);
            Assert.False(chrisBis.CompanyPreference.CanDispatch);
        }

        private IRepository<Market> GetMockedMarketRepo()
        {
            var marketRepositoryMock = new Mock<IRepository<Market>>();

            var serverMock = MongoMock.CreateMongoServer();
            var databaseMock = MongoMock.CreateMongoDatabase(serverMock.Object);
            var collectionMock = MongoMock.CreateMongoCollection<Market>(databaseMock.Object, "FooCollection");
            var cursorMock = MongoMock.CreateMongoCursor(collectionMock.Object, new List<Market>
            {
                new Market { Name = "MTL", EnableDriverBonus = true, ReceiptFooter = "my custom footer 1", DispatcherSettings = new DispatcherSettings { NumberOfOffersPerCycle = 4, NumberOfCycles = 5, DurationOfOfferInSeconds = 55 }, Region = new MapRegion
                    {
                        CoordinateStart = new MapCoordinate { Latitude = 45.514466, Longitude = -73.846313 }, // MTL Top left 
                        CoordinateEnd = new MapCoordinate { Latitude = 45.411296, Longitude = -73.513314 } // MTL BTM Right
                    }},
                new Market { Name = "NYC", EnableDriverBonus = false, ReceiptFooter = "my custom footer 2", DispatcherSettings = new DispatcherSettings { NumberOfOffersPerCycle = 1, NumberOfCycles = 2, DurationOfOfferInSeconds = 50 }, Region = new MapRegion
                    {
                        CoordinateStart = new MapCoordinate { Latitude = 45.514466, Longitude = -73.889451 },
                        CoordinateEnd = new MapCoordinate { Latitude = 45.411296, Longitude = -73.496042 }
                    }},
                new Market { Name = "NYCSS", EnableDriverBonus = true, ReceiptFooter = "my custom footer 3", DispatcherSettings = new DispatcherSettings { NumberOfOffersPerCycle = 3, NumberOfCycles = 4, DurationOfOfferInSeconds = 60 }, Region = new MapRegion
                    {
                        CoordinateStart = new MapCoordinate { Latitude = 45.563135, Longitude = -73.71953 }, //College Montmorency Laval
                        CoordinateEnd = new MapCoordinate { Latitude = 45.498094, Longitude = -73.62233 } //Station cote des neiges
                    }},
                new Market { Name = "SYD", EnableDriverBonus = true, ReceiptFooter = null, DispatcherSettings = new DispatcherSettings { NumberOfOffersPerCycle = 2, NumberOfCycles = 3, DurationOfOfferInSeconds = 15 }, Region = new MapRegion
                    {
                        CoordinateStart = new MapCoordinate { Latitude = 45.420595, Longitude = -75.708386 }, // Ottawa
                        CoordinateEnd = new MapCoordinate { Latitude = 45.411045, Longitude = -75.684568 }
                    }},
                new Market { Name = "CHI", EnableDriverBonus = false, ReceiptFooter = null, DispatcherSettings = new DispatcherSettings { NumberOfOffersPerCycle = 1, NumberOfCycles = 4, DurationOfOfferInSeconds = 25 }, Region = new MapRegion
                    {
                        CoordinateStart = new MapCoordinate { Latitude = 49994, Longitude = -73.656990 }, // Apcuruium 
                        CoordinateEnd = new MapCoordinate { Latitude = 45.474307, Longitude = -73.58412 } // Home
                    }}
            });
            collectionMock.Setup(x => x.Find(It.IsAny<IMongoQuery>())).Returns(cursorMock.Object);
            collectionMock.Setup(x => x.FindAll()).Returns(cursorMock.Object);
            marketRepositoryMock.Setup(x => x.Collection).Returns(collectionMock.Object);
            return marketRepositoryMock.Object;
        }
    }
}
