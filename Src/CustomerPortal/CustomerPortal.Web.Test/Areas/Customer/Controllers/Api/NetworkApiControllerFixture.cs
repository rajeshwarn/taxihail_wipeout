using System.Collections.Generic;
using System.Linq;
using CustomerPortal.Client.Impl;
using CustomerPortal.Contract.Resources;
using CustomerPortal.Contract.Response;
using CustomerPortal.Web.Areas.Customer.Controllers.Api;
using CustomerPortal.Web.Areas.Customer.Models.RequestResponse;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Entities.Network;
using CustomerPortal.Web.Test.Helpers.Repository;
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

        private Company _chrisTaxiCompany;
        private Company _chrisTaxiBisCompany;
        private Company _tonyTaxiCompany;
        private Company _tomTaxiCompany;
        private Company _pilouTaxiCompany;
        private Company _lastTaxiCompany;

        const string BaseUrl = "http://localhost/CustomerPortal.Web/";


        [SetUp]
        public void Setup()
        {
            NetworkRepository = new InMemoryRepository<TaxiHailNetworkSettings>();
            CompanyRepository = new InMemoryRepository<Company>();

            Sut = new NetworkApiController(NetworkRepository, CompanyRepository);

            _chrisTaxi = new TaxiHailNetworkSettings()
            {
                Id = "ChrisTaxi",
                IsInNetwork = true,
                Region = new MapRegion()
                {
                    CoordinateStart = new MapCoordinate() { Latitude = 45.514466, Longitude = -73.846313 }, // MTL Top left 
                    CoordinateEnd = new MapCoordinate() { Latitude = 45.411296, Longitude = -73.513314 } // MTL BTM Right
                },
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
                Region = new MapRegion()
                {
                    CoordinateStart = new MapCoordinate() { Latitude = 45.514466, Longitude = -73.846313 }, // MTL Top left 
                    CoordinateEnd = new MapCoordinate() { Latitude = 45.411296, Longitude = -73.513314 } // MTL BTM Right

                },
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
                Region = new MapRegion()
                {
                    CoordinateStart = new MapCoordinate() { Latitude = 499940, Longitude = -73.656990 }, // Apcuruium 
                    CoordinateEnd = new MapCoordinate() { Latitude = 45.474307, Longitude = -73.584120 } // Home
                }
            };

            _tomTaxi = new TaxiHailNetworkSettings()
            {
                //same Longitude as TonyTaxi
                Id = "TomTaxi",
                IsInNetwork = true,
                Region = new MapRegion()
                {
                    CoordinateStart = new MapCoordinate() { Latitude = 5000000, Longitude = -73.656990 },  
                    CoordinateEnd = new MapCoordinate() { Latitude = 45.43874, Longitude = -73.584120 }
                },
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
                Region = new MapRegion()
                {
                    CoordinateStart = new MapCoordinate() { Latitude = 45.514466, Longitude = -73.889451 },
                    CoordinateEnd = new MapCoordinate() { Latitude = 45.411296, Longitude = -73.496042 }
                },
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
                Region = new MapRegion()
                {
                    CoordinateStart = new MapCoordinate() { Latitude = 45.563135, Longitude = -73.719532 }, //College Montmorency Laval
                    CoordinateEnd = new MapCoordinate() { Latitude = 45.498094, Longitude = -73.622335 } //Station cote des neiges
                }
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

            NetworkRepository.DeleteAll();
            NetworkRepository.Add(_chrisTaxi);
            NetworkRepository.Add(_chrisTaxiBis);
            NetworkRepository.Add(_tonyTaxi);
            NetworkRepository.Add(_tomTaxi);
            NetworkRepository.Add(_pilouTaxi);
            NetworkRepository.Add(_lastTaxi);

            CompanyRepository.DeleteAll();
            CompanyRepository.Add(_chrisTaxiCompany);
            CompanyRepository.Add(_chrisTaxiBisCompany);
            CompanyRepository.Add(_tonyTaxiCompany);
            CompanyRepository.Add(_tomTaxiCompany);
            CompanyRepository.Add(_pilouTaxiCompany);
            CompanyRepository.Add(_lastTaxiCompany);
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
            //should Not return ChrisTaxi
            var response = Sut.Get(_chrisTaxi.Id);
            Assert.True(response.IsSuccessStatusCode);
            var json = response.Content.ReadAsStringAsync().Result;
            Assert.IsNotEmpty(JsonConvert.DeserializeObject<List<CompanyPreferenceResponse>>(json));
        }

        [Test]
        public void When_Regions_Overlap()
        {
            var response = Sut.Get(_lastTaxi.Id);
            Assert.True(response.IsSuccessStatusCode);
            var json = response.Content.ReadAsStringAsync().Result;
            Assert.IsNotEmpty(JsonConvert.DeserializeObject<List<CompanyPreferenceResponse>>(json));
        }

        [Test]
        public void When_Regions_Overlap_Same_Latitude()
        {
            var response = Sut.Get(_pilouTaxi.Id);
            Assert.True(response.IsSuccessStatusCode);
            var json = response.Content.ReadAsStringAsync().Result;
            Assert.IsNotEmpty(JsonConvert.DeserializeObject<List<CompanyPreferenceResponse>>(json));
        }

        [Test]
        public void When_Regions_Overlap_Same_Longitude()
        {
            var response = Sut.Get(_tonyTaxi.Id);
            Assert.True(response.IsSuccessStatusCode);
            var json = response.Content.ReadAsStringAsync().Result;
            Assert.IsNotEmpty(JsonConvert.DeserializeObject<List<CompanyPreferenceResponse>>(json));
        }

        [Test]
        public void When_Regions_No_Overlap()
        {
            //Should return nothing
            var response = Sut.Get(_chrisTaxi.Id);
            Assert.True(response.IsSuccessStatusCode);
            var json = response.Content.ReadAsStringAsync().Result;
            Assert.IsNotEmpty(JsonConvert.DeserializeObject<List<CompanyPreferenceResponse>>(json));
        }

        [Test]
        public void When_Getting_Network_From_Coordinate()
        {
            var position = new MapCoordinate
            {
                Latitude = 45.463944,
                Longitude = -73.643234
            };

            var response = Sut.Get(_chrisTaxi.Id, position);
            Assert.True(response.IsSuccessStatusCode);
            var json = response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<List<NetworkFleetResponse>>(json);
            Assert.IsNotEmpty(JsonConvert.DeserializeObject<List<NetworkFleetResponse>>(json));

            var response3 = Sut.Get(_chrisTaxi.Id);
            Assert.True(response3.IsSuccessStatusCode);
            var json3 = response.Content.ReadAsStringAsync().Result;
            var result3 = JsonConvert.DeserializeObject<List<NetworkFleetResponse>>(json3);
            Assert.IsNotEmpty(JsonConvert.DeserializeObject<List<NetworkFleetResponse>>(json3));

            var position2 = new MapCoordinate
            {
                Latitude = 46.359854,
                Longitude = -72.575015
            };
            var response2 = Sut.Get(_tonyTaxi.Id, position2);
            Assert.True(response2.IsSuccessStatusCode);
            var json2 = response2.Content.ReadAsStringAsync().Result;
            Assert.IsEmpty(JsonConvert.DeserializeObject<List<NetworkFleetResponse>>(json2));

        }

        [Test]
        public void Post_Preferences()
        {
            var response = Sut.Get(_chrisTaxi.Id);
            var json = response.Content.ReadAsStringAsync().Result;
            var chrisPreferences = JsonConvert.DeserializeObject<List<CompanyPreferenceResponse>>(json);

            var tony = chrisPreferences.FirstOrDefault(p => p.CompanyPreference.CompanyKey == _tonyTaxi.Id);
            Assert.NotNull(tony,"Precondition Failed");
            Assert.False(tony.CompanyPreference.CanAccept, "Precondition Failed");
            Assert.False(tony.CompanyPreference.CanDispatch, "Precondition Failed");

            tony.CompanyPreference.CanAccept = tony.CompanyPreference.CanDispatch = true;

            response = Sut.Post(_chrisTaxi.Id, chrisPreferences.Select(x=>x.CompanyPreference).ToArray());
            Assert.True(response.IsSuccessStatusCode);


            response = Sut.Get(_chrisTaxi.Id);
            json = response.Content.ReadAsStringAsync().Result;
            chrisPreferences = JsonConvert.DeserializeObject<List<CompanyPreferenceResponse>>(json);

            tony = chrisPreferences.FirstOrDefault(p => p.CompanyPreference.CompanyKey == _tonyTaxi.Id);
            Assert.NotNull(tony);
            Assert.True(tony.CompanyPreference.CanAccept);
            Assert.True(tony.CompanyPreference.CanDispatch);
        }

    }
}
