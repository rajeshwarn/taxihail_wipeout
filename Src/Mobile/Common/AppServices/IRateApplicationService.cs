using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IRateApplicationService
	{
        bool CanShowRateApplicationDialog(int ordersAboveRatingThreshold);

		Task ShowRateApplicationDialog();
	}
}