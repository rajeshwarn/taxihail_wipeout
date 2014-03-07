using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
	public class TermsAndConditionsService : BaseService, ITermsAndConditionsService
    {
        public Task<TermsAndConditions> GetTerms()
        {
            return UseServiceClientAsync<CompanyServiceClient, TermsAndConditions>(service => service.GetTermsAndConditions());
        }
    }
}

