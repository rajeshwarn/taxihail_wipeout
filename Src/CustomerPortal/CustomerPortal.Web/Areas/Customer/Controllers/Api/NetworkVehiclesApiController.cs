using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using CustomerPortal.Contract.Resources;
using CustomerPortal.Contract.Response;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Entities.Network;
using MongoRepository;
using Newtonsoft.Json;

namespace CustomerPortal.Web.Areas.Customer.Controllers.Api
{
    public class NetworkVehiclesApiController
    {
        private readonly IRepository<NetworkVehicle> _networkVehiclesRepository;
        private readonly IRepository<Company> _companyRepository;

        public NetworkVehiclesApiController()
            : this(new MongoRepository<NetworkVehicle>(), new MongoRepository<Company>())
        {
        }

        public NetworkVehiclesApiController(IRepository<NetworkVehicle> networkVehiclesRepository, IRepository<Company> companyRepository)
        {
            _networkVehiclesRepository = networkVehiclesRepository;
            _companyRepository = companyRepository;
        }

        [Route("api/customer/{companyId}/networkVehicles")]
        public HttpResponseMessage Get(string companyId, string market)
        {
            var company = _companyRepository.GetById(companyId);
            var networkVehicles = _networkVehiclesRepository.Where(v => v.Market == market);
            if (!networkVehicles.Any())
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent); 
            }

            // Select the matching network vehicles for every company vehicles
            var networkVehicleMatches = company.Vehicles.Select(
                companyVehicle => networkVehicles
                    .Where(networkVehicle => networkVehicle.NetworkVehicleId == companyVehicle.NetworkVehicleId)
                    .Select(networkVehicle => new NetworkVehicleResponse
                    {
                        Id = Guid.Parse(networkVehicle.Id),
                        Name = networkVehicle.Name,
                        LogoName = networkVehicle.LogoName,
                        ReferenceDataVehicleId = companyVehicle.ReferenceDataVehicleId,
                        MaxNumberPassengers = networkVehicle.MaxNumberPassengers
                    }))
                    .ToList();

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(networkVehicleMatches))
            };
        }

        [Route("api/customer/{companyId}/companyVehicles")]
        public HttpResponseMessage Post(string companyId, string market, CompanyVehicleType companyVehicleType)
        {
            var company = _companyRepository.GetById(companyId);
            var companyVehicle = company.Vehicles.FirstOrDefault(v => v.Id == companyVehicleType.Id.ToString());

            if (companyVehicle != null)
            {
                // Update existing entry
                companyVehicle.Name = companyVehicle.Name;
                companyVehicle.LogoName = companyVehicleType.LogoName;
                companyVehicle.ReferenceDataVehicleId = companyVehicleType.ReferenceDataVehicleId;
                companyVehicle.NetworkVehicleId = companyVehicleType.NetworkVehicleId;
                companyVehicle.MaxNumberPassengers = companyVehicleType.MaxNumberPassengers;
            }
            else
            {
                // Create new entry
                company.Vehicles.Add(new CompanyVehicle
                {
                    Id = companyVehicleType.Id.ToString(),
                    Name = companyVehicleType.Name,
                    LogoName = companyVehicleType.LogoName,
                    ReferenceDataVehicleId = companyVehicleType.ReferenceDataVehicleId,
                    NetworkVehicleId = companyVehicleType.NetworkVehicleId,
                    MaxNumberPassengers = companyVehicleType.MaxNumberPassengers
                });
            }

            try
            {
                // Save changes
                _companyRepository.Update(company);

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                throw new HttpException((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}
