using System;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Data;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public interface IAppContext
	{
        Account LoggedUser { get; }
        string LoggedInEmail { get; set; }
        string LoggedInPassword { get; set; }
        void UpdateLoggedInUser(Account data, bool syncWithServer);
        

	}
}

