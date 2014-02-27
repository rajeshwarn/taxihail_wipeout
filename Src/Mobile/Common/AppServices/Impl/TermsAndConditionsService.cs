using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class TermsAndConditionsService : BaseService, ITermsAndConditionsService
    {
        public async Task<string> GetText()
        {
			return await UseServiceClientAsync<CompanyServiceClient, string>(service => service.GetTermsAndConditions());
        }
    }
}

