using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.Extensions;
using apcurium.MK.Booking.Api.Contract.Requests;

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class CompanyServiceClient : BaseServiceClient
    {
        public CompanyServiceClient(string url, string sessionId, string userAgent) 
            : base(url, sessionId, userAgent)
        {
        }

        public async Task<string> GetTermsAndConditions()
        {
            var response = await Client.GetAsync(new TermsAndConditionsRequest());
            return response.Content;
        }
    }
}