using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Client.TaxiHail;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.Data;
using System.IO;
using ServiceStack.Text;

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

