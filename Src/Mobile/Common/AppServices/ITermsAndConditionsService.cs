using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface ITermsAndConditionsService
	{
        Task<TermsAndConditions> GetTerms();
	}
}

