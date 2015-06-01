using System;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using CMTPayment.Extensions;
using ServiceStack.ServiceClient.Web;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class CraftyClicksServiceClient : ICraftyClicksServiceClient
    {
        private readonly IAppSettings _settingsService;

        public CraftyClicksServiceClient(IAppSettings settingsService)
        {
            _settingsService = settingsService;
        }

        public Task<CraftyClicksAddress> GetAddressInformation(string postalCode)
        {
            try
            {
                var client = new JsonServiceClient("http://pcls1.craftyclicks.co.uk/json/");

            
                return client.PostAsync(new CraftyClicksRequest
                {
                    Postcode = postalCode,
                    Key = _settingsService.Data.CraftyClicksApiKey
                });
            }
            catch (Exception ex)
            {
                
                throw;
            }
            
        }
    }
}