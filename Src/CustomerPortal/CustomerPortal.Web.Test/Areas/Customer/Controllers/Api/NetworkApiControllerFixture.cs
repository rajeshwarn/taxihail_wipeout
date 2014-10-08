using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using CustomerPortal.Web.Areas.Admin.Models;
using CustomerPortal.Web.Areas.Admin.Models.RequestResponse;
using CustomerPortal.Web.Areas.Customer.Controllers.Api;
using CustomerPortal.Web.Entities.Network;
using CustomerPortal.Web.Test.Helpers.Repository;
using Newtonsoft.Json;
using NUnit.Framework;
using MongoRepository;

namespace CustomerPortal.Web.Test.Areas.Customer.Controllers.Api
{
    public class NetworkApiControllerFixture
    {
        private TaxiHailNetworkSettings _chrisTaxi;
        private TaxiHailNetworkSettings _tonyTaxi;
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

            Repository.DeleteAll();
            Repository.Add(_chrisTaxi);
            Repository.Add(_tonyTaxi);


        }
        public InMemoryRepository<TaxiHailNetworkSettings> Repository { get; set; }
        public NetworkApiController Sut { get; set; }
        
        [Test]
        public void When_Regions_overlap()
        {
            var response = Sut.Get(_chrisTaxi.CompanyId);
             Assert.True(response.IsSuccessStatusCode);
            var json = response.Content.ReadAsStringAsync().Result;
            Assert.IsNotEmpty(JsonConvert.DeserializeObject<List<CompanyPreference>>(json));

            var response2 = Sut.Get(_tonyTaxi.CompanyId);
            Assert.True(response2.IsSuccessStatusCode);
            var json2 = response2.Content.ReadAsStringAsync().Result;
            Assert.IsNotEmpty(JsonConvert.DeserializeObject<List<CompanyPreference>>(json2));

        }

        [Test]
        public void Post_Preferences()
        {
            var response = Sut.Get(_chrisTaxi.CompanyId);
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


            response = Sut.Get(_chrisTaxi.CompanyId);
            json = response.Content.ReadAsStringAsync().Result;
            chrisPreferences = JsonConvert.DeserializeObject<List<CompanyPreference>>(json);

            tony = chrisPreferences.FirstOrDefault(p => p.CompanyId == _tonyTaxi.CompanyId);
            Assert.NotNull(tony);
            Assert.True(tony.CanAccept );
            Assert.True(tony.CanDispatch);
        }

    }
}
