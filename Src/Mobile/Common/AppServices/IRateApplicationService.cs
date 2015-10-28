using apcurium.MK.Common.Enumeration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IRateApplicationService
	{
		bool CanShowRateApplicationDialog(int successfulTripsNumber);

		void ShowRateApplicationDialog();

		RateApplicationState CurrentRateApplicationState();
	}
}