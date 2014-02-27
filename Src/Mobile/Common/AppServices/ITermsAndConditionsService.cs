using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface ITermsAndConditionsService
	{
	    Task<string> GetText();
	}
}

