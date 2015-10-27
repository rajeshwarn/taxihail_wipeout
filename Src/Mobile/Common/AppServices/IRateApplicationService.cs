using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IRateApplicationService
	{
		bool IsShowRateApplicationDialog(int successfulTripsNumber);

		void ShowRateApplicationSuggestDialog();

		RateApplicationState CurrentRateApplicationState();
	}
}