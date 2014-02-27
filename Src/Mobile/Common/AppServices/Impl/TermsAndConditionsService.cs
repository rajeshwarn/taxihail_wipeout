using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.Data;
using System.IO;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class TermsAndConditionsService : ITermsAndConditionsService
    {
        public async Task<string> GetText()
        {
            var response = await UseServiceClientAsync<CompanyServiceClient, TermsAndConditionsResponse>(service => service.GetTermsAndConditions());
            return response.Content;
        }
    }
}

