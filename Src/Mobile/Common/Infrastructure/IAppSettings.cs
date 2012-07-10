using System;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public interface IAppSettings
	{

        int[] InvalidProviderIds { get; }
		string ServiceUrl { get;}

        

	}
}

