using System;

namespace apcurium.MK.Booking.Mobile.Infrastructure
{
	public interface IAppSettings
	{

        int[] InvalidProviderIds { get; }
        string ServiceUrl { get; set; }
        bool ErrorLogEnabled{ get; }
        bool CanChangeServiceUrl { get; }
        
        string ErrorLog{ get; }
        string SiteUrl{ get; }
        string PhoneNumber(int providerId);
        string PhoneNumberDisplay(int companyId);
        string DefaultServiceUrl{ get; }

        bool TwitterEnabled{ get; }        
        string TwitterConsumerKey{ get; }
        string TwitterCallback{ get; }
        string TwitterConsumerSecret{ get; }
        string TwitterRequestTokenUrl{ get; }
        string TwitterAccessTokenUrl{ get; }
        string TwitterAuthorizeUrl { get; }

        bool FacebookEnabled { get; }
        string FacebookAppId{ get; }

        string SupportEmail { get; }
        


	}
}

