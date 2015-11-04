using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using apcurium.MK.Common.Extensions;
using CustomerPortal.Contract.Resources;
using CustomerPortal.Contract.Response;
using CustomerPortal.Web.Entities;
using CustomerPortal.Web.Entities.Network;
using MongoRepository;
using Newtonsoft.Json;

namespace CustomerPortal.Web.Areas.Customer.Controllers.Api
{
    public class NetworkVehiclesApiController : ApiController
    {
        private readonly IRepository<Market> _marketRepository;
        private readonly IRepository<TaxiHailNetworkSettings> _taxiHailNetworkRepository;
        private readonly IRepository<Company> _companyRepository;

        public NetworkVehiclesApiController()
            : this(new MongoRepository<Market>(),
                   new MongoRepository<Company>(),
                   new MongoRepository<TaxiHailNetworkSettings>())
        {
        }

        public NetworkVehiclesApiController(
            IRepository<Market> marketRepository,
            IRepository<Company> companyRepository,
            IRepository<TaxiHailNetworkSettings> taxiHailNetworkRepository)
        {
            _marketRepository = marketRepository;
            _companyRepository = companyRepository;
            _taxiHailNetworkRepository = taxiHailNetworkRepository;
        }

        [Route("api/customer/marketVehicleTypes")]
        public HttpResponseMessage GetMarketVehicleTypes(string companyId = null, string market = null)
        {
            if (companyId == null && market == null)
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, "You must specify at least either the Market or the CompanyId.");
            }

            var vehiclesMarket = market;

            if (companyId != null)
            {
                // Use market from specified company
                var taxiHailNetworkSettings = _taxiHailNetworkRepository.FirstOrDefault(n => n.Id == companyId);
                if (taxiHailNetworkSettings == null)
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(JsonConvert.SerializeObject(new NetworkVehicleResponse[0]))
                    };
                }

                vehiclesMarket = taxiHailNetworkSettings.Market;
            }

            var marketRepresentation = _marketRepository.FirstOrDefault(x => x.Name == vehiclesMarket);
            if (marketRepresentation == null)
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new NetworkVehicleResponse[0]))
                };
            }

            var response = marketRepresentation.Vehicles.Select(v => new NetworkVehicleResponse
            {
                Id = Guid.Parse(v.Id),
                Name = v.Name,
                LogoName = v.LogoName,
                ReferenceDataVehicleId = v.NetworkVehicleId,
                MaxNumberPassengers = v.MaxNumberPassengers
            });

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(response))
            };
        }

        [Route("api/customer/{companyId}/associatedMarketVehicleType")]
        public HttpResponseMessage GetAssociatedMarketVehicleTypes(string companyId, int networkVehicleId)
        {
            var taxiHailNetworkSettings = _taxiHailNetworkRepository.FirstOrDefault(n => n.Id == companyId);
            if (taxiHailNetworkSettings == null)
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            var marketRepresentation = _marketRepository.FirstOrDefault(x => x.Name == taxiHailNetworkSettings.Market);
            if (marketRepresentation == null || !marketRepresentation.Vehicles.Any())
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new NetworkVehicleResponse[0]))
                };
            }

            var company = _companyRepository.GetById(companyId);

            // Select the matching company vehicle
            var companyVehicleMatch = company.Vehicles
                .FirstOrDefault(companyVehicle => companyVehicle.NetworkVehicleId == networkVehicleId)
                .SelectOrDefault(companyVehicle => new NetworkVehicleResponse
                {
                    Id = Guid.Parse(companyVehicle.Id),
                    Name = companyVehicle.Name,
                    LogoName = companyVehicle.LogoName,
                    ReferenceDataVehicleId = companyVehicle.ReferenceDataVehicleId,
                    MaxNumberPassengers = companyVehicle.MaxNumberPassengers
                });
            
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(companyVehicleMatch))
            };
        }

        [Route("api/customer/{companyId}/companyVehicles")]
        public HttpResponseMessage UpsertCompanyVehicleType(string companyId, CompanyVehicleType companyVehicleType)
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

        [Route("api/customer/{companyId}/companyVehicles")]
        public HttpResponseMessage DeleteCompanyVehicleType(string companyId, Guid vehicleTypeId)
        {
            var company = _companyRepository.GetById(companyId);
            company.Vehicles.Remove(v => v.Id == vehicleTypeId.ToString());

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