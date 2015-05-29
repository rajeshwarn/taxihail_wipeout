using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Configuration;
using CMTPayment.Extensions;
using MK.Common.Android.Entity;
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
            var client = new JsonServiceClient("http://pcls1.craftyclicks.co.uk/json/");

            return client.GetAsync(new CraftyClicksRequest
            {
                PostalCode = postalCode,
                Key = _settingsService.Data.CraftyClicksApiKey
            });
        }
    }
}