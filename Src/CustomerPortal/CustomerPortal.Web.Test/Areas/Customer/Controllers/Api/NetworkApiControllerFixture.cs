using System.Collections.Generic;
using System.Linq;
using CustomerPortal.Contract.Requests;
using CustomerPortal.Contract.Resources;
using CustomerPortal.Web.Areas.Customer.Controllers.Api;
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
        const string BaseUrl = "http://localhost/CustomerPortal.Web/";


        [SetUp]
        public void Setup()
        {
            Repository = new InMemoryRepository<TaxiHailNetworkSettings>();

            Sut = new NetworkApiController(Repository);

            _chrisTaxi = new TaxiHailNetworkSettings()
            {
                CompanyId = "ChrisTaxi",
                Id = "ChrisTaxi",
                IsInNetwork = true,
                Region = new MapRegion()
                {
                    CoordinateStart = new MapCoordinate() { Latitude = 45.514466, Longitude = -73.846313 }, // MTL Top left 
                    CoordinateEnd = new MapCoordinate() { Latitude = 45.411296, Longitude = -73.513314 } // MTL BTM Right

                }
            };
            _chrisTaxiBis = new TaxiHailNetworkSettings()
            {
                CompanyId = "ChrisTaxiBis",
                Id = "ChrisTaxiBis",
                IsInNetwork = true,
                Region = new MapRegion()
                {
                    CoordinateStart = new MapCoordinate() { Latitude = 45.514466, Longitude = -73.846313 }, // MTL Top left 
                    CoordinateEnd = new MapCoordinate() { Latitude = 45.411296, Longitude = -73.513314 } // MTL BTM Right

                }
            };

            _tonyTaxi = new TaxiHailNetworkSettings()
            {
                CompanyId = "TonyTaxi",
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
                CompanyId = "TomTaxi",
                Id = "TomTaxi",
                IsInNetwork = true,
                Region = new MapRegion()
                {
                    CoordinateStart = new MapCoordinate() { Latitude = 5000000, Longitude = -73.656990 },  
                    CoordinateEnd = new MapCoordinate() { Latitude = 45.43874, Longitude = -73.584120 } 
                }
            };

            _pilouTaxi = new TaxiHailNetworkSettings()
            {
                //Same Latitude as ChrisTaxi and Chris TaxiBis
                CompanyId = "PilouTaxi",
                Id = "PilouTaxi",
                IsInNetwork = true,
                Region = new MapRegion()
                {
                    CoordinateStart = new MapCoordinate() { Latitude = 45.514466, Longitude = -73.889451 },
                    CoordinateEnd = new MapCoordinate() { Latitude = 45.411296, Longitude = -73.496042 } 
                }
            };

            _lastTaxi = new TaxiHailNetworkSettings()
            {
                //Overlap ChrisTaxi and ChrisTaxiBis
                CompanyId = "LastTaxi",
                Id = "LastTaxi",
                IsInNetwork = true,
                Region = new MapRegion()
                {
                    CoordinateStart = new MapCoordinate() { Latitude = 45.563135, Longitude = -73.719532 }, //College Montmorency Laval
                    CoordinateEnd = new MapCoordinate() { Latitude = 45.498094, Longitude = -73.622335 } //Station cote des neiges
                }
            };

            Repository.DeleteAll();
            Repository.Add(_chrisTaxi);
            Repository.Add(_chrisTaxiBis);
            Repository.Add(_tonyTaxi);
            Repository.Add(_tomTaxi);
            Repository.Add(_pilouTaxi);
            Repository.Add(_lastTaxi);


        }
        public InMemoryRepository<TaxiHailNetworkSettings> Repository { get; set; }
        public NetworkApiController Sut { get; set; }
        
        [Test]
        public void When_Regions_Inside()
        {

            var response = Sut.Get(new PostCompanyPreferencesRequest { CompanyId = _chrisTaxi.CompanyId });
             Assert.True(response.IsSuccessStatusCode);
            var json = response.Content.ReadAsStringAsync().Result;
            Assert.IsNotEmpty(JsonConvert.DeserializeObject<List<CompanyPreference>>(json));

            var response2 = Sut.Get(new PostCompanyPreferencesRequest { CompanyId = _tonyTaxi.CompanyId });
            Assert.True(response2.IsSuccessStatusCode);
            var json2 = response2.Content.ReadAsStringAsync().Result;
            Assert.IsNotEmpty(JsonConvert.DeserializeObject<List<CompanyPreference>>(json2));
        }


        [Test]
        public void When_Regions_Identical()
        {
            //should Not return ChrisTaxi
            var response = Sut.Get(new PostCompanyPreferencesRequest { CompanyId = _chrisTaxi.CompanyId });
            Assert.True(response.IsSuccessStatusCode);
            var json = response.Content.ReadAsStringAsync().Result;
            Assert.IsNotEmpty(JsonConvert.DeserializeObject<List<CompanyPreference>>(json));
        }

        [Test]
        public void When_Regions_Overlap()
        {
            var response = Sut.Get(new PostCompanyPreferencesRequest { CompanyId = _lastTaxi.CompanyId });
            Assert.True(response.IsSuccessStatusCode);
            var json = response.Content.ReadAsStringAsync().Result;
            Assert.IsNotEmpty(JsonConvert.DeserializeObject<List<CompanyPreference>>(json));
        }

        [Test]
        public void When_Regions_Overlap_Same_Latitude()
        {
            var response = Sut.Get(new PostCompanyPreferencesRequest { CompanyId = _pilouTaxi.CompanyId });
            Assert.True(response.IsSuccessStatusCode);
            var json = response.Content.ReadAsStringAsync().Result;
            Assert.IsNotEmpty(JsonConvert.DeserializeObject<List<CompanyPreference>>(json));
        }

        [Test]
        public void When_Regions_Overlap_Same_Longitude()
        {
            var response = Sut.Get(new PostCompanyPreferencesRequest { CompanyId = _tonyTaxi.CompanyId });
            Assert.True(response.IsSuccessStatusCode);
            var json = response.Content.ReadAsStringAsync().Result;
            Assert.IsNotEmpty(JsonConvert.DeserializeObject<List<CompanyPreference>>(json));
        }

        [Test]
        public void When_Regions_No_Overlap()
        {
            //Should return nothing
            var response = Sut.Get(new PostCompanyPreferencesRequest { CompanyId = _chrisTaxi.CompanyId });
            Assert.True(response.IsSuccessStatusCode);
            var json = response.Content.ReadAsStringAsync().Result;
            Assert.IsNotEmpty(JsonConvert.DeserializeObject<List<CompanyPreference>>(json));
        }

        [Test]
        public void Post_Preferences()
        {
            var response = Sut.Get(new PostCompanyPreferencesRequest { CompanyId = _chrisTaxi.CompanyId });
            var json = response.Content.ReadAsStringAsync().Result;
            var chrisPreferences = JsonConvert.DeserializeObject<List<CompanyPreference>>(json);

            var tony = chrisPreferences.FirstOrDefault(p => p.CompanyId == _tonyTaxi.CompanyId);
            Assert.NotNull(tony,"Precondition Failed");
            Assert.False(tony.CanAccept, "Precondition Failed");
            Assert.False(tony.CanDispatch, "Precondition Failed");

            tony.CanAccept = tony.CanDispatch = true;

            response = Sut.Post(new PostCompanyPreferencesRequest()
            {
                CompanyId = _chrisTaxi.CompanyId,
                Preferences = chrisPreferences.ToArray()
            });
            Assert.True(response.IsSuccessStatusCode);


            response = Sut.Get(new PostCompanyPreferencesRequest { CompanyId = _chrisTaxi.CompanyId });
            json = response.Content.ReadAsStringAsync().Result;
            chrisPreferences = JsonConvert.DeserializeObject<List<CompanyPreference>>(json);

            tony = chrisPreferences.FirstOrDefault(p => p.CompanyId == _tonyTaxi.CompanyId);
            Assert.NotNull(tony);
            Assert.True(tony.CanAccept );
            Assert.True(tony.CanDispatch);
        }

    }
}
