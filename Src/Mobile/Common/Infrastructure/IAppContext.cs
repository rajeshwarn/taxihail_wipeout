using System;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Data;
using Android.Locations;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public interface IAppContext
	{
        Account LoggedUser { get; }
        string LoggedInEmail { get; set; }
        string LoggedInPassword { get; set; }
        void UpdateLoggedInUser(Account data, bool syncWithServer);
		string ServerName { get; set; }
        string ServerVersion { get; }

	}
}

